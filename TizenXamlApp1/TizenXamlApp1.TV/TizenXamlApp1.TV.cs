using System;
using System.IO;
using App4;
using Xamarin.Forms;


namespace TizenXamlApp1
{
    class Program : global::Xamarin.Forms.Platform.Tizen.FormsApplication
    {

        public string fstream { get; set; }
        protected override void OnCreate()
        {
            base.OnCreate();

            LoadApplication(new App());
        }

        static void Main(string[] args)
        {
            var app = new Program();
            Forms.Init(app);
            app.Run(args);
        }
    }
}
