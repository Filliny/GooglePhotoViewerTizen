using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App4.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EmbeddedBrowser : ContentPage, IEmbeddedBrowser
    {


        public EmbeddedBrowser(string title)
        {
            Title = title;
            InitializeComponent();

        }

        public event EventHandler Cancelled;

        public void Start(string url)
        {
           Broswer.Source = url;

        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private void OnCancel(object sender, EventArgs e)
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
        }
    }
}