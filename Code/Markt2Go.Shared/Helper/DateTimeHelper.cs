using System;
using System.Collections.Generic;

namespace Markt2Go.Shared.Helper
{
    public static class DateTimeHelper
    {
        public static List<DateTime> GetTimeslots(DateTime start, DateTime end, TimeSpan slotSize)
        {
            List<DateTime> timeslots = new List<DateTime>();
            var timeslot = start;
            while (timeslot <= end)
            {
                timeslots.Add(timeslot);
                timeslot = timeslot.Add(slotSize);
            }
            return timeslots;
        }
    }
}