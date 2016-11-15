using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CustomDBTypes
{
    /// <summary>
    /// Represents a Time with no date because, for whatever reason, the .NET framework doesn't have that.
    /// </summary>
    public class Time
    {
        public int Hours { get; private set; }
        public int Minutes { get; private set; }
        public int Seconds { get; private set; }

        public Time(uint h, uint m, uint s)
        {
            if (h > 23 || m > 59 || s > 59)
            {
                throw new ArgumentException("Invalid time specified");
            }
            Hours = (int)h; Minutes = (int)m; Seconds = (int)s;
        }

        public Time(DateTime dt)
        {
            Hours = dt.Hour;
            Minutes = dt.Minute;
            Seconds = dt.Second;
        }

        public Time()
        {
        }

        /// <summary>
        /// Returns a string formatting it like so : "{0:00}:{1:00}:{2:00}"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format(
                "{0:00}:{1:00}:{2:00}",
                this.Hours, this.Minutes, this.Seconds);
        }

        /// <summary>
        /// Turns a string in the format 00:00:00 into a Time object.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool TryParse(string input, out Time time)
        {
            time = null;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            var elements = input.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            
            if (elements.Count != 3)
                return false;

            int hours, minutes, seconds;

            if (!Int32.TryParse(elements[0], out hours) || hours < 0 || hours > 24)
                return false;

            if (!Int32.TryParse(elements[1], out minutes) || minutes < 0 || minutes > 59)
                return false;

            if (!Int32.TryParse(elements[2], out seconds) || seconds < 0 || seconds > 59)
                return false;

            //All of our parsing went well!  Success!
            time = new Time
            {
                Hours = hours,
                Minutes = minutes,
                Seconds = seconds
            };

            return true;
        }

        public int GetSeconds()
        {
            return (Hours * 3600) + (Minutes * 60) + Seconds;
        }
    }
}
