using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using App4.Services;
using QRCoder;
using Tizen.Multimedia;
using Tizen.Multimedia.Util;
using Tizen.Multimedia.Vision;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace App4.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ResponcePage : ContentPage, IEmbeddedBrowser
    {
        public string Path;

        public event EventHandler Cancelled;

        public string Code { get; set; }
        public ResponcePage()
        {
            
            InitializeComponent();

            Code = "code here";

            BindingContext = this;
        }

        private void Button_OnClicked(object sender, EventArgs e)
        {
            LabelUrl.Text = Code + "sending";

            WebRequest request = WebRequest.Create("http://127.0.0.1:8087/authorize/" + "code153246313641356");
            request.Method = "GET";
            request.ContentType = "text/html";
            //Stream dataStream = request.GetRequestStream();
            //byte [] array = new byte[10];
            //dataStream.Write(array, 0, array.Length);
            var responce = request.GetResponse();

        }

        public string GetCode()
        {
            return Code;
        }

        public void Start(string url)
        {
            LabelUrl.Text = url;
            Path = url;


            //Barcode generation thru QRCoder
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qRCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qRCode.GetGraphic(5);
            BarCode.Source = ImageSource.FromStream(() => new MemoryStream(qrCodeBytes));



             

        }

        private void Entry_OnCompleted(object sender, EventArgs e)
        {
            Entry.Unfocus();
            EnterBtn.Focus();
        }

        //Silly, but native barcode generation not aviable on TV platform :-)
        public string GenerateQR(string textToEncode)
        {
            try
            {
                string path = DependencyService.Get<ICurrentDir>().GetShared() + "/QRCode";
                QrConfiguration qrConfig = new QrConfiguration(QrMode.Utf8, ErrorCorrectionLevel.High, 6);
                BarcodeGenerationConfiguration barConfig = new BarcodeGenerationConfiguration
                {
                    TextVisibility = Visibility.Invisible
                };

                BarcodeImageConfiguration imageConfig = new BarcodeImageConfiguration
                    (400, 400, path, BarcodeImageFormat.Jpeg);

                BarcodeGenerator.GenerateImage(textToEncode, qrConfig, imageConfig, barConfig);

                return path + ".jpg";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            


        }


    }
}