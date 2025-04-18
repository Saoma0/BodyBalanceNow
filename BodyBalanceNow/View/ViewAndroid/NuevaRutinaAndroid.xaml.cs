using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using BodyBalanceNow.Services;
using UraniumUI.Pages;

namespace BodyBalanceNow.View.ViewAndroid
{
    public partial class NuevaRutinaAndroid : UraniumContentPage
    {
        int idRutina = 0;
        DatabaseServiceAndroid db;
        ObservableCollection<Ejercicios> ListaResumen = new ObservableCollection<Ejercicios>();
        private int idUsuarioActual = Preferences.Get("current_user_id", -1);
        private bool usuarioAutenticado = false;
        private bool rutinaCreada = false; // Variable para verificar si la rutina ha sido creada

        private int tamañoPagina = 30; // Número de registros por página
        private int paginaActual = 1; // Página actual
        private int totalPaginas = 0; // Total de páginas, ahora es una variable de instancia

        public NuevaRutinaAndroid()
        {
            InitializeComponent();
            db = new DatabaseServiceAndroid();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            usuarioAutenticado = Authenticacion.UsuarioHaIniciadoSesion();

            if (!usuarioAutenticado || idUsuarioActual == -1)
            {
                await DisplayAlert("Atención", "Debes iniciar sesión para crear una rutina.", "Cerrar");
                await Navigation.PopAsync();
                return;
            }

            cargarEjercicios();
            // Ahora no pedimos el nombre de la rutina aquí, solo cuando se añade el primer ejercicio
            Debug.WriteLine("Esperando a que el usuario añada ejercicios...");
        }

        public void cargarEjercicios()
        {
            int totalRegistros = db.GetCantidadDatos();
            totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamañoPagina); // Solo se calcula una vez

            // Verificamos que la página actual no esté fuera del rango de totalPaginas
            if (paginaActual > totalPaginas)
            {
                paginaActual = totalPaginas; // Ajustar la página actual si es mayor que el total
            }

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

        private async void OnAddExerciseInRoutine(object sender, EventArgs e)
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
                return; // No lo añadimos si ya existe
            }

            // Si no está en la lista, lo añadimos
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
                return; // No se guarda la rutina si no hay ejercicios
            }

            // Pedir el nombre de la rutina solo cuando se haya agregado al menos un ejercicio
            string nombreRutina = await DisplayPromptAsync("Nueva Rutina", "Escribe el nombre de la rutina", "Guardar", "Cancelar", placeholder: "...", maxLength: 100);

            // Si el nombre no es vacío o nulo, guardamos la rutina
            if (!string.IsNullOrEmpty(nombreRutina))
            {
                idRutina = db.InsertarRutina(nombreRutina, idUsuarioActual); // Insertamos la rutina en la base de datos

                // Insertar los ejercicios en la rutina
                foreach (Ejercicios ejercicio in ListaResumen)
                {
                    db.InsertarEjercicioEnRutina(idRutina, ejercicio);
                }

                ListaResumen.Clear();
                rutinaCreada = true; // Marcamos la rutina como creada
                DisplayAlert("Éxito", "Rutina Guardada", "Cerrar");
            }
            else
            {
                DisplayAlert("Error", "El nombre de la rutina no puede estar vacío.", "Cerrar");
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
