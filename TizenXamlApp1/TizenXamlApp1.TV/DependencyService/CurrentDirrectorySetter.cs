using App4.Services;
using TizenXamlApp1.DependencyService;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;

[assembly: Dependency(typeof(CurrentDirrectorySetter))]
namespace TizenXamlApp1.DependencyService
{
    public class CurrentDirrectorySetter: ICurrentDir
    {
        public string GetCurrent()
        {
            return FormsApplication.Current.DirectoryInfo.Resource;
        }
    }
}