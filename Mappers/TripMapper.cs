using FlightSearch.DTOs.InModels;

namespace FlightSearch.Mappers
{
    public static class TripMapper
    {
        public static string TripToChatGPTText(this InTripDTO inTripDTO)
        {
            var tripText = $"Hello ChatGPT, can you generate me a trip itinerary for the following data. Arrival date is {inTripDTO.ToSegments[inTripDTO.ToSegments.Count - 1].Arrival.ToShortDateString()} at {inTripDTO.ToSegments[inTripDTO.ToSegments.Count - 1].Arrival.ToShortTimeString()}. The trip starts at {inTripDTO.ToSegments[inTripDTO.ToSegments.Count - 1].To} airport (IATA code). The flight back is from {inTripDTO.FromSegments[0].From} airport (IATA code) at {inTripDTO.FromSegments[0].Departure.Date.ToShortTimeString()} on {inTripDTO.FromSegments[0].Departure.Date.ToShortDateString()}. Please format the answer with following rules: ";
            tripText += "1. When you speak about the airports use their full name not the IATA code. ";
            tripText += "2. Dont add necessery sentances on the beggining and at the end, begin with day format that is defined by third rule. ";
            tripText += "3. The whole answer is separated by days and write it in this format: five dashes(-----) Day#number - day of the week(date in format yyyy-MM-dd) five dashes(-----). Write everything in the same line. ";
            tripText += "4. Dont send me answer with empty lines. ";
            tripText += "5. For every day write plan in this format: #Ordinal number(reset counter for each day) Attraction name (Google maps coordinates) - data and plan about the attraction. Try to add as many attractions as you can per day but to feel comfortable. ";
            tripText += "Now i am going to give you an example for two day trip and you generate plan like that. ";
            tripText += "-----Day#1 DayOfTheWeek (date)-----(new line)#1 Attraction 1 (lat1, lon1) - plan(new line)#2 Attraction 2 (lat2, lon2) - plan(new line)-----Day#2 DayOfTheWeek (date)-----(new line)#1 Attraction 3 (lat3, lon3) - plan(new line)#2 Attraction 4 (lat4, lon4) - plan(new line)";
            return tripText;
        }
    }
}