using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using BodyBalanceNow.Models;
using BodyBalanceNow.Services;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using UraniumUI.Pages;
//PONER ESTO PARA POP UP
using BodyBalanceNow.View.Components;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;

namespace BodyBalanceNow.View.ViewAndroid
{
    public partial class CalculadoraIMCAndroid : UraniumContentPage // ✅ CORREGIDO
    {
        private ExerciseDatabaseAndroid baseDeDatos = new(); // Instancia de la base de datos
        private double ultimoIMCCalculado; // Variable para almacenar el último IMC calculado
        private int idUsuarioActual = Preferences.Get("current_user_id", -1); // ID del usuario actual
        private bool usuarioAutenticado = false; // Variable para verificar si el usuario ha iniciado sesión
        public ObservableCollection<ProgresoIMC> RegistrosIMC { get; set; } // Colección para almacenar los registros de IMC

        public CalculadoraIMCAndroid()
        {
            InitializeComponent();
            RegistrosIMC = new ObservableCollection<ProgresoIMC>();
            historialIMCCollection.ItemsSource = RegistrosIMC;
            BindingContext = this;
        }

        // Método que se ejecuta al aparecer la página
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

        //  Método para guardar el IMC en la base de datos
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
                var popup = new CustomPopup("Progreso guardado correctamente");
                this.ShowPopup(popup);

                guardarIMCDB.IsVisible = false;
                limpiarEntrys();
                CargarHistorialIMC(); // Recarga el historial después de guardar
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        // Método para cargar el historial de IMC al iniciar la página
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
                Debug.WriteLine(ex.Message);
            }
        }

        // Método para calcular el IMC cuando se pulsa "Calcular IMC"
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
                        var popup = new CustomPopup("La altura debe ser mayor que cero.");
                        this.ShowPopup(popup);
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
                    var popup = new CustomPopup("Introduce valores válidos para peso y altura");
                    this.ShowPopup(popup);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        // Método para obtener la interpretación del IMC
        private string ObtenerInterpretacionIMC(double imc) =>
            imc switch
            {
                < 18.5 => "Estás en bajo peso.",
                < 24.9 => "Tu peso es normal.",
                < 29.9 => "Tienes sobrepeso.",
                < 34.9 => "Obesidad grado I.",
                < 39.9 => "Obesidad grado II.",
                _ => "Obesidad grado III (mórbida)."
            };

        // Método para obtener el color según el IMC
        private Color ObtenerColorIMC(double imc) =>
            imc switch
            {
                < 18.5 => Colors.Orange,
                < 24.9 => Colors.Green,
                < 29.9 => Colors.Goldenrod,
                < 34.9 => Colors.OrangeRed,
                < 39.9 => Colors.IndianRed,
                _ => Colors.DarkRed
            };

        // Método para limpiar los campos de entrada
        public void limpiarEntrys()
        {
            alturaEntry.Text = string.Empty;
            pesoEntry.Text = string.Empty;
        }

    }
}
