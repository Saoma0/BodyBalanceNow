using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using BodyBalanceNow.Services;

namespace BodyBalanceNow.View.ViewWindows
{
    public partial class NuevaRutina : ContentPage
    {
        int idRutina = 0;
        DatabaseService db;
        ObservableCollection<Ejercicios> ListaResumen = new ObservableCollection<Ejercicios>();
        private int idUsuarioActual = Preferences.Get("current_user_id", -1);
        private bool usuarioAutenticado = false;

        private int tamañoPagina = 30; // Número de registros por página
        private int paginaActual = 1; // Página actual
        private int totalPaginas; // Total de páginas

        public NuevaRutina()
        {
            InitializeComponent();
            db = new DatabaseService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            usuarioAutenticado = Authenticacion.UsuarioHaIniciadoSesion();

            if (!usuarioAutenticado || idUsuarioActual == -1)
            {
                await DisplayAlert("Atención", "Debes iniciar sesión para crear una rutina.", "Cerrar");
                await Navigation.PopAsync(); // Regresar a la pantalla anterior
                return;
            }

            cargarEjercicios();
        }

        public void cargarEjercicios()
        {
            int totalRegistros = db.GetCantidadDatos();
            totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamañoPagina);

            var ejercicios = db.GetEjerciciosPorPagina(paginaActual, tamañoPagina);
            pActual.Text = paginaActual.ToString();
            listaEjercicios.ItemsSource = ejercicios;
        }

        private async void OnAnteriorClicked(object sender, EventArgs e)
        {
            if (paginaActual > 1)
            {
                paginaActual--;
                cargarEjercicios();
            }
            else
            {
                await DisplayAlert("Atención", "Estás en la primera página.", "Cerrar");
            }
        }

        private async void OnSiguienteClicked(object sender, EventArgs e)
        {
            if (paginaActual < totalPaginas)
            {
                paginaActual++;
                cargarEjercicios();
            }
            else
            {
                await DisplayAlert("Atención", "No hay más páginas disponibles.", "Cerrar");
            }
        }

        private void OnAddExerciseInRoutine(object sender, EventArgs e)
        {
            var button = sender as ImageButton;
            if (button == null) return;

            var ejercicio = button.BindingContext as Ejercicios;
            if (ejercicio == null)
            {
                Debug.WriteLine("Error: No se pudo obtener el Ejercicio seleccionado.");
                return;
            }

            // Comprobar si el ejercicio ya está en ListaResumen usando el ID
            if (ListaResumen.Any(ex => ex.ID == ejercicio.ID))
            {
                DisplayAlert("Error", "Ejercicio ya añadido", "Cerrar");
                return;
            }

            ListaResumen.Add(ejercicio);
            resumenEntrenamiento.ItemsSource = ListaResumen;
        }

        private void OnRemoveExerciseFromRoutine(object sender, EventArgs e)
        {
            var button = sender as ImageButton;
            if (button == null) return;

            var ejercicio = button.BindingContext as Ejercicios;
            if (ejercicio == null)
            {
                Debug.WriteLine("Error: No se pudo obtener el Ejercicio seleccionado.");
                return;
            }

            ListaResumen.Remove(ejercicio);
            resumenEntrenamiento.ItemsSource = ListaResumen;
        }

        private async void OnAddRoutine(object sender, EventArgs e)
        {
            if (ListaResumen.Count == 0)
            {
                await DisplayAlert("Atención", "No se pueden guardar rutinas vacías.", "Cerrar");
                return;
            }

            string nombreRutina = await DisplayPromptAsync("Nueva Rutina", "Escribe el nombre de la rutina", "Guardar", "Cancelar", placeholder: "...", maxLength: 100);

            if (!string.IsNullOrEmpty(nombreRutina))
            {
                idRutina = db.InsertarRutina(nombreRutina, idUsuarioActual);

                foreach (Ejercicios ejercicio in ListaResumen)
                {
                    db.InsertarEjercicioEnRutina(idRutina, ejercicio);
                }

                ListaResumen.Clear();
                await DisplayAlert("Éxito", "Rutina Guardada", "Cerrar");
            }
            else
            {
                await DisplayAlert("Error", "El nombre de la rutina no puede estar vacío.", "Cerrar");
            }
        }

        private void OnInfo(object sender, EventArgs e)
        {
            var button = sender as ImageButton;
            if (button?.BindingContext is Ejercicios ejercicio)
            {
                var popup = new ListaEjerciciosPopUp(ejercicio);
                this.ShowPopup(popup);
            }
        }
    }
}
