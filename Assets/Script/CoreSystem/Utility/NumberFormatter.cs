using UnityEngine;

namespace CoreSystem.Utility
{
    public static class NumberFormatter
    {
        /// <summary>
        /// Formats a number to a string with K and M suffixes.
        /// 1000 -> 1000
        /// 9999 -> 9999
        /// 10000 -> 10K
        /// 100000 -> 100K
        /// 1000000 -> 1M
        /// </summary>
        public static string Format(long value)
        {
            if (value < 10000)
            {
                return value.ToString();
            }

            if (value < 1000000)
            {
                double kValue = value / 1000.0;
                // If it's something like 10.5K, maybe show 1 decimal? 
                // But the request says 10K, 100K. Let's stick to whole numbers for simplicity unless it's very large.
                // Re-reading: "10K แล้วก็ 100K - 999K และก็ไป 1M - 999M"
                return $"{(int)kValue}K";
            }

            if (value < 1000000000)
            {
                double mValue = value / 1000000.0;
                // Check if it's a whole number or needs decimals. Typically M shows 1 decimal if needed e.g. 1.2M
                // But following the request's pattern: "1M - 999M"
                return $"{(int)mValue}M";
            }

            double bValue = value / 1000000000.0;
            return $"{(int)bValue}B";
        }

        public static string Format(int value)
        {
            return Format((long)value);
        }

        public static string Format(float value)
        {
            return Format((long)value);
        }

        /// <summary>
        /// Formats a number with comma separators. 
        /// 1000 -> 1,000
        /// 1000000 -> 1,000,000
        /// </summary>
        public static string FormatWithCommas(long value)
        {
            return value.ToString("N0");
        }

        public static string FormatWithCommas(int value)
        {
            return FormatWithCommas((long)value);
        }

        public static string FormatWithCommas(float value)
        {
            return FormatWithCommas((long)value);
        }
    }
}
