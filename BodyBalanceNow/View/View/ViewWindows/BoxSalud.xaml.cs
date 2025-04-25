namespace BodyBalanceNow.View.ViewWindows;

public partial class BoxSalud : ContentPage
{
	public BoxSalud()
	{
		InitializeComponent();
	}

    private async void IrARegistroAguaWindows_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new BodyBalanceNow.View.ViewWindows.RegistroAguaWindows());
        
    }

    private async void IrARegistroSuenoWindows_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new BodyBalanceNow.View.ViewWindows.RegistroSuenoWindows());
    }

    private async void IrARegistroEstresWindows_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new BodyBalanceNow.View.ViewWindows.RegistroEstresWindows());
    }


}