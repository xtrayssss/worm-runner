using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace _Project.Scripts.Gameplay
{
    public static class GameFormatter
    {
        public static string FormatPercentageWithColor(string description, float value, string colorHex,
            int decimalPlaces = 0)
        {
            string formatted = FormatPercentage(description, value, decimalPlaces, includePercentSign: false);

            float percentValue = value * 100f;
            string valueString = percentValue.ToString("G", CultureInfo.InvariantCulture);

            int valueIndex = formatted.IndexOf(valueString, StringComparison.Ordinal);
            if (valueIndex >= 0)
            {
                int endIndex = valueIndex + valueString.Length;
                int colorEndIndex = endIndex;

                if (colorEndIndex < formatted.Length && formatted[colorEndIndex] == '%')
                    colorEndIndex++;

                string before = formatted.Substring(0, valueIndex);
                string colored =
                    $"<color={colorHex}>{formatted.Substring(valueIndex, colorEndIndex - valueIndex)}</color>";
                string after = formatted.Substring(colorEndIndex);

                return before + colored + after;
            }

            return formatted;
        }

        public static string FormatPercentage(string description, float value, int decimalPlaces = 0,
            bool includePercentSign = false, bool formatAsPositive = false)
        {
            string formattedValue = FormatSingleValue(value, true, decimalPlaces, formatAsPositive, includePercentSign);
            return string.Format(description, formattedValue);
        }

        public static string FormatValue(string description, float value, bool formatAsPositive = false)
        {
            int decimalPlaces = GetDecimalPlacesFromFormat(description);
            string formattedValue = FormatSingleValue(value, false, decimalPlaces, formatAsPositive, false);
            return string.Format(description, formattedValue);
        }

        public static string FormatValueWithColor(string description, float value, string colorHex,
            bool formatAsPositive = false)
        {
            int decimalPlaces = GetDecimalPlacesFromFormat(description);
            string formattedValue = FormatSingleValue(value, false, decimalPlaces, formatAsPositive, false);
            string colored = $"<color={colorHex}>{formattedValue}</color>";
            return string.Format(description, colored);
        }

        public static string FormatIdentifier(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : string.Join(" ", value.Split('_')
                    .Where(static word => !string.IsNullOrEmpty(word))
                    .Select(static word =>
                        word.Length == 0 ? string.Empty : char.ToUpper(word[0]) + word[1..].ToLower()));
        }

        public static string FormatTwoValues(string description, float value1, float value2,
            bool isValue1Percentage = false, bool isValue2Percentage = false,
            int decimalPlaces1 = 0, int decimalPlaces2 = 0,
            bool formatValue1AsPositive = false, bool formatValue2AsPositive = false,
            bool includePercentSign1 = false, bool includePercentSign2 = false)
        {
            string formattedValue1 =
                FormatSingleValue(value1, isValue1Percentage, decimalPlaces1, formatValue1AsPositive,
                    includePercentSign1);
            string formattedValue2 =
                FormatSingleValue(value2, isValue2Percentage, decimalPlaces2, formatValue2AsPositive,
                    includePercentSign2);

            return string.Format(description, formattedValue1, formattedValue2);
        }

        public static string FormatTwoPercentages(string description, float value1, float value2,
            int decimalPlaces1 = 0, int decimalPlaces2 = 0,
            bool includePercentSign = false)
        {
            return FormatTwoValues(description, value1, value2,
                true, true, decimalPlaces1, decimalPlaces2, false, false, includePercentSign, includePercentSign);
        }

        public static string FormatPercentageAndValue(string description, float percentValue, float regularValue,
            int percentDecimalPlaces = 0, bool formatRegularAsPositive = false, bool includePercentSign = false)
        {
            return FormatTwoValues(description, percentValue, regularValue,
                true, false, percentDecimalPlaces, 0, false, formatRegularAsPositive, includePercentSign, false);
        }

        public static string FormatValueAndPercentage(string description, float regularValue, float percentValue,
            bool formatRegularAsPositive = false, int percentDecimalPlaces = 0, bool includePercentSign = false)
        {
            return FormatTwoValues(description, regularValue, percentValue,
                false, true, 0, percentDecimalPlaces, formatRegularAsPositive, false, includePercentSign);
        }

        public static string FormatTwoRegularValues(string description, float value1, float value2,
            bool formatValue1AsPositive = false, bool formatValue2AsPositive = false)
        {
            return FormatTwoValues(description, value1, value2,
                false, false, 0, 0, formatValue1AsPositive, formatValue2AsPositive);
        }

        private static string FormatSingleValue(float value, bool isPercentage, int decimalPlaces,
            bool formatAsPositive, bool includePercentSign)
        {
            float valueToFormat = formatAsPositive ? Math.Abs(value) : value;

            string result;
            if (isPercentage)
            {
                float percentValue = valueToFormat * 100f;
                result = percentValue.ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture);
                if (includePercentSign)
                {
                    result += "%";
                }
            }
            else
            {
                result = valueToFormat.ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture);
            }

            return result;
        }

        private static int GetDecimalPlacesFromFormat(string format)
        {
            if (string.IsNullOrEmpty(format))
                return 0;

            Match match = Regex.Match(format, @"\{0:F(\d+)\}");
            if (match.Success && match.Groups.Count > 1 && int.TryParse(match.Groups[1].Value, out int places))
                return places;

            return 0;
        }

        public static string StripTags(string description)
        {
            if (string.IsNullOrEmpty(description))
                return description;

            return Regex.Replace(
                description,
                "<[^>]*>",
                string.Empty
            );
        }
    }
}