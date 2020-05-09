using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App4.Models;
using App4.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App4.Views
{
    [DesignTimeVisible(false)]
    public partial class AlbumPage : ContentPage
    {
        AlbumViewModel viewModel;
        public AlbumPage(AlbumViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = this.viewModel = viewModel;

           CollectionViewC.EmptyView = new LoadingView();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.MediaItemsCollection.Count == 0)
                viewModel.IsBusy = true;
            
        }

        private void ItemsView_OnScrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            RefreshViewPhotos.IsRefreshing = true;
            RefreshViewPhotos.IsRefreshing = false;
        }

        private async void CollectionViewC_OnRemainingItemsThresholdReached(object sender, EventArgs e)
        {


                await viewModel.ExecuteAddMediaItemsCommand();
                var ver = viewModel.MediaItemsCollection.Count;
           
        }
    }
}