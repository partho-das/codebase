namespace AIBackend.Models
{
    public class GetWeatherInput
    {
        public GetWeatherInput(string city)
        {
            City = city;
        }
        public string City { get; set; }
    }

    public class GetWeatherOutput
    {
        public string City { get; set; } = "";
        public string Condition { get; set; } = "";
        public int TemperatureCelsius { get; set; }
    }
}
