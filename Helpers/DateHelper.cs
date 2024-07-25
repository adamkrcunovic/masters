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

        public static string LayoverStringInFormat(DateTime dateTime1, DateTime dateTime2)
        {
            TimeSpan dateDiff = dateTime1 - dateTime2;
            int hours = Convert.ToInt32(Math.Floor(dateDiff.TotalHours));
            int minutes = dateDiff.Minutes;
            return (hours > 0 ? (hours > 1 ? hours + "Hours " : "1Hour ") : "") + (minutes > 0 ? minutes + "Minutes" : "");
        }

        public static string MinutesToTimeFormat(int minutesRequest)
        {
            int hours = minutesRequest / 60;
            int minutes = minutesRequest % 60;
            return (hours > 0 ? (hours > 1 ? hours + "Hours " : "1Hour ") : "") + (minutes > 0 ? minutes + "Minutes" : "");
        }

        public static string MinutesToTimeFormatWithDays(int minutesRequest)
        {
            int days = minutesRequest / 1440;
            int hours = minutesRequest / 60 % 24;
            int minutes = minutesRequest % 60;
            return (days > 0 ? (days > 1 ? days + "Days " : "1Day ") : "") + (hours > 0 ? (hours > 1 ? hours + "Hours " : "1Hour ") : "") + (minutes > 0 ? minutes + "Minutes" : "");
        }

        public static int MinutesDifference(DateTime dateTime1, DateTime dateTime2)
        {
            return Convert.ToInt32(Math.Floor((dateTime1 - dateTime2).TotalMinutes));
        }

        public static bool MinutesDifferenceMoreThan12Hours(DateTime dateTime1, DateTime dateTime2)
        {
            return Convert.ToInt32(Math.Floor((dateTime1 - dateTime2).TotalMinutes)) > 720;
        }
    }
}