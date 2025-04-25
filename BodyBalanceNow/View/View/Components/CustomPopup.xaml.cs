using CommunityToolkit.Maui.Views;

namespace BodyBalanceNow.View.Components;

public partial class CustomPopup : Popup
{
    public CustomPopup(string mensaje)
    {
        InitializeComponent();
        MessageLabel.Text = mensaje;
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close();
    }
}