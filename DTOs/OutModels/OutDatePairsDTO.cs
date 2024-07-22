namespace FlightSearch.DTOs.OutModels
{
    public class OutDatePairsDTO
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public List<DateOnly> PublicHolidays { get; set;} = new List<DateOnly>();
        public List<DateOnly> Weekends { get; set;} = new List<DateOnly>();
        public List<DateOnly> DaysOff { get; set;} = new List<DateOnly>();
    }
}