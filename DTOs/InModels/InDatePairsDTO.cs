using System.ComponentModel.DataAnnotations;

namespace FlightSearch.DTOs.InModels
{
    public class InDatePairsDTO
    {
        [Range(3, 23, ErrorMessage = "Please select how long would you like to go on holiday, between 3 and 23")]
        public int TotalDays { get; set; }

        [Range(0, 14, ErrorMessage = "Please select amount of days off that you will take, between 0 and 14")]
        public int DaysOff { get; set; }
    }
}