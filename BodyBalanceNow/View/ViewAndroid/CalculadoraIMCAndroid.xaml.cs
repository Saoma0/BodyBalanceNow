using System;
using System.Collections.ObjectModel;
using System.Globalization;
using BodyBalanceNow.Models;
using BodyBalanceNow.Services;
using Microsoft.Maui.Controls;

namespace BodyBalanceNow.View.ViewAndroid // ← CAMBIADO
{
    public partial class CalculadoraIMCAndroid : ContentPage // ← CAMBIADO
    {
        private ExerciseDatabase baseDeDatos = new ExerciseDatabase();
        private double ultimoIMCCalculado;
        private int idUsuarioActual = Preferences.Get("current_user_id", -1);
        private bool usuarioAutenticado = false;
        public ObservableCollection<ProgresoIMC> RegistrosIMC { get; set; }

        public CalculadoraIMCAndroid() // ← CAMBIADO
        {
            InitializeComponent();
            RegistrosIMC = new ObservableCollection<ProgresoIMC>();
            historialIMCCollection.ItemsSource = RegistrosIMC;
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            usuarioAutenticado = Authenticacion.UsuarioHaIniciadoSesion();

            if (!usuarioAutenticado)
            {
                mensajeNoSesion.IsVisible = true;
                guardarIMCDB.IsVisible = false;
                RegistrosIMC.Clear();
                historialTitleLabel.IsVisible = false;
            }
            else
            {
                mensajeNoSesion.IsVisible = false;
                guardarIMCDB.IsVisible = true;
                CargarHistorialIMC();
            }
        }

        private async void CargarHistorialIMC()
        {
            try
            {
                var historial = await baseDeDatos.ObtenerHistorialIMC(idUsuarioActual);
                RegistrosIMC.Clear();

                foreach (var item in historial)
                    RegistrosIMC.Add(item);

                historialTitleLabel.IsVisible = RegistrosIMC.Count > 0;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo cargar el historial: {ex.Message}", "OK");
            }
        }

        private async void CalcularIMC(object sender, EventArgs e)
        {
            try
            {
                string textoAltura = alturaEntry.Text?.Replace(',', '.');
                string textoPeso = pesoEntry.Text?.Replace(',', '.');

                if (double.TryParse(textoAltura, CultureInfo.InvariantCulture, out double altura) &&
                    double.TryParse(textoPeso, CultureInfo.InvariantCulture, out double peso))
                {
                    string unidadSeleccionada = unidadPicker.SelectedItem?.ToString();
                    bool esMetros = unidadSeleccionada == "Metros ej: 1.75";

                    if (!esMetros)
                        altura /= 100.0;

                    if (altura <= 0)
                    {
                        await DisplayAlert("Error", "La altura debe ser mayor que cero.", "OK");
                        return;
                    }

                    double imc = peso / (altura * altura);
                    string interpretacion = ObtenerInterpretacionIMC(imc);

                    resultadoLabel.Text = $"Tu IMC es: {imc:F2}\n{interpretacion}";
                    resultadoLabel.TextColor = Colors.White;
                    resultadoFrame.BackgroundColor = ObtenerColorIMC(imc);
                    resultadoFrame.IsVisible = true;

                    ultimoIMCCalculado = imc;
                    guardarIMCDB.IsVisible = usuarioAutenticado;
                }
                else
                {
                    await DisplayAlert("Error", "Introduce valores válidos para peso y altura", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Se produjo un error: {ex.Message}", "OK");
            }
        }

        private async void GuardarIMC(object sender, EventArgs e)
        {
            if (idUsuarioActual == -1 || !usuarioAutenticado)
            {
                mensajeNoSesion.IsVisible = true;
                return;
            }

            var progreso = new ProgresoIMC
            {
                FechaRegistro = DateTime.Now,
                IMC = ultimoIMCCalculado,
                IdUser = idUsuarioActual
            };

            try
            {
                await baseDeDatos.GuardarProgresoIMC(progreso);
                await DisplayAlert("Éxito", "Progreso guardado correctamente.", "OK");
                guardarIMCDB.IsVisible = false;
                limpiarEntrys();
                CargarHistorialIMC();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo guardar el progreso: {ex.Message}", "OK");
            }
        }

        private string ObtenerInterpretacionIMC(double imc)
        {
            return imc switch
            {
                < 18.5 => "Estás en bajo peso.",
                < 24.9 => "Tu peso es normal.",
                < 29.9 => "Tienes sobrepeso.",
                < 34.9 => "Obesidad grado I.",
                < 39.9 => "Obesidad grado II.",
                _ => "Obesidad grado III (mórbida)."
            };
        }

        private Color ObtenerColorIMC(double imc)
        {
            return imc switch
            {
                < 18.5 => Colors.Orange,
                < 24.9 => Colors.Green,
                < 29.9 => Colors.Goldenrod,
                < 34.9 => Colors.OrangeRed,
                < 39.9 => Colors.IndianRed,
                _ => Colors.DarkRed
            };
        }

        public void limpiarEntrys()
        {
            alturaEntry.Text = string.Empty;
            pesoEntry.Text = string.Empty;
        }
    }
}
