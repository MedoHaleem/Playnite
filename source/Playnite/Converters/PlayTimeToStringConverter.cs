using Playnite.SDK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.Converters
{
    public class PlayTimeToStringConverter : MarkupExtension, IValueConverter
    {
        public static PlayTimeToStringConverter Instance { get; } = new PlayTimeToStringConverter();

        private static string LOCPlayedNoneString;
        private static string LOCPlayedNone;
        private static string LOCPlayedSeconds;
        private static string LOCPlayedMinutes;
        private static string LOCPlayedHours;
        private static string LOCPlayedDays;

        // Cache conversion results to avoid repeated calculations
        private static readonly ConcurrentDictionary<(ulong time, bool useDays), string> conversionCache = new ConcurrentDictionary<(ulong, bool), string>();

        // Limit cache size to prevent memory bloat
        private const int MaxCacheSize = 500;

        private static void CacheStrings()
        {
            if (LOCPlayedNoneString != null)
            {
                return;
            }

            LOCPlayedNoneString = ResourceProvider.GetString("LOCPlayedNoneString");
            LOCPlayedNone = ResourceProvider.GetString("LOCPlayedNone");
            LOCPlayedSeconds = ResourceProvider.GetString("LOCPlayedSeconds");
            LOCPlayedMinutes = ResourceProvider.GetString("LOCPlayedMinutes");
            LOCPlayedHours = ResourceProvider.GetString("LOCPlayedHours");
            LOCPlayedDays = ResourceProvider.GetString("LOCPlayedDays");
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CacheStrings();
            if (value == null)
            {
                return LOCPlayedNone;
            }

            var seconds = (ulong)value;
            if (seconds == 0)
            {
                return LOCPlayedNone;
            }

            var useDays = parameter is bool formatToDays && formatToDays;
            var cacheKey = (seconds, useDays);

            // Check cache first
            if (conversionCache.TryGetValue(cacheKey, out var cachedResult))
            {
                return cachedResult;
            }

            // Clean up cache if it gets too large
            if (conversionCache.Count > MaxCacheSize)
            {
                var keysToRemove = conversionCache.Keys.Take(MaxCacheSize / 4).ToList();
                foreach (var key in keysToRemove)
                {
                    conversionCache.TryRemove(key, out _);
                }
            }

            // Calculate the result
            string result;

            // Can't use TimeSpan from seconds because ulong is too large for it
            if (seconds < 60)
            {
                result = string.Format(LOCPlayedSeconds, seconds);
            }
            else
            {
                var minutes = seconds / 60;
                if (minutes < 60)
                {
                    result = string.Format(LOCPlayedMinutes, minutes);
                }
                else
                {
                    var hours = minutes / 60;
                    if (useDays && hours >= 24)
                    {
                        var days = hours / 24;
                        var remainingHours = hours % 24;
                        var remainingMinutes = minutes % 60;

                        result = string.Format(LOCPlayedDays, days, remainingHours, remainingMinutes);
                    }
                    else
                    {
                        result = string.Format(LOCPlayedHours, hours, minutes - (hours * 60));
                    }
                }
            }

            // Cache the result
            conversionCache.TryAdd(cacheKey, result);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
