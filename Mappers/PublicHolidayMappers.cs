using FlightSearch.Database.Models;
using FlightSearch.DTOs.InModels;
using FlightSearch.DTOs.OutModels;

namespace FlightSearch.Mappers
{
    public static class PublicHolidayMapper
    {
        public static PublicHoliday ToPublicHolidayFromInDTO(this InPublicHolidayDTO inPublicHolidayDTO)
        {
            return new PublicHoliday
            {
                CountryId = inPublicHolidayDTO.CountryId,
                HolidayDate = inPublicHolidayDTO.HolidayDate
            };
        }

        public static List<OutDatePairsDTO> HolidaysToDatePairs(List<DateOnly>? publicHolidays, InDatePairsDTO inDatePairsDTO) {
            var dateToday = DateOnly.FromDateTime(DateTime.Now);
            var listOfDatePairs = new List<OutDatePairsDTO>();
            if (publicHolidays != null)
            {
                foreach (DateOnly publicHolidayDate in publicHolidays)
                {
                    for(int i = 0; i < inDatePairsDTO.TotalDays; i++)
                    {
                        DateOnly startingDate = publicHolidayDate.AddDays(-i);
                        DateOnly endDate = startingDate.AddDays(inDatePairsDTO.TotalDays - 1);
                        if (startingDate.CompareTo(dateToday) <= 0)
                        {
                            break;
                        }
                        int publicHolidayAndWeekendCount = 0;
                        DateOnly currentDate = startingDate;
                        List<DateOnly> publicHolidaysInRange = new List<DateOnly>();
                        List<DateOnly> weekendsInRange = new List<DateOnly>();
                        List<DateOnly> daysOffInRange = new List<DateOnly>();
                        while (currentDate.CompareTo(endDate) <= 0)
                        {
                            if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday || publicHolidays.Contains(currentDate))
                            {
                                publicHolidayAndWeekendCount++;
                                if (publicHolidays.Contains(currentDate))
                                {
                                    publicHolidaysInRange.Add(currentDate);
                                }
                                else
                                {
                                    weekendsInRange.Add(currentDate);
                                }
                            }
                            else
                            {
                                daysOffInRange.Add(currentDate);
                            }
                            currentDate = currentDate.AddDays(1);
                        }
                        if (inDatePairsDTO.TotalDays <= inDatePairsDTO.DaysOff + publicHolidayAndWeekendCount)
                        {
                            if (listOfDatePairs.Where(datePair => datePair.StartDate.Equals(startingDate)).ToList().Count == 0)
                            {
                                listOfDatePairs.Add(new OutDatePairsDTO()
                                {
                                    StartDate = startingDate,
                                    EndDate = endDate,
                                    PublicHolidays = publicHolidaysInRange,
                                    Weekends = weekendsInRange,
                                    DaysOff = daysOffInRange
                                });
                            }
                        }
                    }
                }
            }
            listOfDatePairs.Sort((x, y) => x.StartDate.CompareTo(y.StartDate));
            return listOfDatePairs;
        }
    }
}