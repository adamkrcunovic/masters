namespace FlightSearch.Interfaces
{
    public interface ITripRepository
    {
        public Task<string> GetChatGPTData(string inputText);
    }
}