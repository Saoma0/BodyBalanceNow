using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using BodyBalanceNow.Services;
using UraniumUI.Pages;
using BodyBalanceNow.View.Components;
using Microsoft.Maui.Controls;

namespace BodyBalanceNow.View.ViewAndroid
{
    public partial class NuevaRutinaAndroid : UraniumContentPage
    {
        int idRutina = 0;
        DatabaseService db;
        ObservableCollection<Ejercicios> ListaResumen = new ObservableCollection<Ejercicios>();
        private int idUsuarioActual = Preferences.Get("current_user_id", -1);
        private bool usuarioAutenticado = false;

        private int tamañoPagina = 30;
        private int paginaActual = 1;
        private int totalPaginas;

        public NuevaRutinaAndroid()
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
                var popup = new CustomPopup("Debes iniciar sesión para crear una rutina.");
                this.ShowPopup(popup);
                await Navigation.PopAsync();
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
                var popup = new CustomPopup("Estás en la primera página.");
                this.ShowPopup(popup);
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
                var popup = new CustomPopup("No hay más páginas disponibles.");
                this.ShowPopup(popup);
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

            if (ListaResumen.Any(ex => ex.ID == ejercicio.ID))
            {
                var popup = new CustomPopup("Ejercicio ya añadido");
                this.ShowPopup(popup);
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
                var popup = new CustomPopup("No se pueden guardar rutinas vacías.");
                this.ShowPopup(popup);
                return;
            }

            var prompt = new PromptPopup("Escribe el nombre de la rutina", "...");
            string nombreRutina = await this.ShowPopupAsync(prompt) as string;

            if (!string.IsNullOrEmpty(nombreRutina))
            {
                idRutina = db.InsertarRutina(nombreRutina, idUsuarioActual);

                foreach (Ejercicios ejercicio in ListaResumen)
                {
                    db.InsertarEjercicioEnRutina(idRutina, ejercicio);
                }

                ListaResumen.Clear();

                var popup = new CustomPopup("Rutina Guardada");
                this.ShowPopup(popup);
            }
            else
            {
                var popup = new CustomPopup("El nombre de la rutina no puede estar vacío.");
                this.ShowPopup(popup);
            }
        }

        private void OnInfo(object sender, EventArgs e)
        {
            var button = sender as ImageButton;
            if (button?.BindingContext is Ejercicios ejercicio)
            {
                var popup = new ListaEjerciciosPopUpAndroid(ejercicio);
                this.ShowPopup(popup);
            }
        }
    }
}
