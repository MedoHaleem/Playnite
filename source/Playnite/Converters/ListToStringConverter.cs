using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.Converters
{
    public class ListSizeToBoolConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is IList<dynamic> list)
            {
                return list.Count > 0;
            }
            else
            {
                return false;
            }
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

    public class ListSizeToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is System.Collections.IList list)
            {
                return list.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return Visibility.Collapsed;
            }
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

    public class NiceListToStringConverter : MarkupExtension, IValueConverter
    {
        // Cache conversion results to avoid repeated string operations
        private static readonly ConcurrentDictionary<int, string> conversionCache = new ConcurrentDictionary<int, string>();

        // Limit cache size to prevent memory bloat
        private const int MaxCacheSize = 1000;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is IEnumerable<object> enumerable)
            {
                // Create a hash from the list content for caching
                var hash = GetListHash(enumerable);

                // Check cache first
                if (conversionCache.TryGetValue(hash, out var cachedResult))
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

                // Calculate and cache the result
                var result = string.Join(", ", enumerable);
                conversionCache.TryAdd(hash, result);
                return result;
            }
            else
            {
                return value.ToString();
            }
        }

        private static int GetListHash(IEnumerable<object> enumerable)
        {
            // Compatible hash code calculation for .NET Framework 4.6.2
            unchecked
            {
                int hash = 17;
                foreach (var item in enumerable)
                {
                    hash = hash * 23 + (item?.ToString() ?? string.Empty).GetHashCode();
                }
                return hash;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string stringVal && !stringVal.IsNullOrEmpty())
            {
                var converted = stringVal.Split(new char[] { ',' }).Select(a => a.Trim());
                if (targetType == typeof(ComparableList<string>))
                {
                    return new ComparableList<string>(converted);
                }
                else
                {
                    return converted.ToList();
                }
            }

            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    public class ListToStringConverter : MarkupExtension, IValueConverter
    {
        private const string defaultSeperator = ",";

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var sep = defaultSeperator;
            if (parameter is string customSep)
            {
                sep = customSep;
            }

            if (value is IEnumerable<dynamic>)
            {
                return string.Join(sep, (IEnumerable<object>)value);
            }
            else
            {
                return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string stringVal && !stringVal.IsNullOrEmpty())
            {
                var sep = defaultSeperator;
                if (parameter is string customSep)
                {
                    sep = customSep;
                }

                var converted = stringVal.Split(new [] { sep }, StringSplitOptions.None);
                if (targetType == typeof(ComparableList<string>))
                {
                    return new ComparableList<string>(converted);
                }
                if (targetType == typeof(ObservableCollection<string>))
                {
                    return new ObservableCollection<string>(converted);
                }
                else
                {
                    return converted.ToList();
                }
            }

            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    public class ListToMultilineStringConverter : MarkupExtension, IValueConverter
    {
        private readonly string[] splitter = new string[] { "\n" };

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is IEnumerable<dynamic>)
            {
                return string.Join("\n", (IEnumerable<object>)value);
            }
            else
            {
                return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string stringVal && !stringVal.IsNullOrEmpty())
            {
                var converted = stringVal.Split(splitter, StringSplitOptions.None).Select(a => a.Trim('\r')).ToArray();
                if (targetType == typeof(ComparableList<string>))
                {
                    return new ComparableList<string>(converted);
                }
                if (targetType == typeof(ObservableCollection<string>))
                {
                    return new ObservableCollection<string>(converted);
                }
                else
                {
                    return converted.ToList();
                }
            }

            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
