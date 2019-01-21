using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Juna.Feed.WebApi.Helpers
{
    public static class BoardValidityHelper
    {
        public static bool StartTimeInInterval(DateTime dt1, DateTime dt2, int interval)
        {
            TimeSpan start = new TimeSpan(dt1.Hour, dt1.Minute, dt1.Second);
            TimeSpan end = new TimeSpan(dt2.Hour, dt2.Minute, dt2.Second);
            TimeSpan now = DateTime.Now.TimeOfDay;

            TimeSpan diff = end - start;

            var hours = diff.Hours;

            if (hours <= interval)
            {
                return true;
            }

            return false;
        }

        public static bool EndTimeInInterval(DateTime dt1, DateTime dt2, int interval)
        {
            TimeSpan diff = dt2 - dt1;

            var hours = diff.Hours;

            if (hours > interval)
            {
                return false;
            }

            return true;
        }

        public static bool IsBetween(this DateTime now, DateTime startDate, DateTime endDate)
        {
            TimeSpan start = new TimeSpan(startDate.Hour, startDate.Minute, startDate.Second);
            TimeSpan end = new TimeSpan(endDate.Hour, endDate.Minute, endDate.Second);
            var time = now.TimeOfDay;

            // If the start time and the end time is in the same day.
            if (start < end)
                return time >= start && time <= end;
            
            // The start time and end time is on different days.
            return time >= start || time <= end;
        }
    }
}
