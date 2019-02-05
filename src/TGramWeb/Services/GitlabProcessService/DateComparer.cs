using System;
using System.Globalization;

namespace TGramWeb.Services.GitlabProcessService
{
    internal static class DateComparer
    {
        public static bool DateStringsMatch(string date1, string date2)
        {
            bool CompareDates(ref DateTime d1, ref DateTime d2)
            {
                TimeSpan ts = d2.Subtract(d1);
                bool match = Math.Abs(ts.TotalSeconds) <= 5;
                return match;
            }

            bool result;
            if (string.IsNullOrEmpty(date1) && string.IsNullOrEmpty(date2))
            {
                result = true;
            }
            else if (string.IsNullOrEmpty(date1))
            {
                result = false;
            }
            else if (string.IsNullOrEmpty(date2))
            {
                result = false;
            }
            else if (string.Equals(date1, date2, StringComparison.InvariantCultureIgnoreCase))
            {
                result = true;
            }
            else if (DateTime.TryParse(date1, out DateTime d1) && DateTime.TryParse(date2, out DateTime d2))
            {
                result = CompareDates(ref d1, ref d2);
            }
            else
            {
                try
                {
                    d1 = DateTime.ParseExact(date1, "yyyy-MM-dd HH:mm:ss UTC", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal);
                    d2 = DateTime.ParseExact(date2, "yyyy-MM-dd HH:mm:ss UTC", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal);
                    result = CompareDates(ref d1, ref d2);
                }
                catch (FormatException)
                {
                    result = false;
                }
            }

            return result;
        }
    }
}