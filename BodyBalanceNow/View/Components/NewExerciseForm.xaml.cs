namespace TFGMultiplatform.Views.Components;
using CommunityToolkit.Maui.Views;

public partial class NewExerciseForm : Popup
{
	public NewExerciseForm()
	{
		InitializeComponent();
	}

    private void OnSaveNewExercise(object sender, EventArgs e)
    {

    }

    private void OnCloseForm(object sender, EventArgs e)
    {

    }

    private void OnCloseTabClicked(object sender, EventArgs e)
    {
        Close();
    }
}