using System;
using System.Collections.ObjectModel;
using System.Globalization;
using BodyBalanceNow.Models;
using BodyBalanceNow.Services;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using UraniumUI.Pages;
using BodyBalanceNow.View.Components;
using System.Diagnostics;

namespace BodyBalanceNow.View.ViewAndroid;
public partial class RegistroAguaAndroid : UraniumContentPage
{
    private ExerciseDatabaseAndroid baseDeDatos = new(); // 👈 Cambia esto por tu base de datos real
    private int idUsuarioActual; //ID del usuario actual
    private bool usuarioAutenticado = false; //  Verifica si el usuario ha iniciado sesión
    private const int objetivoDiario = 2000; //  Objetivo diario de 2 litros (2000 ml)
    public ObservableCollection<RegistroAgua> RegistrosAgua { get; set; } = new(); //  Colección para el historial de agua

    private int totalConsumidoHoy = 0; //  Total consumido hoy

    public RegistroAguaAndroid()
    {
        InitializeComponent();
        historialAguaCollection.ItemsSource = RegistrosAgua;
        BindingContext = this;
    }

    // Método para inicializar la vista
    protected async override void OnAppearing()
    {
        base.OnAppearing();
        idUsuarioActual = Preferences.Get("current_user_id", -1); 

        usuarioAutenticado = Authenticacion.UsuarioHaIniciadoSesion();

        if (!usuarioAutenticado)
        {
            mensajeNoSesion.IsVisible = true;
            botonesRegistro.IsVisible = false;
            RegistrosAgua.Clear();
            historialTitleLabel.IsVisible = false;
        }
        else
        {
            mensajeNoSesion.IsVisible = false;
            botonesRegistro.IsVisible = true;

            await CargarHistorialAgua();
            await CalcularTotalHoy();
        }

        await CargarGraficoSemanalCasero();
    }

    // Método para registrar el consumo de agua
    private async void RegistrarConsumo(object sender, EventArgs e)
    {
        if (idUsuarioActual == -1 || !usuarioAutenticado)
        {
            var popup = new CustomPopup("Debes iniciar sesión para registrar consumo.");
            this.ShowPopup(popup);
            return;
        }

        if (sender is Button btn && int.TryParse(btn.CommandParameter?.ToString(), out int cantidad))
        {
            var nuevoRegistro = new RegistroAgua
            {
                UsuarioId = idUsuarioActual,
                Fecha = fechaPicker.Date,
                Cantidad = cantidad
            };

            try
            {
                await baseDeDatos.GuardarRegistroAgua(nuevoRegistro);
                var popup = new CustomPopup($"{cantidad} ml añadidos.");
                this.ShowPopup(popup);

                await CargarHistorialAgua();
                await CalcularTotalHoy();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al guardar el consumo: {ex.Message}"); // Para depuración
            }
        }
        CargarGraficoSemanalCasero();
    }

    // Método para mostrar el popup de información
    private async Task CalcularTotalHoy()
    {
        try
        {
            totalConsumidoHoy = await baseDeDatos.ObtenerTotalAguaHoy(idUsuarioActual, DateTime.Now.Date);
            progresoLabel.Text = $"Hoy: {totalConsumidoHoy} / {objetivoDiario} ml";

            double porcentaje = (double)totalConsumidoHoy / objetivoDiario;
            progresoAguaBar.Progress = Math.Min(1.0, porcentaje);

            // CAMBIO DE COLOR SEGÚN NIVEL DE AGUA
            if (porcentaje < 0.5)
                progresoAguaBar.ProgressColor = Colors.SteelBlue;
            else if (porcentaje < 0.75)
                progresoAguaBar.ProgressColor = Colors.Orange;
            else
                progresoAguaBar.ProgressColor = Colors.MediumSeaGreen;

            // MOSTRAR/OCULTAR MENSAJE DE OBJETIVO CUMPLIDO
            if (porcentaje >= 1)
            {
                objetivoCumplidoLabel.IsVisible = true;
            }
            else
            {
                objetivoCumplidoLabel.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al calcular el total diario: {ex.Message}"); // Para depuración
        }
    }

    // Método para mostrar el popup de información
    private async void EliminarRegistroAgua_Clicked(object sender, EventArgs e)
    {
        if (sender is ImageButton btn && btn.CommandParameter is RegistroAgua registro)
        {
            var mensaje = $"¿Seguro que deseas eliminar el registro del {registro.Fecha:dd/MM/yyyy} ({registro.Cantidad} ml)?";
            var popup = new ConfirmPopup(mensaje);
            bool? resultado = await this.ShowPopupAsync(popup) as bool?;
            bool confirmacion = resultado == true;

            if (confirmacion)
            {
                try
                {
                    await baseDeDatos.EliminarRegistroAgua(registro.Id); // necesitarás este método en tu clase de base de datos

                    await CargarHistorialAgua();
                    await CalcularTotalHoy();
                    CargarGraficoSemanalCasero();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al eliminar el registro: {ex.Message}"); // Para depuración
                }
            }
        }
    }


    // Método para cargar el historial de agua (ultimos 20 registros)
    private async Task CargarHistorialAgua()
    {
        try
        {
            var historial = await baseDeDatos.ObtenerHistorialAgua(idUsuarioActual);

            // Quedarse solo con los 20 más recientes
            var ultimos20 = historial
                .OrderByDescending(r => r.Fecha)
                .Take(20)
                .ToList();

            RegistrosAgua.Clear();

            foreach (var item in ultimos20)
            {
                RegistrosAgua.Add(item);
            }

            historialTitleLabel.IsVisible = RegistrosAgua.Count > 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al cargar el historial: {ex.Message}"); // Para depuración
        }
    }

    // Método para cargar el gráfico semanal
    private async Task CargarGraficoSemanalCasero()
    {
        try
        {
            var datos = await baseDeDatos.ObtenerConsumoUltimos7Dias(idUsuarioActual);

            // Limpiar la gráfica antes de redibujar
            graficoSemanalCasero.Children.Clear();

            if (datos == null || datos.Count == 0)
            {
                var mensaje = new Label
                {
                    Text = "No hay datos disponibles.",
                    TextColor = Colors.Gray,
                    FontSize = 14,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                };
                graficoSemanalCasero.Children.Add(mensaje);
                return;
            }

            // Valor máximo para escalar la altura de las barras
            int maximo = Math.Max(2000, datos.Max(d => d.Total));
            Console.WriteLine($"Máximo para altura: {maximo}");

            for (int i = 0; i < datos.Count; i++)
            {
                var dia = datos[i];

                // Calcular porcentaje respecto al objetivo de 2 litros (2000 ml)
                double porcentaje = (double)dia.Total / 2000; // Objetivo fijo
                Console.WriteLine($"Día: {dia.Fecha.ToString("dd")}, Total: {dia.Total}, Porcentaje: {porcentaje}");

                // Evitar valores fuera de rango
                porcentaje = Math.Clamp(porcentaje, 0, 1);

                // Calcular altura basada en el máximo (para visualización proporcional)
                int altura = (int)((double)dia.Total / maximo * 140);

                // Cambiar color dependiendo del nivel de consumo respecto al objetivo
                Color colorBarra;
                if (porcentaje >= 1) // ≥ 2000 ml
                {
                    colorBarra = Colors.MediumSeaGreen; // Verde
                }
                else if (porcentaje >= 0.75) // 1500-1999 ml
                {
                    colorBarra = Color.FromHex("#FFB300"); // Amarillo
                }
                else // < 1500 ml
                {
                    colorBarra = Colors.Red; // Azul
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
                graficoSemanalCasero.Children.Add(stack); // Añadir la barra al grid
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al cargar el gráfico semanal: {ex.Message}"); // Para depuración
        }
    }




}
