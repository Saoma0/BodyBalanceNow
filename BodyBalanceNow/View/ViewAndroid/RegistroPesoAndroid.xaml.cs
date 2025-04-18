using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using BodyBalanceNow.Models;
using BodyBalanceNow.Services;
using Microsoft.Maui.Storage;
using UraniumUI.Pages; // Necesario para usar Preferences

namespace BodyBalanceNow.View.ViewAndroid
{
    public partial class RegistroPesoAndroid : UraniumContentPage
    {
        private ExerciseDatabaseAndroid database = new();
        private readonly int idUsuarioActual = Preferences.Get("current_user_id", -1);
        private bool usuarioAutenticado = false;

        public RegistroPesoAndroid()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            usuarioAutenticado = Authenticacion.UsuarioHaIniciadoSesion();
            mensajeNoSesion.IsVisible = !usuarioAutenticado;

            if (usuarioAutenticado && idUsuarioActual != -1)
            {
                CargarHistorial();
                await CargarGraficoPeso(); // 👈 Añadido
            }
            else
            {
                historialPesoCollection.ItemsSource = null;
            }
        }


        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            if (!usuarioAutenticado || idUsuarioActual == -1)
            {
                await DisplayAlert("Error", "Usuario no autenticado. Inicia sesión primero.", "OK");
                return;
            }

            if (decimal.TryParse(PesoEntry.Text, out decimal peso))
            {
                var registro = new PesoModel
                {
                    UsuarioId = idUsuarioActual,
                    Fecha = FechaPicker.Date,
                    Peso = peso
                };

                try
                {
                    await database.GuardarRegistroPeso(registro);
                    await DisplayAlert("Éxito", "Registro guardado correctamente", "OK");

                    PesoEntry.Text = string.Empty;
                    FechaPicker.Date = DateTime.Now;

                    CargarHistorial();
                    await CargarGraficoPeso(); // 👈 esto actualiza el gráfico
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"No se pudo guardar el peso: {ex.Message}", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "Introduce un peso válido", "OK");
            }
        }


        private async void CargarHistorial()
        {
            if (!usuarioAutenticado || idUsuarioActual == -1) return;

            var historial = await database.ObtenerHistorialPeso(idUsuarioActual);
            historialPesoCollection.ItemsSource = historial;
        }

        private async Task CargarGraficoPeso()
        {
            var datos = await database.ObtenerPesoUltimos7Dias(idUsuarioActual);

            GraficoPesoSemanal.Children.Clear();
            GraficoPesoSemanal.ColumnDefinitions.Clear();

            if (datos.Count == 0)
                return;

            double min = datos.Min(d => d.Total);
            double max = datos.Max(d => d.Total);
            double rango = max - min;

            for (int i = 0; i < datos.Count; i++)
            {
                if (GraficoPesoSemanal.ColumnDefinitions.Count >= 7)
                    break;

                // Altura escalada, con altura mínima garantizada (20 px)
                double altura = (rango == 0)
                    ? 80
                    : Math.Max(20, (datos[i].Total - min) / rango * 120);

                // Color según nivel
                var color = datos[i].Total >= max ? Colors.Red :
                            datos[i].Total <= min ? Colors.Green :
                            Color.FromArgb("#FFB300");

                // Barra del gráfico
                var barra = new BoxView
                {
                    HeightRequest = altura,
                    WidthRequest = 20,
                    VerticalOptions = LayoutOptions.End,
                    BackgroundColor = color,
                    CornerRadius = 6
                };

                // Fecha formateada tipo "13 abr"
                var fechaLabel = new Label
                {
                    Text = datos[i].Fecha.ToString("dd MMM"),
                    FontSize = 11,
                    TextColor = Color.FromArgb("#2C3E50"),
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 4, 0, 0)
                };

                // Apilar barra y etiqueta
                var stack = new VerticalStackLayout
                {
                    Spacing = 4,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.End,
                    Children = { barra, fechaLabel }
                };

                // Añadir al gráfico
                GraficoPesoSemanal.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                Grid.SetColumn(stack, GraficoPesoSemanal.ColumnDefinitions.Count - 1);
                GraficoPesoSemanal.Children.Add(stack);
            }
        }

        private async void EliminarRegistroPeso_Clicked(object sender, EventArgs e)
        {
            if (sender is ImageButton btn && btn.CommandParameter is PesoModel registro)
            {
                bool confirm = await DisplayAlert("Confirmar", $"¿Eliminar el registro del {registro.Fecha:dd/MM/yyyy}?", "Sí", "No");
                if (confirm)
                {
                    await database.EliminarRegistroPeso(registro.ID);
                    await DisplayAlert("Eliminado", "Registro eliminado correctamente", "OK");
                    CargarHistorial();
                    await CargarGraficoPeso();
                }
            }
        }

    }
}
