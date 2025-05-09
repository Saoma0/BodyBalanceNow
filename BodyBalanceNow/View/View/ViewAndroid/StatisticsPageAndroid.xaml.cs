using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using BodyBalanceNow.Services;
using BodyBalanceNow.Models;
using System.Diagnostics;
using System.ComponentModel;

namespace BodyBalanceNow.View.ViewAndroid
{
    public partial class StatisticsPageAndroid : ContentPage
    {
        private ExerciseDatabaseAndroid _database;
        private int idUsuarioActual; // ID del usuario actual
        private bool usuarioAutenticado = false; // ¿El usuario ha iniciado sesión?

        private List<RegistroSueno> _sueno;
        private List<PesoModel> _peso;
        private List<RegistroEstres> _estres;
        private List<RegistroAgua> _agua;

        private bool _datosCargados = false; // Evitar recarga innecesaria

        public StatisticsPageAndroid()
        {
            InitializeComponent();
            _database = new ExerciseDatabaseAndroid();
            CargarFiltros();
            // Asociar el PropertyChanged de los DropdownField normales
            pickerAño.PropertyChanged += PickerAño_PropertyChanged;
            pickerMes.PropertyChanged += PickerMes_PropertyChanged;
            pickerSemana.PropertyChanged += PickerSemana_PropertyChanged;

            // Asociar el SelectedIndexChanged de pickerGrafica
            pickerGrafica.PropertyChanged += PickerGrafica_SelectedIndexChanged;

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                idUsuarioActual = Preferences.Get("current_user_id", -1);
                usuarioAutenticado = Authenticacion.UsuarioHaIniciadoSesion();

                if (!usuarioAutenticado || idUsuarioActual == -1)
                {
                    await DisplayAlert("Atención", "Debes iniciar sesión para ver estadísticas.", "OK");
                    return;
                }

                await CargarDatos(); // 🔥 Siempre recargar los datos de la base de datos
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnAppearing: {ex.Message}");
            }
        }

        // Actualizar la visibilidad de las gráficas según la selección del usuario
        private void ActualizarGraficasVisibles()
        {
            string seleccion = pickerGrafica.SelectedItem?.ToString();

            bool mostrarTodas = seleccion == "Todas";

            // Mostrar u ocultar cada sección
            ((VisualElement)graficoSueno.Parent.Parent).IsVisible = mostrarTodas || seleccion == "Sueño";
            ((VisualElement)graficoPeso.Parent.Parent).IsVisible = mostrarTodas || seleccion == "Peso";
            ((VisualElement)graficoEstres.Parent.Parent).IsVisible = mostrarTodas || seleccion == "Estrés";
            ((VisualElement)graficoAgua.Parent.Parent).IsVisible = mostrarTodas || seleccion == "Agua";

        }


        // Actualizar la visibilidad de las gráficas según la selección del usuario
        private void PickerGrafica_SelectedIndexChanged(object sender, EventArgs e)
        {
            var seleccion = pickerGrafica.SelectedItem?.ToString();
            bool mostrarTodas = seleccion == "Todas";

            seccionSueno.IsVisible = mostrarTodas || seleccion == "Sueño";
            seccionPeso.IsVisible = mostrarTodas || seleccion == "Peso";
            seccionEstres.IsVisible = mostrarTodas || seleccion == "Estrés";
            seccionAgua.IsVisible = mostrarTodas || seleccion == "Agua";
        }



        // CargarFiltros() se llama al iniciar la página
        private void PickerAño_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedItem")
            {
                CargarSemanasDelMes();
                AplicarFiltros();
            }
        }

        // CargarFiltros() se llama al iniciar la página
        private void PickerMes_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedItem")
            {
                CargarSemanasDelMes();
                AplicarFiltros();
            }
        }

        // CargarFiltros() se llama al iniciar la página
        private void PickerSemana_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedItem")
            {
                AplicarFiltros();
            }
        }

        // CargarFiltros() se llama al iniciar la página
        private async Task CargarDatos()
        {
            _sueno = await _database.ObtenerHistorialSueno(idUsuarioActual);
            _peso = await _database.ObtenerHistorialPeso(idUsuarioActual);
            _estres = await _database.ObtenerHistorialEstres(idUsuarioActual);
            _agua = await _database.ObtenerHistorialAgua(idUsuarioActual);

            AplicarFiltros();
        }

        // CargarFiltros() se llama al iniciar la página
        private void CargarFiltros()
        {
            var años = Enumerable.Range(2020, DateTime.Now.Year - 2020 + 1).ToList();
            pickerAño.ItemsSource = años.Select(a => a.ToString()).ToList();

            var meses = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames
                .Where(m => !string.IsNullOrEmpty(m))
                .ToList();
            pickerMes.ItemsSource = meses;

            pickerSemana.ItemsSource = new List<string>(); // Inicial vacío

            pickerGrafica.ItemsSource = new List<string> { "Todas", "Sueño", "Peso", "Estrés", "Agua" };
            pickerGrafica.SelectedItem = "Todas";

        }

        // CargarFiltros() se llama al iniciar la página
        private void CargarSemanasDelMes()
        {
            if (pickerMes.SelectedItem == null)
                return;

            int.TryParse(pickerAño.SelectedItem?.ToString(), out int añoSeleccionado);
            if (añoSeleccionado == 0)
                añoSeleccionado = DateTime.Now.Year;

            var mesSeleccionado = pickerMes.SelectedItem?.ToString();
            int mes = DateTime.ParseExact(mesSeleccionado, "MMMM", System.Globalization.CultureInfo.CurrentCulture).Month;

            var semanas = new List<string> { "Últimos 15 días" };

            int diasMes = DateTime.DaysInMonth(añoSeleccionado, mes);
            for (int i = 0; i < diasMes; i += 7)
            {
                int semanaNumero = (i / 7) + 1;
                semanas.Add($"{semanaNumero}ª semana de {mesSeleccionado}");
            }

            pickerSemana.ItemsSource = semanas;
            pickerSemana.SelectedItem = "Últimos 15 días"; // De primeras
        }

        // CargarFiltros() se llama al iniciar la página
        private void AplicarFiltros()
        {
            int.TryParse(pickerAño.SelectedItem?.ToString(), out int añoSeleccionado);
            var mesSeleccionado = pickerMes.SelectedItem?.ToString();
            var semanaSeleccionada = pickerSemana.SelectedItem?.ToString();

            DateTime? fechaInicio = null;
            DateTime? fechaFin = null;

            if (!string.IsNullOrEmpty(semanaSeleccionada) && semanaSeleccionada != "Últimos 15 días")
            {
                if (string.IsNullOrEmpty(mesSeleccionado))
                    return;

                int mes = DateTime.ParseExact(mesSeleccionado, "MMMM", System.Globalization.CultureInfo.CurrentCulture).Month;
                int semanaNumero = int.Parse(semanaSeleccionada.Split('ª')[0]);

                fechaInicio = new DateTime(añoSeleccionado, mes, 1).AddDays((semanaNumero - 1) * 7);
                fechaFin = fechaInicio.Value.AddDays(6);

                var finMes = new DateTime(añoSeleccionado, mes, DateTime.DaysInMonth(añoSeleccionado, mes));
                if (fechaFin > finMes)
                    fechaFin = finMes;
            }
            else
            {
                // 🧠 Nuevo comportamiento para "Todo el mes" ➔ mostrar últimos 15 días
                // Combinamos todos los datos que tenemos para buscar el último día

                var todasFechas = _sueno.Select(s => s.Fecha)
                    .Concat(_peso.Select(p => p.Fecha))
                    .Concat(_estres.Select(e => e.Fecha))
                    .Concat(_agua.Select(a => a.Fecha))
                    .ToList();

                if (todasFechas.Count > 0)
                {
                    var fechaMaxima = todasFechas.Max().Date;
                    fechaFin = fechaMaxima;
                    fechaInicio = fechaFin.Value.AddDays(-14); // últimos 15 días
                }
            }

            var suenoFiltrado = FiltrarDatos(_sueno, s => s.Fecha, fechaInicio, fechaFin);
            var pesoFiltrado = FiltrarDatos(_peso, p => p.Fecha, fechaInicio, fechaFin);
            var estresFiltrado = FiltrarDatos(_estres, e => e.Fecha, fechaInicio, fechaFin);
            var aguaFiltrado = FiltrarDatos(_agua, a => a.Fecha, fechaInicio, fechaFin);

            PintarGraficoSueno(suenoFiltrado);
            PintarGraficoPeso(pesoFiltrado);
            PintarGraficoEstres(estresFiltrado);
            PintarGraficoAgua(aguaFiltrado);
        }

        // FiltrarDatos() se llama al aplicar los filtros
        private List<T> FiltrarDatos<T>(List<T> lista, Func<T, DateTime> selectorFecha, DateTime? inicio, DateTime? fin)
        {
            if (inicio == null || fin == null)
                return lista.OrderByDescending(selectorFecha).ToList();

            return lista
                .Where(item => selectorFecha(item).Date >= inicio.Value.Date && selectorFecha(item).Date <= fin.Value.Date)
                .OrderByDescending(selectorFecha)
                .ToList();
        }

        // PintarGraficoSueno() se llama al aplicar los filtros
        private void PintarGraficoSueno(List<RegistroSueno> datos)
        {
            graficoSueno.Children.Clear();
            graficoSueno.ColumnDefinitions.Clear();

            if (datos == null || datos.Count == 0)
                return;

            // Agrupar por día
            var datosAgrupados = datos
                .GroupBy(d => d.Fecha.Date)
                .Select(g => new
                {
                    Fecha = g.Key,
                    HorasTotal = g.Sum(x => x.HorasDormidas)
                })
                .OrderBy(x => x.Fecha)
                .Take(30)
                .ToList();

            for (int i = 0; i < datosAgrupados.Count; i++)
            {
                graficoSueno.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }

            // Definir el máximo entre el objetivo y el dato más alto para escalar
            double objetivoDiario = 8.0; // Objetivo sueño diario (puedes cambiarlo si quieres)
            double maximo = Math.Max(objetivoDiario, datosAgrupados.Max(d => d.HorasTotal));

            for (int i = 0; i < datosAgrupados.Count; i++)
            {
                var dia = datosAgrupados[i];
                double porcentaje = dia.HorasTotal / maximo;
                int altura = (int)(porcentaje * 140);

                // Color basado en las horas dormidas
                Color colorBarra;
                if (dia.HorasTotal >= 8.0)
                    colorBarra = Colors.MediumSeaGreen; // Nivel Alto
                else if (dia.HorasTotal >= 6.0)
                    colorBarra = Colors.Orange; // Nivel Medio
                else
                    colorBarra = Colors.LightCoral; // Nivel Bajo

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
                graficoSueno.Children.Add(stack);
            }
        }

        // PintarGraficoPeso() se llama al aplicar los filtros
        private void PintarGraficoPeso(List<PesoModel> datos)
        {
            graficoPeso.Children.Clear();
            graficoPeso.ColumnDefinitions.Clear();

            if (datos == null || datos.Count == 0)
                return;

            // Agrupar por fecha
            var datosAgrupados = datos
                .GroupBy(d => d.Fecha.Date)
                .Select(g => new
                {
                    Fecha = g.Key,
                    PesoPromedio = g.Average(x => (double)x.Peso)
                })
                .OrderBy(x => x.Fecha)
                .Take(30)
                .ToList();

            if (datosAgrupados.Count == 0)
                return;

            // Calcular mínimo y máximo para escalar
            double min = datosAgrupados.Min(d => d.PesoPromedio);
            double max = datosAgrupados.Max(d => d.PesoPromedio);
            double rango = max - min;

            for (int i = 0; i < datosAgrupados.Count; i++)
            {
                graficoPeso.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }

            for (int i = 0; i < datosAgrupados.Count; i++)
            {
                var dia = datosAgrupados[i];

                double altura = (rango == 0)
                    ? 80
                    : Math.Max(20, (dia.PesoPromedio - min) / rango * 120);

                // Color basado en el valor
                Color colorBarra;
                if (dia.PesoPromedio >= max)
                    colorBarra = Colors.Red; // Mayor peso
                else if (dia.PesoPromedio <= min)
                    colorBarra = Colors.Green; // Menor peso
                else
                    colorBarra = Color.FromArgb("#FFB300"); // Amarillo intermedio

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
                graficoPeso.Children.Add(stack);
            }
        }

        // PintarGraficoEstres() se llama al aplicar los filtros
        private void PintarGraficoEstres(List<RegistroEstres> datos)
        {
            graficoEstres.Children.Clear();
            graficoEstres.ColumnDefinitions.Clear();

            if (datos == null || datos.Count == 0)
                return;

            // Agrupar por día
            var datosAgrupados = datos
                .GroupBy(d => d.Fecha.Date)
                .Select(g => new
                {
                    Fecha = g.Key,
                    NivelPromedio = (int)Math.Round(g.Average(x => x.Nivel))
                })
                .OrderBy(x => x.Fecha)
                .Take(30) // Mostramos como máximo 30 días
                .ToList();

            // Crear columnas dinámicamente
            for (int i = 0; i < datosAgrupados.Count; i++)
            {
                graficoEstres.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }

            double maximo = 10.0; // Nivel máximo de estrés

            for (int i = 0; i < datosAgrupados.Count; i++)
            {
                var dia = datosAgrupados[i];
                double porcentaje = dia.NivelPromedio / maximo;
                int altura = (int)(porcentaje * 140);

                // Color de la barra
                Color colorBarra;
                if (porcentaje >= 0.75)
                    colorBarra = Colors.OrangeRed; // Estrés alto
                else if (porcentaje >= 0.25)
                    colorBarra = Color.FromHex("#FFB300"); // Medio
                else
                    colorBarra = Colors.Teal; // Bajo

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
                graficoEstres.Children.Add(stack);
            }
        }

        // PintarGraficoAgua() se llama al aplicar los filtros
        private void PintarGraficoAgua(List<RegistroAgua> datos)
        {
            graficoAgua.Children.Clear();
            graficoAgua.ColumnDefinitions.Clear();

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
                graficoAgua.Children.Add(mensaje);
                return;
            }

            // Agrupar por día
            var datosAgrupados = datos
                .GroupBy(d => d.Fecha.Date)
                .Select(g => new
                {
                    Fecha = g.Key,
                    TotalAgua = g.Sum(x => x.Cantidad)
                })
                .OrderBy(x => x.Fecha)
                .Take(30)
                .ToList();

            int maximo = Math.Max(2000, datosAgrupados.Max(d => d.TotalAgua));

            for (int i = 0; i < datosAgrupados.Count; i++)
            {
                graficoAgua.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                var dia = datosAgrupados[i];

                double porcentaje = (double)dia.TotalAgua / 2000;
                porcentaje = Math.Clamp(porcentaje, 0, 1);

                int altura = (int)((double)dia.TotalAgua / maximo * 140);

                Color colorBarra;
                if (porcentaje >= 1)
                    colorBarra = Colors.MediumSeaGreen; // Verde
                else if (porcentaje >= 0.75)
                    colorBarra = Color.FromHex("#FFB300"); // Amarillo
                else
                    colorBarra = Colors.Red; // Rojo

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
                graficoAgua.Children.Add(stack);
            }
        }
    }
}