using System;
using System.Timers;
using Microsoft.Maui.Controls;
using BodyBalanceNow.View.Components;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;

namespace BodyBalanceNow.View.ViewWindows;

public partial class CronometroTemporizador : ContentPage
{
    private System.Timers.Timer cronometroTimer; // Timer para el cronómetro
    private TimeSpan tiempoTranscurrido; 
    private List<string> vueltasCrono = new(); // Lista para almacenar las vueltas del cronómetro

    private System.Timers.Timer temporizadorTimer; // Timer para el temporizador
    private TimeSpan tiempoRestante; // Tiempo restante del temporizador
    private List<string> vueltasTemp = new(); // Lista para almacenar las marcas del temporizador

    public CronometroTemporizador()
    {
        InitializeComponent();
        InicializarCronometro();
        InicializarTemporizador();
        SeleccionarModo("crono"); // Por defecto Cronómetro
    }

    // Método general para cambiar entre cronómetro y temporizador
    private void SeleccionarModo(string modo)
    {
        bool esCrono = modo == "crono";

        frameCronometro.IsVisible = esCrono;
        frameTemporizador.IsVisible = !esCrono;
        frameVueltasCrono.IsVisible = esCrono;
        frameVueltasTemp.IsVisible = !esCrono;

        // Estilo botones
        btnModoCrono.BackgroundColor = esCrono ? Color.FromArgb("#2E86DE") : Colors.White;
        btnModoCrono.TextColor = esCrono ? Colors.White : Color.FromArgb("#2E86DE");
        btnModoCrono.BorderWidth = esCrono ? 0 : 2;

        btnModoTemp.BackgroundColor = !esCrono ? Color.FromArgb("#FFA500") : Colors.White;
        btnModoTemp.TextColor = !esCrono ? Colors.White : Color.FromArgb("#2E86DE");
        btnModoTemp.BorderWidth = !esCrono ? 0 : 2;
    }

    // Métodos para manejar los eventos de los botones
    private void OnSeleccionarCronometro(object sender, EventArgs e) => SeleccionarModo("crono");

    //  Método para manejar el evento de selección del temporizador
    private void OnSeleccionarTemporizador(object sender, EventArgs e) => SeleccionarModo("tempo");

    // ------------------- CRONÓMETRO -------------------
    // Método para inicializar el cronómetro
    private void InicializarCronometro()
    {
        cronometroTimer = new System.Timers.Timer(1000);
        cronometroTimer.Elapsed += (s, e) =>
        {
            tiempoTranscurrido = tiempoTranscurrido.Add(TimeSpan.FromSeconds(1));
            MainThread.BeginInvokeOnMainThread(() =>
            {
                string tiempoStr = tiempoTranscurrido.ToString(@"hh\:mm\:ss");
                cronometroLabel.Text = tiempoStr;
                estadoTiempoCrono.Text = tiempoStr;
            });
        };
    }

    // Método para manejar el evento de inicio del cronómetro
    private void OnStartCronometro(object sender, EventArgs e)
    {
        cronometroTimer.Start();
        ActualizarEstadosActivos();
    }

    // Método para manejar el evento de pausa del cronómetro
    private void OnPauseCronometro(object sender, EventArgs e)
    {
        cronometroTimer.Stop();
        ActualizarEstadosActivos();
    }

    // Método para manejar el evento de reinicio del cronómetro
    private void OnResetCronometro(object sender, EventArgs e)
    {
        cronometroTimer.Stop();
        tiempoTranscurrido = TimeSpan.Zero;
        cronometroLabel.Text = "00:00:00";
        estadoTiempoCrono.Text = "00:00:00";
        vueltasCrono.Clear();
        vueltasCronoLayout.Children.Clear(); // Borra los elementos visuales
        ActualizarEstadosActivos();
    }

    // Método para manejar el evento de registro de vuelta del cronómetro
    private void OnRegistrarVueltaCrono(object sender, EventArgs e)
    {
        string tiempo = tiempoTranscurrido.ToString(@"hh\:mm\:ss");
        string vuelta = $"Vuelta {vueltasCrono.Count + 1}: {tiempo}";
        vueltasCrono.Add(vuelta);

        vueltasCronoLayout.Children.Add(new Label
        {
            Text = vuelta,
            FontSize = 14,
            TextColor = Colors.Gray,
            HorizontalOptions = LayoutOptions.Start
        });
    }

    // ------------------- TEMPORIZADOR -------------------
    // Método para inicializar el temporizador
    private void InicializarTemporizador()
    {
        temporizadorTimer = new System.Timers.Timer(1000);
        temporizadorTimer.Elapsed += (s, e) =>
        {
            if (tiempoRestante.TotalSeconds > 0)
            {
                tiempoRestante = tiempoRestante.Subtract(TimeSpan.FromSeconds(1));
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    string tiempoStr = tiempoRestante.ToString(@"mm\:ss");
                    temporizadorLabel.Text = tiempoStr;
                    estadoTiempoTemp.Text = tiempoStr;
                    entryMinutos.Text = string.Empty;
                    entrySegundos.Text = string.Empty;
                });
            }
            else
            {
                temporizadorTimer.Stop();
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    estadoTiempoTemp.Text = "00:00";
                    // Reproducir audio al terminar el tiempo
                    var audioPlayer = Plugin.Maui.Audio.AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("alarma.mp3"));
                    audioPlayer.Play();
                    var popup = new CustomPopup("El temporizador ha terminado.");
                    this.ShowPopup(popup);
                    audioPlayer.Stop();
                    ActualizarEstadosActivos();
                });
            }
        };
    }

    // Método para manejar el evento de inicio del temporizador
    private void OnStartTemporizador(object sender, EventArgs e)
    {
        if (!int.TryParse(entryMinutos.Text, out int minutos)) minutos = 0;
        if (!int.TryParse(entrySegundos.Text, out int segundos)) segundos = 0;

        tiempoRestante = new TimeSpan(0, minutos, segundos);
        temporizadorLabel.Text = tiempoRestante.ToString(@"mm\:ss");

        temporizadorTimer.Start();
        ActualizarEstadosActivos();
    }

    // Método para manejar el evento de pausa del temporizador
    private void OnStopTemporizador(object sender, EventArgs e)
    {
        temporizadorTimer.Stop();
        ActualizarEstadosActivos();
    }

    // Método para manejar el evento de reinicio del temporizador
    private void OnResetTemporizador(object sender, EventArgs e)
    {
        temporizadorTimer.Stop();
        entryMinutos.Text = string.Empty;
        entrySegundos.Text = string.Empty;
        tiempoRestante = TimeSpan.Zero;
        temporizadorLabel.Text = "00:00";
        estadoTiempoTemp.Text = "00:00";
        vueltasTemp.Clear();
        vueltasTempLayout.Children.Clear(); // Borra los elementos visuales
        ActualizarEstadosActivos();
    }

    //  Método para manejar el evento de registro de vuelta del temporizador
    private void OnRegistrarVueltaTemp(object sender, EventArgs e)
    {
        string tiempo = tiempoRestante.ToString(@"mm\:ss");
        string vuelta = $"Marca {vueltasTemp.Count + 1}: {tiempo}";
        vueltasTemp.Add(vuelta);

        vueltasTempLayout.Children.Add(new Label
        {
            Text = vuelta,
            FontSize = 14,
            TextColor = Colors.Gray,
            HorizontalOptions = LayoutOptions.Start
        });
    }

    // Método para actualizar la visibilidad de los estados activos
    private void ActualizarEstadosActivos()
    {
        bool cronometroActivo = cronometroTimer?.Enabled ?? false;
        bool temporizadorActivo = temporizadorTimer?.Enabled ?? false;

        // Mantener visible si hay tiempo acumulado, aunque esté pausado
        bool mostrarEstadoCrono = cronometroActivo || tiempoTranscurrido.TotalSeconds > 0;
        bool mostrarEstadoTemp = temporizadorActivo || tiempoRestante.TotalSeconds > 0;

        estadoCronometro.IsVisible = mostrarEstadoCrono;
        estadoTemporizador.IsVisible = mostrarEstadoTemp;
        estadoBarLayout.IsVisible = mostrarEstadoCrono || mostrarEstadoTemp;
    }

    //  
    private void IrACronometro(object sender, EventArgs e)
    {
        SeleccionarModo("crono");
    }

    // Método para manejar el evento de selección del temporizador
    private void IrATemporizador(object sender, EventArgs e)
    {
        SeleccionarModo("tempo");
    }
}