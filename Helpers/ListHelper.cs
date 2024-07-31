using Microsoft.IdentityModel.Tokens;

namespace FlightSearch.Helpers
{
    public static class ListHelper
    {
        public static string? ListToString(this List<string>? list)
        {
            if (list.IsNullOrEmpty())
            {
                return null;
            }
            var returnString = "";
            foreach(var listObject in list?? new List<string>())
            {
                returnString += listObject + ";";
            }
            return returnString;
        }
    }
}