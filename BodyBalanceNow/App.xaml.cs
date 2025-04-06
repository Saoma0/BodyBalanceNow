using BodyBalanceNow.View.ViewWindows;
using BodyBalanceNow.View.ViewAndroid;
namespace BodyBalanceNow
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }
    }
}
