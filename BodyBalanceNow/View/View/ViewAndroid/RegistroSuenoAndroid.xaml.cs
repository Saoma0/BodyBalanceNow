using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BodyBalanceNow.Models;
using BodyBalanceNow.Services;
using Microsoft.Maui.Controls;
using UraniumUI.Pages;
using UraniumUI.Material.Controls;
using BodyBalanceNow.View.Components;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;


namespace BodyBalanceNow.View.ViewAndroid
{
    public partial class RegistroSuenoAndroid : UraniumContentPage
    {
        private ExerciseDatabaseAndroid baseDeDatos = new(); // Base de datos para Android
        private int idUsuarioActual; // ID del usuario actual
        private bool usuarioAutenticado = false; // Indica si el usuario ha iniciado sesión
        private const double objetivoDiario = 8.0; // 8 horas recomendadas
        public ObservableCollection<RegistroSueno> RegistrosSueno { get; set; } = new(); // Colección de registros de sueño

        public RegistroSuenoAndroid()
        {
            InitializeComponent();
            historialSuenoCollection.ItemsSource = RegistrosSueno;
            BindingContext = this;
        }

        // El evento OnAppearing se llama cuando la página aparece en la pantalla
        protected override async void OnAppearing() 
        {
            base.OnAppearing();

            idUsuarioActual = Preferences.Get("current_user_id", -1);
            usuarioAutenticado = Authenticacion.UsuarioHaIniciadoSesion();

            if (!usuarioAutenticado || idUsuarioActual == -1)
            {
                mensajeNoSesion.IsVisible = true;
                RegistrosSueno.Clear();
            }
            else
            {
                mensajeNoSesion.IsVisible = false;
                await CargarHistorialSueno();
                await CargarGraficoSemanal();
            }
        }

        // El evento para registrar el sueño
        private async void RegistrarSueno_Clicked(object sender, EventArgs e)
        {
            if (!usuarioAutenticado || idUsuarioActual == -1)
            {
                var popup = new CustomPopup("Debes iniciar sesión para registrar tu sueño.");
                this.ShowPopup(popup);
                return;
            }

            var fecha = fechaSuenoPicker.Date;
            var horaDormir = horaDormirPicker.Time;
            var horaDespertar = horaDespertarPicker.Time;

            double horasDormidas = CalcularHorasDormidas(horaDormir, horaDespertar);

            var nuevoRegistro = new RegistroSueno
            {
                UsuarioId = idUsuarioActual,
                Fecha = fecha,
                HoraDormir = horaDormir,
                HoraDespertar = horaDespertar,
                HorasDormidas = horasDormidas
            };

            try
            {
                await baseDeDatos.GuardarRegistroSueno(nuevoRegistro);
                var popup = new CustomPopup($"Has dormido {horasDormidas:F1} horas.");
                this.ShowPopup(popup);

                await CargarHistorialSueno();
                await CargarGraficoSemanal();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al guardar el registro de sueño: {ex.Message}");
            }
        }

        // Método para calcular las horas dormidas
        private double CalcularHorasDormidas(TimeSpan dormir, TimeSpan despertar)
        {
            double horas = (despertar - dormir).TotalHours;
            return horas < 0 ? 24 + horas : horas;
        }

        // Método para cargar el historial de sueño
        private async Task CargarHistorialSueno()
        {
            try
            {
                var historial = await baseDeDatos.ObtenerHistorialSueno(idUsuarioActual);
                var ultimos20 = historial.OrderByDescending(r => r.Fecha).Take(20).ToList();

                RegistrosSueno.Clear();
                foreach (var item in ultimos20)
                    RegistrosSueno.Add(item);

                double horasHoy = historial
                    .Where(r => r.Fecha.Date == DateTime.Today)
                    .Sum(r => r.HorasDormidas);

                resumenSuenoLabel.Text = $"Hoy: {horasHoy:F1} h dormidas";

                if (horasHoy >= objetivoDiario)
                {
                    recomendacionSuenoLabel.Text = "✅ Objetivo cumplido";
                    recomendacionSuenoLabel.TextColor = Colors.MediumSeaGreen;
                    recomendacionSuenoLabel.FontAttributes = FontAttributes.Bold;
                }
                else
                {
                    recomendacionSuenoLabel.Text = "*Se recomienda un sueño de 8 horas";
                    recomendacionSuenoLabel.TextColor = Color.FromArgb("#7F8C8D");
                    recomendacionSuenoLabel.FontAttributes = FontAttributes.Italic;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar el historial de sueño: {ex.Message}");
            }
        }

        // Método para cargar el gráfico semanal
        private async Task CargarGraficoSemanal()
        {
            try
            {
                var datos = await baseDeDatos.ObtenerSuenoUltimos7Dias(idUsuarioActual);
                graficoSemanalSueno.Children.Clear();

                if (datos == null || datos.Count == 0)
                    return;

                // El máximo sigue siendo útil para escalar las barras
                double maximo = Math.Max(objetivoDiario, datos.Max(d => d.Total));

                for (int i = 0; i < datos.Count; i++)
                {
                    var dia = datos[i];
                    double porcentaje = dia.Total / maximo;
                    int altura = (int)(porcentaje * 140);

                    // Determinar el color según el nivel de horas dormidas
                    Color colorBarra;
                    if (dia.Total >= 8.0) // Nivel Alto: 8 horas o más
                    {
                        colorBarra = Colors.MediumSeaGreen;
                    }
                    else if (dia.Total >= 6.0) // Nivel Medio: 6 a menos de 8 horas
                    {
                        colorBarra = Colors.Orange;
                    }
                    else // Nivel Bajo: menos de 6 horas
                    {
                        colorBarra = Colors.LightCoral;
                    }

                    var barra = new BoxView
                    {
                        HeightRequest = altura,
                        WidthRequest = 16,
                        VerticalOptions = LayoutOptions.End,
                        HorizontalOptions = LayoutOptions.Center,
                        Color = colorBarra,
                        CornerRadius = 4
                    };

                    var etiqueta = new Label
                    {
                        Text = dia.Fecha.ToString("dd"),
                        FontSize = 11,
                        TextColor = Colors.Gray,
                        HorizontalTextAlignment = TextAlignment.Center
                    };

                    var stack = new VerticalStackLayout
                    {
                        Spacing = 4,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.End,
                        Children = { barra, etiqueta }
                    };

                    Grid.SetColumn(stack, i);
                    graficoSemanalSueno.Children.Add(stack);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar la gráfica semanal: {ex.Message}");
            }
        }

        // Método para eliminar un registro de sueño
        private async void EliminarRegistroSueno_Clicked(object sender, EventArgs e)
        {
            if (sender is ImageButton btn && btn.CommandParameter is RegistroSueno registro)
            {
                var mensaje = $"¿Eliminar el registro del {registro.Fecha:dd/MM/yyyy}?";
                var popup = new ConfirmPopup(mensaje);
                bool? resultado = await this.ShowPopupAsync(popup) as bool?;
                bool confirmacion = resultado == true;

                if (!confirmacion) return;

                try
                {
                    await baseDeDatos.EliminarRegistroSueno(registro.Id);
                    await CargarHistorialSueno();
                    await CargarGraficoSemanal();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al eliminar el registro de sueño: {ex.Message}");
                }
            }
        }

    }
}
