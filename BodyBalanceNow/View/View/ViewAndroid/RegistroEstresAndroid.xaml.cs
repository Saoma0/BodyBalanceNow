using System;
using System.Collections.ObjectModel;
using BodyBalanceNow.Converters;
using BodyBalanceNow.Models;
using BodyBalanceNow.Services;
using Microsoft.Maui.Controls;
using UraniumUI.Pages;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using BodyBalanceNow.View.Components;
using System.Diagnostics;

namespace BodyBalanceNow.View.ViewAndroid;

public partial class RegistroEstresAndroid : UraniumContentPage
{
    private ExerciseDatabaseAndroid baseDeDatos = new (); // Base de datos para Android
    private int idUsuarioActual; // ID del usuario actual
    private bool usuarioAutenticado = false; // Indica si el usuario ha iniciado sesión
    public ObservableCollection<RegistroEstres> RegistrosEstres { get; set; } = new(); // Colección de registros de estrés

    public RegistroEstresAndroid()
    {
        InitializeComponent();
        historialEstresCollection.ItemsSource = RegistrosEstres;
        BindingContext = this;
    }

    /// Método que se ejecuta al cargar la página
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        idUsuarioActual = Preferences.Get("current_user_id", -1);
        usuarioAutenticado = Authenticacion.UsuarioHaIniciadoSesion();

        if (!usuarioAutenticado || idUsuarioActual == -1)
        {
            mensajeNoSesion.IsVisible = true;
            historialTitleLabel.IsVisible = true;
            RegistrosEstres.Clear();
        }
        else
        {
            mensajeNoSesion.IsVisible = false;
            await CargarHistorialEstres();
        }

        await CargarGraficoSemanal();
    }

    // Método que se ejecuta al cerrar la página
    private async void RegistrarEstres_Clicked(object sender, EventArgs e)
    {
        if (!usuarioAutenticado || idUsuarioActual == -1)
        {
            var popup = new CustomPopup("Debes iniciar sesión para registrar tu nivel de estrés.");
            this.ShowPopup(popup);
            return;
        }

        var fecha = fechaEstresPicker.Date;
        var nivel = (int)Math.Round(sliderNivelEstres.Value);

        var nuevoRegistro = new RegistroEstres
        {
            UsuarioId = idUsuarioActual,
            Fecha = fecha,
            Nivel = nivel
        };

        try
        {
            await baseDeDatos.GuardarRegistroEstres(nuevoRegistro);

            var popup = new CustomPopup($"Has registrado un nivel de estrés: {nivel}.");
            this.ShowPopup(popup);

            await CargarHistorialEstres();
            await CargarGraficoSemanal();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al guardar el registro: {ex.Message}");
        }
    }

    // Método que se ejecuta al cambiar el valor del slider
    private void SliderNivelEstres_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        int nivel = (int)Math.Round(e.NewValue);
        valorSliderLabel.Text = $"Nivel actual: {nivel}";

        // Cambiar emoji según nivel
        string emoji = nivel switch
        {
            <= 2 => "😌",   // Muy relajado
            <= 4 => "🙂",   // Tranquilo
            <= 6 => "😐",   // Neutro
            <= 8 => "😟",   // Algo estresado
            _ => "😫"    // Muy estresado
        };

        emojiEstresLabel.Text = emoji;
    }

    // Método para cargar el historial de estrés
    private async Task CargarHistorialEstres()
    {
        try
        {
            var historial = await baseDeDatos.ObtenerHistorialEstres(idUsuarioActual);
            var ultimos20 = historial.OrderByDescending(r => r.Fecha).Take(20).ToList();

            RegistrosEstres.Clear();
            foreach (var item in ultimos20)
                RegistrosEstres.Add(item);


            // Obtener registros de hoy
            var registrosHoy = historial.Where(r => r.Fecha.Date == DateTime.Today).ToList();
            int nivelPromedio = registrosHoy.Any()
                ? (int)Math.Round(registrosHoy.Average(r => r.Nivel))
                : 0;

            // Emoji del nivel promedio
            string emoji = new NivelEstresToEmojiConverter().Convert(nivelPromedio, typeof(string), null, null)?.ToString();
            resumenEstresLabel.Text = $"Hoy: Nivel {nivelPromedio} {emoji}";

            // Mensaje según el nivel promedio
            switch (nivelPromedio)
            {
                case 0:
                case 1:
                    recomendacionEstresLabel.Text = "🧘 Estás muy tranquilo. ¡Sigue así!";
                    recomendacionEstresLabel.TextColor = Colors.SeaGreen;
                    break;
                case 2:
                case 3:
                    recomendacionEstresLabel.Text = "🙂 Estrés muy bajo. Ambiente relajado.";
                    recomendacionEstresLabel.TextColor = Colors.Teal;
                    break;
                case 4:
                case 5:
                    recomendacionEstresLabel.Text = "😐 Nivel medio. Intenta tomar descansos.";
                    recomendacionEstresLabel.TextColor = Colors.DarkOrange;
                    break;
                case 6:
                case 7:
                    recomendacionEstresLabel.Text = "😟 Estrés elevado. Haz una pausa.";
                    recomendacionEstresLabel.TextColor = Colors.OrangeRed;
                    break;
                case 8:
                case 9:
                    recomendacionEstresLabel.Text = "😫 Estrés muy alto. Intenta desconectar.";
                    recomendacionEstresLabel.TextColor = Colors.Red;
                    break;
                case 10:
                    recomendacionEstresLabel.Text = "🚨 Estrés extremo. Busca apoyo si lo necesitas.";
                    recomendacionEstresLabel.TextColor = Colors.DarkRed;
                    break;
                default:
                    recomendacionEstresLabel.Text = "*El estrés debe mantenerse bajo para una buena salud";
                    recomendacionEstresLabel.TextColor = Color.FromArgb("#7F8C8D");
                    break;
            }

            // Estilo del mensaje
            recomendacionEstresLabel.FontAttributes = nivelPromedio >= 6
                ? FontAttributes.Bold
                : FontAttributes.Italic;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al cargar el historial de estrés: {ex.Message}");
        }
    }

    // Método para cargar el gráfico semanal de estrés
    private async Task CargarGraficoSemanal()
    {
        try
        {
            var datos = await baseDeDatos.ObtenerEstresUltimos7Dias(idUsuarioActual);
            graficoSemanalEstres.Children.Clear();

            if (datos == null || datos.Count == 0)
                return;

            double maximo = 10.0;

            for (int i = 0; i < datos.Count; i++)
            {
                var dia = datos[i];
                double porcentaje = dia.Total / maximo;
                int altura = (int)(porcentaje * 140);

                // Cambiar color dependiendo del nivel de estrés
                Color colorBarra;
                if (porcentaje >= 0.75) // Estrés alto
                {
                    colorBarra = Colors.OrangeRed;
                }
                else if (porcentaje >= 0.25) // Estrés medio
                {
                    colorBarra = Color.FromHex("#FFB300");
                }
                else // Estrés bajo
                {
                    colorBarra = Colors.Teal;
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
                graficoSemanalEstres.Children.Add(stack);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al cargar el gráfico semanal: {ex.Message}");
        }
    }

    // Método para eliminar un registro de estrés
    private async void EliminarRegistroEstres_Clicked(object sender, EventArgs e)
    {
        if (sender is ImageButton btn && btn.CommandParameter is RegistroEstres registro)
        {
            var mensaje = $"¿Eliminar el registro del {registro.Fecha:dd/MM/yyyy}?";
            var popup = new ConfirmPopup(mensaje);
            bool? resultado = await this.ShowPopupAsync(popup) as bool?;
            bool confirmacion = resultado == true;

            if (!confirmacion) return;

            try
            {
                await baseDeDatos.EliminarRegistroEstres(registro.Id);
                await CargarHistorialEstres();
                await CargarGraficoSemanal();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al eliminar el registro: {ex.Message}");
            }
        }
    }
}
