using System;

namespace TGramWeb.Services.GitlabProcessService
{
    internal static class DateComparer
    {
        public static bool DateStringsMatch(string date1, string date2)
        {
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
                TimeSpan ts = d2.Subtract(d1);
                result = Math.Abs(ts.TotalSeconds) <= 5;
            }
            else
            {
                result = false;
            }
            return result;
        }
    }
}
