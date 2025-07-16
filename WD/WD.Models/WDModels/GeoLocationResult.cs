using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WD.Models.WDModels
{
    // Define una clase que representa el resultado de la API de geolocalizacion de OpenWeatherMap
    public class GeoLocationResult
    {
        // Asocia esta propiedad con el campo "name" del JSON recibido
        [JsonPropertyName("name")]
        public string Name { get; set; }

        // Asocia esta propiedad con el campo "lat" (latitud) del JSON recibido
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        // Asocia esta propiedad con el campo "lon" (longitud) del JSON recibido
        [JsonPropertyName("lon")]
        public double Lon { get; set; }

        // Asocia esta propiedad con el campo "country" del JSON recibido
        [JsonPropertyName("country")]
        public string Country { get; set; }
    }
}
