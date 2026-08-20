namespace Task_Manager_Care.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly TimeZoneInfo CanadaEastern =
            GetEasternZone();

        private static TimeZoneInfo GetEasternZone()
        {
            try
            {
                //WINDWS ID
                return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                //Linux/macOS ID
                return TimeZoneInfo.FindSystemTimeZoneById("America/Toronto");
            }
        }
        public static DateTime ToEastern(DateTime utcDate)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(utcDate, DateTimeKind.Utc),
                CanadaEastern);
        }
    }
}
