using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral.CustomTypes
{
    /// <summary>
    /// Represents a Time with no date because, for whatever reason, the .NET framework doesn't have that.
    /// </summary>
    public class Time
    {
        /// <summary>
        /// The hours component.
        /// </summary>
        public int Hours { get; set; }

        /// <summary>
        /// The minutes component.
        /// </summary>
        public int Minutes { get; set; }

        /// <summary>
        /// The seconds component.
        /// </summary>
        public int Seconds { get; set; }

        /// <summary>
        /// Creates a new time object using the given hours, minutes and seconds.
        /// </summary>
        /// <param name="h"></param>
        /// <param name="m"></param>
        /// <param name="s"></param>
        public Time(uint h, uint m, uint s)
        {
            if (h > 23 || m > 59 || s > 59)
            {
                throw new ArgumentException("Invalid time specified");
            }
            Hours = (int)h; Minutes = (int)m; Seconds = (int)s;
        }

        /// <summary>
        /// Creates a new time object from the given date time.
        /// </summary>
        /// <param name="dt"></param>
        public Time(DateTime dt)
        {
            Hours = dt.Hour;
            Minutes = dt.Minute;
            Seconds = dt.Second;
        }

        /// <summary>
        /// Creates a new time object.
        /// </summary>
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

            if (!Int32.TryParse(elements[0], out int hours) || hours < 0 || hours > 24)
                return false;

            if (!Int32.TryParse(elements[1], out int minutes) || minutes < 0 || minutes > 59)
                return false;

            if (!Int32.TryParse(elements[2], out int seconds) || seconds < 0 || seconds > 59)
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

        /// <summary>
        /// Returns the total number of seconds represented by this Time object.
        /// </summary>
        /// <returns></returns>
        public int GetSeconds()
        {
            return (Hours * 3600) + (Minutes * 60) + Seconds;
        }
    }
}
