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
                await Navigation.PopAsync(); // Opcional: regresar a la pantalla anterior
                return;
            }

            // Mostrar el prompt para ingresar el nombre de la rutina
            string nombreRutina = await DisplayPromptAsync("Nueva Rutina", "Escribe el nombre de la rutina", "Guardar", "Cancelar", placeholder: "...", maxLength: 100);

            // Verificar si el nombre no está vacío ni es nulo
            if (!string.IsNullOrEmpty(nombreRutina))
            {
                // Si el nombre es válido, insertar en la base de datos
                idRutina = db.InsertarRutina(nombreRutina, idUsuarioActual); // Añadir id usuario cuando lo pases al proyecto final
                Debug.WriteLine($"Nombre ingresado: {nombreRutina}");

                // Llamar a la función para cargar ejercicios después de la inserción
                cargarEjercicios();
            }
            else
            {
                // Si el nombre es nulo o vacío, no hacer nada
                Debug.WriteLine("El usuario canceló o no ingresó un nombre.");
            }
        }



        private int tamañoPagina = 30; // Número de registros por página
        private int paginaActual = 1; // Página actual
        private int totalPaginas; // Total de páginas
        public void cargarEjercicios()
        {
            int totalRegistros = db.GetCantidadDatos();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamañoPagina);

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
            if (paginaActual > totalPaginas)
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
            var button = sender as Button;
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
                return; // No lo añadimos si ya existe
            }

            // Si no está en la lista, lo añadimos
            ListaResumen.Add(ejercicio);
            resumenEntrenamiento.ItemsSource = ListaResumen;
        }

        private void OnRemoveExerciseFromRoutine(object sender, EventArgs e)
        {

            var button = sender as Button;
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

        private void OnAddRoutine(object sender, EventArgs e)
        {
            foreach (Ejercicios ejercicios in ListaResumen)
            {
                db.InsertarEjercicioEnRutina(idRutina,ejercicios);
            }

            ListaResumen.Clear();
            DisplayAlert("Exito","Rutina Guardada","Cerrar");
        }

        private void OnInfo(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button?.BindingContext is Ejercicios ejercicio)
            {
                var popup = new ListaEjerciciosPopUp(ejercicio);
                this.ShowPopup(popup);
            }
        }

    }

}
