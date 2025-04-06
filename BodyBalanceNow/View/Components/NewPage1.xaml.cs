using CommunityToolkit.Maui.Views;

namespace TFGMultiplatform.Views.Components;

public partial class NewPage1 : Popup
{
	public NewPage1()
	{
		InitializeComponent();
	}

    private void OnCloseButtonClicked(object sender, EventArgs e)
    {
        Close(); // Cierra el popup programáticamente
    }
}