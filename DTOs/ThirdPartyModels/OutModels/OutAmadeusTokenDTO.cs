using System.ComponentModel.DataAnnotations;

namespace FlightSearch.DTOs.ThirdPartyModels.OutModels
{
    public class OutAmadeusTokenDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public DateTime ExpiringDate { get; set; }

        public void SetExpirationDate() {
            ExpiringDate = DateTime.UtcNow.AddSeconds(ExpiresIn - 10);
        }
    }
}