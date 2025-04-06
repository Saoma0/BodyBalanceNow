using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using BodyBalanceNow.View.ViewAndroid;
using BodyBalanceNow.View.ViewWindows;

namespace BodyBalanceNow;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
#if ANDROID
        InicioTab.ContentTemplate = new DataTemplate(typeof(MainPageAndroid));
#elif WINDOWS
        InicioTab.ContentTemplate = new DataTemplate(typeof(MainPageWindows));
#else

#endif
    }

}