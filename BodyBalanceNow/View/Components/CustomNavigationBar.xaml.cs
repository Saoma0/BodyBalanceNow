namespace TFGMultiplatform.Views.Components;

public partial class CustomNavigationBar : ContentView
{
	public CustomNavigationBar()
	{
		InitializeComponent();
	}
    private void onAbrirMenu(object sender, EventArgs e)
    {
        Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
    }
}