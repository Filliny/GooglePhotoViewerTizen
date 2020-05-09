using App4.Models;
using App4.ViewModels;
using Google.Apis.Auth.OAuth2;
using Google.Apis.PhotosLibrary.v1;
using Google.Apis.Services;
using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using App4.Services;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Tizen.Multimedia;



namespace App4.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class ItemsPage : ContentPage
    {
        public PhotosLibraryService service;
        
        ItemsViewModel viewModel;

        UserCredential credential;

        private Player radioPlayer;

       public ItemsPage()
        {
            InitializeComponent();
            
            BindingContext = this.viewModel = new ItemsViewModel();


            MediaUriSource radioSource = new MediaUriSource("http://cast.loungefm.com.ua/loungefm");
            radioPlayer = new Player();
            radioPlayer.SetSource(radioSource);
            radioPlayer.BufferingTime = new PlayerBufferingTime(10000, 10000);

            radioPlayer.BufferingProgressChanged += PlayerStart;
            CollectionViewMain.EmptyView = new LoadingView();
            this.Appearing += StartLoad;
        }

        void PlayerStart(object o, EventArgs e)
        {
            if (radioPlayer.State == PlayerState.Ready)
            {
                radioPlayer.IsAudioOnly = true;
                radioPlayer.Start();
            }

        }

        async void OnItemSelected(object sender, EventArgs args)
        {
            var layout = (BindableObject)sender;
            var item = (ItemInst)layout.BindingContext;
            await Navigation.PushAsync(new ItemDetailPage(new ItemDetailViewModel(item)));
            
        }

        async void AddItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NavigationPage(new NewItemPage()));
        }

        protected override void OnAppearing()
        {
            
            base.OnAppearing();

            //if (viewModel.Items.Count == 0) 
            //    viewModel.IsBusy = true;

            //if (viewModel.PhotoCollection.Count == 0 )
            //{
            //    if (radioPlayer.State == PlayerState.Idle)
            //        radioPlayer.PrepareAsync();
            //    Run();
            //}

            

        }

        private  void  Button_OnClicked(object sender, EventArgs e)
        {
            if (radioPlayer.State == PlayerState.Idle)
                radioPlayer.PrepareAsync();

            Run();
            
        }

        private void StartLoad(object sender, EventArgs e)
        {
            if (radioPlayer.State == PlayerState.Idle)
                radioPlayer.PrepareAsync();

            if (viewModel.PhotoCollection.Count == 0)
            {

                //GoogleAuth();
                Run();

            }
             
        }


        //Not in viewmodel cos  Navigation available only for content page and we need to create Browser view to show 
        //in authentication process
        private async Task Run() 
        {
            var folders = Environment.SpecialFolder.ApplicationData;

            viewModel.IsBusy = true;
            viewModel.PhotoCollection.Clear();
            this.Appearing -= StartLoad;

            try
            {
                //var responcePage = new ResponcePage();
                //var unused = Navigation.PushModalAsync(responcePage);

                var browser = new EmbeddedBrowser("Login to google");
                browser.Cancelled += (s, evt) => { Navigation.PopModalAsync(); };
                
                var unused = Navigation.PushModalAsync(browser);
                var currdir = DependencyService.Get<ICurrentDir>().GetCurrent();
               // var currdir = "/opt/usr/apps/org.tizen.example.TizenXamlApp1.TV/res/";

                using (var stream = new FileStream(currdir+"client_secrets.json", FileMode.Open, FileAccess.Read)){

                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        new[] { PhotosLibraryService.Scope.Photoslibrary },
                        "user",
                        CancellationToken.None,
                        codeReceiver: new TizenLocalServerCodeReceiver { EmbeddedBrowser = browser });
                }
                
                unused = Navigation.PopModalAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            

            // Create the service.
            service = new PhotosLibraryService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "PhotosViewerTizen",
            });

            var request = service.SharedAlbums.List();
            request.PageSize = 50;


            try
            {

                var bookshelves = await request.ExecuteAsync();

                var she = bookshelves.SharedAlbums;


                foreach (var album in she)
                {
                    if (album != null && album.Title != null)
                    {
                        string test = album.Title;
                        await viewModel.AddOne(album);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            finally
            {   
                
                
                viewModel.IsBusy = false;
                TopLabel.Text = "My Albums";
                this.Appearing += StartLoad;
            }
            
        }


        private async void ClickGestureRecognizer_OnClicked(object sender, EventArgs e)
        {
            var layout = (BindableObject)sender;
            var item = (Album)layout.BindingContext;
            await Navigation.PushModalAsync(new AlbumPage(new AlbumViewModel(service, item)));
        }


        private async void GoogleAuth()
        {

            var currdir = "/opt/usr/apps/org.tizen.example.TizenXamlApp1.TV/res/";

            using (var stream = new FileStream(currdir + "client_secrets.json", FileMode.Open, FileAccess.Read))
            {

                GoogleAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(
                    new GoogleAuthorizationCodeFlow.Initializer()
                    {ClientSecretsStream = stream,
                        Scopes = new string[] { "https://www.googleapis.com/auth/photoslibrary" }

                    }
                );


                var request =  flow.CreateAuthorizationCodeRequest("http://localhost").Build();
                var path = request.AbsoluteUri;

                Page responcePage = new ResponcePage();
                
               await Navigation.PushModalAsync(responcePage);

               

            }
        }




    }
}