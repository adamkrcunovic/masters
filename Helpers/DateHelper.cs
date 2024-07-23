namespace FlightSearch.Helpers
{
    public static class DateHelper
    {
        public static bool IsDateWithinYear(this DateOnly? date)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var todayYearAfter = today.AddYears(1);
            return date != null && date >= today && date < todayYearAfter;
        }

        public static DateOnly Today() {
            return DateOnly.FromDateTime(DateTime.Now);
        }

        public static string RenameHourString(string hourAndMinutes)
        {
            var returnString = hourAndMinutes.Remove(0, 2);
            var HIndex = returnString.IndexOf('H');
            if (HIndex != -1)
            {
                returnString = returnString.Insert(HIndex + 1, "our" + (HIndex != 1 || returnString[0] != '1' ? "s" : "") + " ");
            }
            var MIndex = returnString.IndexOf('M');
            if (MIndex != -1)
            {
                returnString = returnString.Insert(MIndex + 1, "inutes");
            }
            return returnString;
        }
    }
}