using System;

using Markt2Go.Shared.Extensions;

namespace Markt2Go.DTOs.Market
{
    public class GetMarketDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int DayOfWeek { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Location { get; set; }
        public bool Visible { get; set; }

        public DateTime Next
        {
            get
            {
                // calc day
                DateTime next = DateTime.Now.GetNextDate((DayOfWeek)DayOfWeek);
                return next.SetTime(StartTime, DateTimeKind.Local).ToUniversalTime();
            }
        }
    }
}