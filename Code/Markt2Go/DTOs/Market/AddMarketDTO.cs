using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Markt2Go.DTOs.Market
{
    public class AddMarketDTO : IValidatableObject
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        public int DayOfWeek { get; set; }
        [Required]
        [StringLength(5)]
        public string StartTime { get; set; }
        [Required]
        [StringLength(5)]
        public string EndTime { get; set; }
        [Required]
        [StringLength(100)]
        public string Location { get; set; }
        public bool Visible { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            DateTime startTime, endTime;

            // check if starttime is valid
            if (!DateTime.TryParseExact(StartTime, "H:mm", null, System.Globalization.DateTimeStyles.None, out startTime))
            {
                yield return new ValidationResult(
                    $"{nameof(StartTime)} must be a valid time in ##:## format.",
                    new[] { nameof(StartTime) });
            }

            // check if endtime is valid
            if (!DateTime.TryParseExact(EndTime, "H:mm", null, System.Globalization.DateTimeStyles.None, out endTime))
            {
                yield return new ValidationResult(
                    $"{nameof(EndTime)} must be a valid time in ##:## format.",
                    new[] { nameof(EndTime) });
            }

            // check if starttime is before endtime
            if (startTime > endTime)
            {
                yield return new ValidationResult(
                    $"{nameof(StartTime)} needs to be before {nameof(EndTime)}.",
                    new[] { nameof(StartTime), nameof(EndTime) });
            }
        }
    }
}