using System;

namespace Markt2Go.Shared.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime GetNextDate(this DateTime dateTime, DayOfWeek dayOfWeek)
        {
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysUntilNext = ((int)dayOfWeek - (int)dateTime.DayOfWeek + 7) % 7;
            return dateTime.AddDays(daysUntilNext);
        }

        public static DateTime SetTime(this DateTime date, string time, DateTimeKind dateTimeKind = DateTimeKind.Unspecified)
        {
            DateTime parsedTime;
            if (!DateTime.TryParseExact(time, "H:mm", null, System.Globalization.DateTimeStyles.None, out parsedTime))
            {
                throw new FormatException($"Time '{time}' could not be parsed into a System.DateTime");
            }
            return date.SetTime(parsedTime, dateTimeKind);
        }
        public static DateTime SetTime(this DateTime date, DateTime time, DateTimeKind dateTimeKind = DateTimeKind.Unspecified)
        {
            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Millisecond, dateTimeKind);
        }
    }
}