using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace WD.Models.WDModels
{
    // Define una clase que representa el resultado de datos meteorologicos
    public class WeatherResult
    {
        // Nombre de la ciudad o localidad
        // Esta propiedad se asocia al campo "name" del JSON
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        // Nombre del pais
        // Se asocia al campo "country" del JSON
        [JsonPropertyName("country")]
        public string? Country { get; set; }

        // Estado principal del clima (ej. "Clear", "Rain")
        // Se asocia al campo "main" del JSON
        [JsonPropertyName("main")]
        public string? WeatherMain { get; set; }

        // Descripcion detallada del clima (ej. "cielo claro")
        // Se asocia al campo "description" del JSON
        [JsonPropertyName("description")]
        public string? WeatherDescription { get; set; }

        // Temperatura en grados Celsius
        // Se asocia al campo "temp" del JSON
        [JsonPropertyName("temp")]
        public double? Temperature { get; set; }

        // Humedad relativa en porcentaje
        // Se asocia al campo "humidity" del JSON
        [JsonPropertyName("humidity")]
        public int? Humidity { get; set; }
    }


}
