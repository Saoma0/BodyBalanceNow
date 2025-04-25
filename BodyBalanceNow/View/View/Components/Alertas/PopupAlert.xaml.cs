using CommunityToolkit.Maui.Views;

namespace TFGMultiplatform.Views.Components.Alertas;

public partial class PopupAlert : Popup
{
	public PopupAlert()
	{
		InitializeComponent();
        
    }

    private void OnCloseAlert(object sender, EventArgs e)
    {
		Close();
    }
}