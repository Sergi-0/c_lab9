using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;

namespace lab9_2
{
    public partial class Form1 : Form
    {
        private readonly string API_KEY = "db6642f622789c2dbe748c0ab3e0b176";
        private Dictionary<string, (double lat, double lon)> cities = new Dictionary<string, (double, double)>();
        private Label resultLabel;

        public Form1()
        {
            InitializeComponent();
            LoadCities();
            InitializeResultLabel();
        }

        private void InitializeResultLabel()
        {
            resultLabel = new Label
            {
                Location = new Point(271, 150),
                Size = new Size(454, 100),
                Font = new Font("Microsoft Sans Serif", 12F),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(resultLabel);
        }

        private void LoadCities()
        {
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "city.txt");
                var lines = File.ReadAllLines(filePath);

                foreach (var line in lines)
                {
                    string[] parts = line.Split('\t'); // разделяет строку на 2 строки, между которыми ранее была табуляция
                    string cityName = parts[0].Trim(); // удаляет лишние пробелы
                    string coordinates = parts[1].Trim();
                    string[] coordParts = coordinates.Split(',');

                    // Используем инвариантную культуру для правильного преобразования чисел. Т.к для convert нужна именно запятая, а у нас точки.
                    double lat = Convert.ToDouble(coordParts[0].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                    double lon = Convert.ToDouble(coordParts[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                    cities[cityName] = (lat, lon);
                    comboBox1.Items.Add(cityName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки городов: {ex.Message}");
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                button1.Enabled = false;
                resultLabel.Text = "Загрузка...";

                var coordinates = cities[comboBox1.Text];
                var weather = await GetWeatherAsync(coordinates.lat, coordinates.lon);

                resultLabel.Text = $"Погода в {weather.Name}, {weather.Country}:\n" +
                                 $"Температура: {(weather.Temp - 273.15):F1}°C\n" +
                                 $"Описание: {weather.Description}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
            finally
            {
                button1.Enabled = true;
            }
        }

        private async Task<Weather> GetWeatherAsync(double latitude, double longitude)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid={API_KEY}";
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                WeatherData weatherData = JsonConvert.DeserializeObject<WeatherData>(responseBody);

                return new Weather
                {
                    Country = weatherData.sys.country,
                    Name = weatherData.name,
                    Temp = weatherData.main.temp,
                    Description = weatherData.weather[0].description
                };
            }
        }
    }

    public struct Weather
    {
        public string Country { get; set; }
        public string Name { get; set; }
        public double Temp { get; set; }
        public string Description { get; set; }
    }

    public class WeatherData
    {
        public Sys sys { get; set; }
        public string name { get; set; }
        public Main main { get; set; }
        public WeatherOBJ[] weather { get; set; }
    }

    public class Sys
    {
        public string country { get; set; }
    }

    public class Main
    {
        public double temp { get; set; }
    }

    public class WeatherOBJ
    {
        public string description { get; set; }
    }
}