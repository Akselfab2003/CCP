using OllamaSharp;

namespace ChatService.Infrastructure.LLM.Tools
{
    /// <summary>
    /// 
    /// </summary>
    public class ContactSupporter
    {
        /// <summary>
        /// Contact a supporter for assistance. When you can't find a relevant FAQ or the chatbot is unable to assist with the user's query, this tool can be used to escalate the issue to a human supporter.
        /// </summary>
        /// <returns></returns>
        [OllamaTool]
        public static string ContactSupporterAgent()
        {
            // Here you would implement the logic to contact a supporter, such as sending an email or creating a support ticket.
            // For demonstration purposes, we'll just return a success message.
            return "A supporter has been contacted and will reach out to you shortly.";
        }



        /// <summary>
        /// Get the current weather for a city
        /// </summary>
        /// <param name="city">Name of the city</param>
        /// <param name="unit">Temperature unit for the weather</param>
        [OllamaTool]
        public static string GetWeather(string city, Unit unit = Unit.Celsius) => $"It's cold at only 6° {unit} in {city}.";

        public enum Unit
        {
            Celsius,
            Fahrenheit
        }
    }
}
