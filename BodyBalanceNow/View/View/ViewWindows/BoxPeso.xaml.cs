namespace BodyBalanceNow.View.ViewWindows;

public partial class BoxPeso : ContentPage
{
	public BoxPeso()
	{
		InitializeComponent();
	}


    private async void IrARegistroIMC_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new BodyBalanceNow.View.ViewWindows.CalculadoraIMC());
    }

    private async void IrARegistroPeso_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new BodyBalanceNow.View.ViewWindows.RegistroPesoWindows());
    }
}