using System;
using System.Collections;
using Google.Apis.PhotosLibrary.v1;
using Google.Apis.PhotosLibrary.v1.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using App4.Models;
using Xamarin.Forms;
using Album = App4.Models.Album;

namespace App4.ViewModels
{
    public class AlbumViewModel: BaseViewModel
    {
        private string NextPage { get; set; }
        public ObservableCollection<PhotoItem> MediaItemsCollection { get; set; }
        public Album AlbumSelected { get; set; }
        
        private PhotosLibraryService service { get;}

        public Command LoadItemsCommand { get; set; }
        public Command AddItemsCommand { get; set; }

        public AlbumViewModel(PhotosLibraryService service, Album albumSelected = null )
        {
            Title = albumSelected?.Name;
            AlbumSelected = albumSelected;
            this.service = service;

            LoadItemsCommand = new Command(async () => await ExecuteLoadMediaItemsCommand());
            AddItemsCommand =  new Command(async () => await ExecuteAddMediaItemsCommand());

            MediaItemsCollection = new ObservableCollection<PhotoItem>();

            
            
        }

        async Task  ExecuteLoadMediaItemsCommand()
        {
            IsBusy = true;

            try
            {
                MediaItemsResource.SearchRequest request = new MediaItemsResource.SearchRequest(service, new SearchMediaItemsRequest() { AlbumId = AlbumSelected?.Id,PageSize = 100, PageToken = NextPage});
                
                var photos = await request.ExecuteAsync();

                NextPage = photos.NextPageToken;

                foreach (var item in photos.MediaItems)
                {
                    PhotoItem tempItem = new PhotoItem(){FullUrl = item.ProductUrl,Id = item.Id,Name = item.Filename,ThumbUrl = item.BaseUrl+ "=w120-h120" };
                    MediaItemsCollection.Add(tempItem);
                    
                }

                if (NextPage != null)
                {
                    await ExecuteLoadMediaItemsCommand();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {

                IsBusy = false;
            }

            

        }


        public async Task ExecuteAddMediaItemsCommand()
        {
            IsBusy = true;



            try
            {
                if (NextPage != null)
                {
                    MediaItemsResource.SearchRequest request = new MediaItemsResource.SearchRequest(service, new SearchMediaItemsRequest() { AlbumId = AlbumSelected?.Id, PageSize = 100, PageToken = NextPage });

                    var photos = await request.ExecuteAsync();

                    NextPage = photos.NextPageToken;

                    foreach (var item in photos.MediaItems)
                    {
                        PhotoItem tempItem = new PhotoItem() { FullUrl = item.ProductUrl, Id = item.Id, Name = item.Filename, ThumbUrl = item.BaseUrl + "=w120-h120" };
                        MediaItemsCollection.Add(tempItem);

                    }

                }



            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                IsBusy = false;
            }



        }

    }
}