namespace Task_Manager_Care.Helpers
{
    public class DateTimeHelper
    {
        private static readonly TimeZoneInfo CanadaEastern =
            TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        public static DateTime ToEastern(DateTime utcDate)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(utcDate, DateTimeKind.Utc),
                CanadaEastern);
        }
    }
}
