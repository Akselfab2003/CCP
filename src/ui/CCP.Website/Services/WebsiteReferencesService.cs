namespace CCP.Website.Services
{
    public class WebsiteReferencesService : IWebsiteReferencesService
    {
        private string SassServiceUrl { get; set; }

        public WebsiteReferencesService(string sassServiceUrl)
        {
            SassServiceUrl = sassServiceUrl;
        }

        public string Login => $"{SassServiceUrl}/authentication/login?returnUrl=/dashboard";

        public string Register => $"/Register";

    }
}
