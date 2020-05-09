using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Logging;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;


namespace App4
{
    public interface IEmbeddedBrowser
    {
        event EventHandler Cancelled;
        void Start(string url);
    }
    class ConsoleLogger : ILogger
    {
        public bool IsDebugEnabled => false;

        public void Debug(string message, params object[] formatArgs)
        {
            Console.WriteLine(message, formatArgs);
        }

        public void Error(Exception exception, string message, params object[] formatArgs)
        {
            Debug(message, formatArgs);
        }

        public void Error(string message, params object[] formatArgs)
        {
            Debug(message, formatArgs);
        }

        public ILogger ForType(Type type)
        {
            return this;
        }

        public ILogger ForType<T>()
        {
            return this;
        }

        public void Info(string message, params object[] formatArgs)
        {
            Debug(message, formatArgs);
        }

        public void Warning(string message, params object[] formatArgs)
        {
            Debug(message, formatArgs);
        }
    }


    public class TizenLocalServerCodeReceiver : ICodeReceiver
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        /// <summary>The call back request path.</summary>
        internal const string LoopbackCallbackPath = "/authorize/";

        /// <summary>Localhost callback URI, expects a port parameter.</summary>
        internal static readonly string CallbackUriTemplateLocalhost = $"http://localhosts:{{0}}{LoopbackCallbackPath}";
        /// <summary>127.0.0.1 callback URI, expects a port parameter.</summary>
        internal static readonly string CallbackUriTemplate127001 = $"http://127.0.0.1:{{0}}{LoopbackCallbackPath}";

        /// <summary>Close HTML tag to return the browser so it will close itself.</summary>
        internal const string DefaultClosePageResponse =
@"<html>
  <head><title>OAuth 2.0 Authentication Token Received</title></head>
  <body>
    Received verification code. You may now close this window.
    <script type='text/javascript'>
      // This doesn't work on every browser.
      window.setTimeout(function() {
          this.focus();
          window.opener = this;
          window.open('', '_self', ''); 
          window.close(); 
        }, 1000);
      //if (window.opener) { window.opener.checkToken(); }
    </script>
  </body>
</html>";

        private static object s_lock = new object();
        // Current best-guess as to most suitable callback URI.
        private static string s_callbackUriTemplate;
        // Has a successful callback been received?
        private static bool s_receivedCallback;

        /// <summary>
        /// Create an instance of <see cref="TizenLocalServerCodeReceiver"/>.
        /// </summary>
        public TizenLocalServerCodeReceiver() : this(DefaultClosePageResponse) { }

        /// <summary>
        /// Create an instance of <see cref="TizenLocalServerCodeReceiver"/>.
        /// </summary>
        /// <param name="closePageResponse">Custom close page response for this instance</param>
        public TizenLocalServerCodeReceiver(string closePageResponse)
        {
            _closePageResponse = closePageResponse;

            lock (s_lock)
            {
                // Listening on 127.0.0.1 is recommended, but can't be done in non-admin Windows 7 & 8.
                // So use some tests/heuristics so maybe listen on localhost instead.
                if (s_callbackUriTemplate != null && !s_receivedCallback)
                {
                    // On non-first runs, if a successful callback has not been received
                    // then force callback to use localhost rather than 127.0.0.1
                    // This is to heuristically avoid errors on browsers that can't connect to 127.0.0.1
                    // E.g. IE11 in enhanced protection mode.
                    s_callbackUriTemplate = CallbackUriTemplateLocalhost;
                }
                if (s_callbackUriTemplate == null)
                {
                    s_callbackUriTemplate = CallbackUriTemplate127001;
                    // No check required on NETStandard, it uses TcpListener which can only use IP adddresses, not DNS names.
                }
                // Set the instance field of which callback URI to use.
                // An instance field is used to ensure any one instance of this class
                // uses a consistent callback URI.
                 _callbackUriTemplate = s_callbackUriTemplate;
                //_callbackUriTemplate = $"http://ca9be1f9.eu.ngrok.io:{0}/authorize/";
            }
        }

        // Callback URI used for this instance.
        private string _callbackUriTemplate;

        // Close page response for this instance.
        private readonly string _closePageResponse;

        public IEmbeddedBrowser EmbeddedBrowser { get; set; }


        // There is a race condition on the port used for the loopback callback.
        // This is not good, but is now difficult to change due to RedirectUri and ReceiveCodeAsync
        // being public methods.

        private string redirectUri;
        /// <inheritdoc />
        public string RedirectUri
        {
            get
            {
                if (string.IsNullOrEmpty(redirectUri))
                {
                   redirectUri = string.Format(_callbackUriTemplate, GetRandomUnusedPort());
                   //redirectUri = "http://ca9be1f9.eu.ngrok.io/authorize/";
                }
                return redirectUri;
            }
        }

        /// <inheritdoc />
        public async Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url,
            CancellationToken taskCancellationToken)
        {
            var authorizationUrl = url.Build().AbsoluteUri;
            // The listener type depends on platform:
            // * .NET desktop: System.Net.HttpListener
            // * .NET Core: LimitedLocalhostHttpServer (above, HttpListener is not available in any version of netstandard)
            using (var listener = StartListener())
            {
                Logger.Debug("Open a browser with \"{0}\" URL", authorizationUrl);
                bool browserOpenedOk;
                try
                {
                     //browserOpenedOk = OpenBrowser(RedirectUri);
                    browserOpenedOk = OpenBrowser(authorizationUrl);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Failed to launch browser with \"{0}\" for authorization", authorizationUrl);
                    throw new NotSupportedException(
                        $"Failed to launch browser with \"{authorizationUrl}\" for authorization. See inner exception for details.", e);
                }
                if (!browserOpenedOk)
                {
                    Logger.Error("Failed to launch browser with \"{0}\" for authorization; platform not supported.", authorizationUrl);
                    throw new NotSupportedException(
                        $"Failed to launch browser with \"{authorizationUrl}\" for authorization; platform not supported.");
                }

                var ret = await GetResponseFromListener(listener, taskCancellationToken).ConfigureAwait(false);

                // Note that a successful callback has been received.
                s_receivedCallback = true;

                return ret;
            }
        }

        /// <summary>Returns a random, unused port.</summary>
        private static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            try
            {
                listener.Start();
                return ((IPEndPoint)listener.LocalEndpoint).Port;
            }
            finally
            {
                listener.Stop();
            }
        }
        private HttpListener StartListener()
        {
            try
            {
                var listener = new HttpListener();
                //listener.Prefixes.Add("http://127.0.0.1:8087/authorize/");
                listener.Prefixes.Add(RedirectUri);
                listener.Start();
                return listener;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private async Task<AuthorizationCodeResponseUrl> GetResponseFromListener(HttpListener listener, CancellationToken ct)
        {
            HttpListenerContext context;
            // Set up cancellation. HttpListener.GetContextAsync() doesn't accept a cancellation token,
            // the HttpListener needs to be stopped which immediately aborts the GetContextAsync() call.
            using (ct.Register(listener.Stop))
            {
                // Wait to get the authorization code response.
                try
                {
                    context = await listener.GetContextAsync().ConfigureAwait(false);
                }
                catch (Exception) when (ct.IsCancellationRequested)
                {
                    ct.ThrowIfCancellationRequested();
                    // Next line will never be reached because cancellation will always have been requested in this catch block.
                    // But it's required to satisfy compiler.
                    throw new InvalidOperationException();
                }
            }
            NameValueCollection coll = context.Request.QueryString;

            // Write a "close" response.
            var bytes = Encoding.UTF8.GetBytes(_closePageResponse);
            context.Response.ContentLength64 = bytes.Length;
            context.Response.SendChunked = false;
            context.Response.KeepAlive = false;
            var output = context.Response.OutputStream;
            await output.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            await output.FlushAsync().ConfigureAwait(false);
            output.Close();
            context.Response.Close();

            // Create a new response URL with a dictionary that contains all the response query parameters.
            return new AuthorizationCodeResponseUrl(coll.AllKeys.ToDictionary(k => k, k => coll[k]));
        }

        private bool OpenBrowser(string url)
        {
            EmbeddedBrowser.Start(url);
            //Launcher.OpenAsync(url);
            return true;
        }
    }
}