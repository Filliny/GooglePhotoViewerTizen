using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using App4.Services;
using App4.Views;

namespace App4
{
    public partial class App : Application
    {

        public App()
        {

            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new ItemsPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
            
        }

        protected override void OnResume()
        {
        }
    }
}
