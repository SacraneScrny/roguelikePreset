using System;
using System.Globalization;
using System.Text;

using UnityEngine;

namespace Sackrany.Numerics
{
    public static class BigNumberExtensions
    {        
        private static readonly double[] _suffixRemainderCache = { 0.01, 0.1, 1, 10, 100 };
        
        private static readonly string[] _largeSuffixes = {
            "", "K", "M", "B", "T", 
            "Qa", "Qi", "Sx", "Sp", "Oc", "No", 
            "Dc", "Udc", "Ddc", "Tdc", "Qadc", "Qidc", "Sxdc", "Spdc", "Ocdc", "Nodc"
        };
        private static readonly char[] _largeSuffixes2 = {
            'K', 'M', 'B', 'T'
        };
        
        private static readonly string[] _smallSuffixes = {
            "", "m", "μ", "n", "p", "f", "a", "z", "y"
        };
        
        public static string ToShortStringTMP(
            this BigNumber number,
            int maxPrecision = 2,
            int minSuffixPercent = 70
        )
        {
            if (number.Mantissa == 0)
                return "0";

            double mantissa = number.Mantissa;
            int exponent = number.Exponent;

            bool negative = mantissa < 0;
            mantissa = Math.Abs(mantissa);

            if (exponent < 3)
            {
                double value = mantissa * Math.Pow(10, exponent);
                return (negative ? "-" : "") +
                       value.ToString($"0.{new string('#', maxPrecision)}", CultureInfo.InvariantCulture);
            }

            int tier = exponent / 3 - 1;
            int rem = exponent % 3;

            mantissa *= Math.Pow(10, rem);

            char baseChar = _largeSuffixes2[tier % 4];
            int repeatCount = tier / 4 + 1;

            if (mantissa >= 1000)
            {
                mantissa /= 1000;
                repeatCount++;
            }

            string numberPart = mantissa.ToString(
                $"0.{new string('#', maxPrecision)}",
                CultureInfo.InvariantCulture
            );

            string suffix = BuildTmpSuffix(baseChar, repeatCount, minSuffixPercent);

            return (negative ? "-" : "") + numberPart + suffix;
        }
        private static string BuildTmpSuffix(
            char letter,
            int count,
            int minPercent)
        {
            if (count <= 1)
                return letter.ToString();

            var sb = new StringBuilder(count * 12);

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / (count - 1);
                int size = Mathf.RoundToInt(
                    Mathf.Lerp(100, minPercent, t)
                );

                sb.Append("<size=");
                sb.Append(size);
                sb.Append("%>");
                sb.Append(letter);
                sb.Append("</size>");
            }

            return sb.ToString();
        }

        
        public static string ToShortString2(
            this BigNumber number,
            int maxPrecision = 2)
        {
            if (number.Mantissa == 0)
                return "0";

            double mantissa = number.Mantissa;
            int exponent = number.Exponent;

            bool negative = mantissa < 0;
            mantissa = Math.Abs(mantissa);

            if (exponent < 3)
            {
                double value = mantissa * Math.Pow(10, exponent);
                return (negative ? "-" : "") + value.ToString($"0.{new string('#', maxPrecision)}", CultureInfo.InvariantCulture);
            }

            int tier = exponent / 3 - 1;
            int rem = exponent % 3;

            mantissa *= Math.Pow(10, rem);

            char baseChar = _largeSuffixes2[(tier % 4)];

            int repeatCount = tier / 4 + 1;

            string suffix = new string(baseChar, repeatCount);

            if (mantissa >= 1000)
            {
                mantissa /= 1000;
                repeatCount++;
                suffix = new string(baseChar, repeatCount);
            }

            string format = $"0.{new string('#', maxPrecision)}";
            string num = mantissa.ToString(format, CultureInfo.InvariantCulture);

            return (negative ? "-" : "") + num + suffix;
        }
        public static string ToShortString(
            this BigNumber number, 
            CultureInfo culture = null, 
            int maxPrecision = 0)
        {
            culture ??= CultureInfo.InvariantCulture;
            
            if (number.Mantissa == 0) 
                return "0";
            
            bool isNegative = number.Mantissa < 0;
            double absMantissa = Math.Abs(number.Mantissa);
            int exp = number.Exponent;
            
            if (exp > -3 && exp < 3) 
                return FormatStandardNumber(isNegative, absMantissa * BigNumber.GetPowerOfTen(exp), culture, maxPrecision);
            
            if (exp >= 3)
            {
                int idx = exp / 3;
                int remainder = exp % 3;
                
                if (idx < _largeSuffixes.Length)
                {
                    double value = absMantissa * GetSuffixRemainderPower(remainder);
                    return FormatWithSuffix(isNegative, value, _largeSuffixes[idx], culture, maxPrecision);
                }
                return FormatScientific(isNegative, absMantissa, exp, culture, maxPrecision);
            }
            
            int absExp = -exp;
            int smallIdx = absExp / 3;
            int smallRemainder = absExp % 3;
            
            if (smallIdx < _smallSuffixes.Length && smallRemainder == 0)
            {
                return FormatWithSuffix(isNegative, absMantissa, _smallSuffixes[smallIdx], culture, maxPrecision);
            }
            
            if (smallIdx + 1 < _smallSuffixes.Length)
            {
                double value = absMantissa * GetSuffixRemainderPower(3 - smallRemainder);
                return FormatWithSuffix(isNegative, value, _smallSuffixes[smallIdx + 1], culture, maxPrecision);
            }
            
            return FormatScientific(isNegative, absMantissa, exp, culture, maxPrecision);
        }
        
        private const int MaxResultLength = 10000;
        private const int MaxExponentForFull = 1000;
        public static string ToStringFull(this BigNumber bn)
        {
            if (bn.Mantissa == 0)
                return "0";
            
            BigNumber normalized = bn.Normalize();
            double absMantissa = Math.Abs(normalized.Mantissa);
            int exponent = normalized.Exponent;
            
            if (Math.Abs(exponent) > MaxExponentForFull)
            {
                return $"Value too large for full representation (exp={exponent}). Use standard ToString().";
            }

            string mantissaStr = absMantissa.ToString("G17", CultureInfo.InvariantCulture);
            
            int dotIndex = mantissaStr.IndexOf('.');
            string integerPart = dotIndex < 0 ? mantissaStr : mantissaStr.Substring(0, dotIndex);
            string fractionalPart = dotIndex < 0 ? "" : mantissaStr.Substring(dotIndex + 1);
            
            string digits = integerPart + fractionalPart;
            int totalDigits = digits.Length;
            int originalPointPos = integerPart.Length;
            int newPointPos = originalPointPos + exponent;
            
            StringBuilder resultBuilder = new StringBuilder();
            bool isNegative = bn.Mantissa < 0;
            
            if (newPointPos <= 0)
            {
                int leadingZeros = -newPointPos;
                int resultLength = 2 + leadingZeros + totalDigits; // "0." + нули + цифры
                
                if (resultLength > MaxResultLength)
                    return TruncateResult(isNegative, leadingZeros, totalDigits);
                
                if (isNegative) resultBuilder.Append('-');
                resultBuilder.Append('0');
                resultBuilder.Append('.');
                resultBuilder.Append('0', leadingZeros);
                resultBuilder.Append(digits);
            }
            else
            {
                int trailingZeros = newPointPos - totalDigits;
                int integerLength = newPointPos > totalDigits ? newPointPos : newPointPos;
                int fractionalLength = newPointPos < totalDigits ? totalDigits - newPointPos : 0;
                int resultLength = (isNegative ? 1 : 0) + integerLength + (fractionalLength > 0 ? 1 + fractionalLength : 0);
                
                if (resultLength > MaxResultLength)
                    return TruncateResult(isNegative, integerLength, fractionalLength);
                
                if (isNegative) resultBuilder.Append('-');
                
                if (newPointPos <= totalDigits)
                {
                    resultBuilder.Append(digits, 0, newPointPos);
                }
                else
                {
                    resultBuilder.Append(digits);
                    resultBuilder.Append('0', trailingZeros);
                }
                
                if (newPointPos < totalDigits)
                {
                    string fractional = digits.Substring(newPointPos);
                    fractional = fractional.TrimEnd('0');
                    if (fractional.Length > 0)
                    {
                        resultBuilder.Append('.');
                        resultBuilder.Append(fractional);
                    }
                }
            }
            
            return resultBuilder.ToString();
        }
        private static string TruncateResult(bool isNegative, int integerLength, int fractionalLength)
        {
            string msg = $"Result too long ({integerLength + fractionalLength} digits). Use standard ToString().";
            return isNegative ? $"-{msg}" : msg;
        }
        
        public static double GetSuffixRemainderPower(int remainder)
        {
            int index = remainder + 2;
            if ((uint)index < (uint)_suffixRemainderCache.Length)
            {
                return _suffixRemainderCache[index];
            }
            return Math.Pow(10, remainder);
        }

        
        private static string FormatStandardNumber(
            bool isNegative, 
            double value, 
            CultureInfo culture, 
            int maxPrecision)
        {
            if (Math.Abs(value) < 0.001 || Math.Abs(value) >= 1000)
                return FormatScientific(isNegative, value, 0, culture, maxPrecision);
            
            return CleanupFormattedNumber(
                isNegative, 
                value.ToString($"0.{new string('#', maxPrecision)}", culture)
            );
        }

        private static string FormatWithSuffix(
            bool isNegative, 
            double value, 
            string suffix, 
            CultureInfo culture, 
            int maxPrecision)
        {
            if (value >= 1000 && !string.IsNullOrEmpty(suffix))
            {
                bool wasNegative = value < 0;
                value = Math.Abs(value) / 1000 * (wasNegative ? -1 : 1);
                suffix = GetNextLargeSuffix(suffix) ?? suffix;
            }
            else if (value < 1 && !string.IsNullOrEmpty(suffix) && !suffix.Equals(_smallSuffixes[0]))
            {
                bool wasNegative = value < 0;
                value = Math.Abs(value) * 1000 * (wasNegative ? -1 : 1);
                suffix = GetPreviousSmallSuffix(suffix) ?? suffix;
            }
            
            string format = $"0.{new string('#', maxPrecision)}";
            string formatted = value.ToString(format, culture);
            return CleanupFormattedNumber(isNegative, formatted) + suffix;
        }

        private static string FormatScientific(
            bool isNegative, 
            double mantissa, 
            int exponent, 
            CultureInfo culture, 
            int maxPrecision)
        {
            string format = $"0.{new string('#', maxPrecision)}";
            string mantissaStr = mantissa.ToString(format, culture);
            return (isNegative ? "-" : "") + $"{mantissaStr}e{exponent}";
        }

        private static string CleanupFormattedNumber(bool isNegative, string formatted)
        {
            if (formatted.Contains('.'))
            {
                formatted = formatted.TrimEnd('0').TrimEnd('.');
            }
            return (isNegative ? "-" : "") + formatted;
        }

        private static string GetNextLargeSuffix(string currentSuffix)
        {
            int idx = Array.IndexOf(_largeSuffixes, currentSuffix);
            return (idx >= 0 && idx + 1 < _largeSuffixes.Length) 
                ? _largeSuffixes[idx + 1] 
                : null;
        }

        private static string GetPreviousSmallSuffix(string currentSuffix)
        {
            int idx = Array.IndexOf(_smallSuffixes, currentSuffix);
            return (idx > 0) ? _smallSuffixes[idx - 1] : null;
        }
    }
}