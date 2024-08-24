namespace FlightSearch.Constants
{
    public static class ApplicationConstants
    {
        public const string DateOnlyPattern = "yyyy-MM-dd";
        public const string TokenCacheString = "token";
        public const string TokenAmadeusApiAddress = "https://test.api.amadeus.com/v1/security/oauth2/token";
        public const string OpenAIApiAddress = "https://api.openai.com/v1/chat/completions";
        public const string GoogleCloudMessagingApiAddress = "https://fcm.googleapis.com/v1/projects/masters-362af/messages:send";
        public const string FlightOffersAmadeusApiAddress = "https://test.api.amadeus.com/v2/shopping/flight-offers?";
        public const string AirporstAmadeusApiAddress = "https://test.api.amadeus.com/v1/reference-data/locations?subType=AIRPORT&keyword=";
    }
}