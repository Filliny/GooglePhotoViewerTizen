using App4.Models;
using App4.ViewModels;
using Google.Apis.Auth.OAuth2;
using Google.Apis.PhotosLibrary.v1;
using Google.Apis.Services;
using System;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using App4.Services;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;



namespace App4.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class ItemsPage : ContentPage
    {
        ItemsViewModel viewModel;

        UserCredential credential;

        public ItemsPage()
        {
            InitializeComponent();
            
            BindingContext = this.viewModel = new ItemsViewModel();
            
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

            if (viewModel.Items.Count == 0)
               viewModel.IsBusy = true;

            //if (viewModel.PhotoCollection.Count == 0)
            //     Run();
        }

        private  void  Button_OnClicked(object sender, EventArgs e)
        {
            Run();
           
        }



        private async Task Run()
        {
            var folders = Environment.SpecialFolder.ApplicationData;

            viewModel.IsBusy = true;
            viewModel.PhotoCollection.Clear();
            

            try
            {
                
                
                var browser = new EmbeddedBrowser("Login to google");
                browser.Cancelled += (s, evt) => { Navigation.PopModalAsync(); };
               // var unused = page.PushAsync(browser);
                var unused = Navigation.PushModalAsync(browser);
                
                var currdir = DependencyService.Get<ICurrentDir>().GetCurrent();

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
            var service = new PhotosLibraryService(new BaseClientService.Initializer()
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
                
            }
            
            


            
        }




    }
}