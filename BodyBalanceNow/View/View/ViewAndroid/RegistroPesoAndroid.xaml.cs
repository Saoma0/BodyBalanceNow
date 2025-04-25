using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using BodyBalanceNow.Models;
using BodyBalanceNow.Services;
using Microsoft.Maui.Storage;
using UraniumUI.Pages; // Necesario para usar Preferences
using BodyBalanceNow.View.Components;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;

namespace BodyBalanceNow.View.ViewAndroid
{
    public partial class RegistroPesoAndroid : UraniumContentPage
    {
        private ExerciseDatabaseAndroid database = new(); // base de datos Android
        private readonly int idUsuarioActual = Preferences.Get("current_user_id", -1); // ID del usuario actual
        private bool usuarioAutenticado = false; // Verifica si el usuario ha iniciado sesión

        public RegistroPesoAndroid()
        {
            InitializeComponent();
        }

        // Método que se ejecuta al cargar la página
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

        // Método que se ejecuta al hacer clic en el botón "Guardar"
        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            if (!usuarioAutenticado || idUsuarioActual == -1)
            {
                var popup = new CustomPopup("Usuario no autenticado. Inicia sesión primero.");
                this.ShowPopup(popup);
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
                    var popup = new CustomPopup("Registro guardado correctamente");
                    this.ShowPopup(popup);

                    PesoEntry.Text = string.Empty;
                    FechaPicker.Date = DateTime.Now;

                    CargarHistorial();
                    await CargarGraficoPeso(); // 👈 esto actualiza el gráfico
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al guardar el peso: {ex.Message}");
                }
            }
            else
            {
                var popup = new CustomPopup("Introduce un peso válido");
                this.ShowPopup(popup);
            }
        }

        // Método que se ejecuta al hacer clic en el botón "Eliminar"
        private async void CargarHistorial()
        {
            if (!usuarioAutenticado || idUsuarioActual == -1) return;

            var historial = await database.ObtenerHistorialPeso(idUsuarioActual);
            historialPesoCollection.ItemsSource = historial;
        }

        // Método que carga el gráfico de peso
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

        // Método que se ejecuta al hacer clic en el botón "Eliminar registro"
        private async void EliminarRegistroPeso_Clicked(object sender, EventArgs e)
        {
            if (sender is ImageButton btn && btn.CommandParameter is PesoModel registro)
            {
                var mensaje = $"¿Eliminar el registro del {registro.Fecha:dd/MM/yyyy}?";
                var popup = new ConfirmPopup(mensaje);
                bool? resultado = await this.ShowPopupAsync(popup) as bool?;
                bool confirm = resultado == true;

                if (confirm)
                {
                    await database.EliminarRegistroPeso(registro.ID);
                    var popup2 = new CustomPopup("Registro eliminado correctamente");
                    this.ShowPopup(popup2);
                    CargarHistorial();
                    await CargarGraficoPeso();
                }
            }
        }

    }
}
