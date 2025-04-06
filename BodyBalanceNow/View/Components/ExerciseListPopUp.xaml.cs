using CommunityToolkit.Maui.Views;

namespace TFGMultiplatform.Views.Components;

public partial class ExerciseListPopUp : Popup
{
	public ExerciseListPopUp()
	{
		InitializeComponent();
	}

    private void OnCloseWindow(object sender, EventArgs e)
    {
		Close();
    }

    private void OnRemoveExercise(object sender, EventArgs e)
    {

    }

    private void OnClosePopup(object sender, EventArgs e)
    {

    }
}