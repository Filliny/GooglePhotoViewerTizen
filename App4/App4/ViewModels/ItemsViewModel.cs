using App4.Models;
using App4.Views;
using Google.Apis.Auth.OAuth2;
using Google.Apis.PhotosLibrary.v1;
using Google.Apis.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace App4.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        
        public ObservableCollection<ItemInst> Items { get; set; }
        public Command LoadItemsCommand { get; set; }
        public ObservableCollection<Album> PhotoCollection { get; set; }

        public ItemsViewModel()
        {
            Title = "Browse";
            Items = new ObservableCollection<ItemInst>();
            PhotoCollection = new ObservableCollection<Album>();
           
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            MessagingCenter.Subscribe<NewItemPage, ItemInst>(this, "AddItem", async (obj, item) =>
            {
                var newItem = item as ItemInst;
                Items.Add(newItem);
                await DataStore.AddItemAsync(newItem);
            });

            
        }

        async Task ExecuteLoadItemsCommand()
        {
            
            
            IsBusy = true;

            try
            {



                Items.Clear();
                var items = await DataStore.GetItemsAsync(true);
                foreach (var item in items)
                {

                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task AddOne(Google.Apis.PhotosLibrary.v1.Data.Album to_add)
        {
            Album tempAlbum = new Album(){Id = to_add.Id, AlbumURI = to_add.ProductUrl,Name = to_add.Title,ThumbURI = (to_add.CoverPhotoBaseUrl + "=w120-h120")};
            
            PhotoCollection.Add(tempAlbum);

        }
    }
}