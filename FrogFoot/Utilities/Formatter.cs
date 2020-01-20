using System;

namespace FrogFoot.Utilities
{
    public static class Formatter
    {
        public static string AsRands(this decimal? amount)
        {
            return string.Format("{0:#,##0.00}", amount);
        }

        public static string Date(this DateTime date)
        {
            return date.ToString("dd/MM/yyyy");
        }

        public static string Date(this DateTime? date)
        {
            return date != null ? date.Value.ToString("dd/MM/yyyy") : "";
        }
    }
}