using System;
using System.Globalization;

using UnityEngine;

namespace Sackrany.Numerics
{
    public struct BigNumber : IComparable<BigNumber>, IEquatable<BigNumber>
    {
        public static readonly BigNumber Zero = new BigNumber(0);
        public static readonly BigNumber One = new BigNumber(1);
        
        private double mantissa;
        private int exponent;

        public double Mantissa => mantissa;
        public int Exponent => exponent;

        private static double[] _powerOfTenCache = InitCache();
        private const int MinCachedExponent = -30;
        private const int MaxCachedExponent = 30;

        private static double[] InitCache()
        {
            int size = MaxCachedExponent - MinCachedExponent + 1;
            var ret = new double[size];
            for (int exp = MinCachedExponent; exp <= MaxCachedExponent; exp++)
            {
                ret[exp - MinCachedExponent] = Math.Pow(10, exp);
            }
            return ret;
        }

        public static double GetPowerOfTen(int exponent)
        {
            if (_powerOfTenCache == null)
            {
                Debug.Log("nll");
                _powerOfTenCache = InitCache();
            }
            int index = exponent - MinCachedExponent;
            if ((uint)index < (uint)_powerOfTenCache.Length)
                return _powerOfTenCache[index];
            return Math.Pow(10, exponent);
        }

        public BigNumber(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Invalid number value");
            if (value == 0)
            {
                mantissa = 0;
                exponent = 0;
                return;
            }

            exponent = (int)Math.Floor(Math.Log10(Math.Abs(value)));
            mantissa = value / GetPowerOfTen(exponent);
        }        
        public BigNumber Normalize()
        {
            if (mantissa == 0) return Zero;
    
            double m = mantissa;
            int e = exponent;
            
            if (double.IsNaN(m) || double.IsInfinity(m))
                throw new ArithmeticException("Invalid mantissa");
    
            const int maxIterations = 100;
            int iterations = 0;
    
            while (Math.Abs(m) >= 10 && iterations < maxIterations)
            {
                m /= 10;
                e++;
                iterations++;
            }
    
            while (Math.Abs(m) < 1 && iterations < maxIterations)
            {
                m *= 10;
                e--;
                iterations++;
            }
    
            if (iterations >= maxIterations)
            {
                throw new InvalidOperationException("Normalize failed: invalid mantissa");
            }
    
            return new BigNumber { mantissa = m, exponent = e };
        }
        public static bool TryCreate(double value, out BigNumber result)
        {
            try
            {
                result = new BigNumber(value);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        public static BigNumber operator +(BigNumber a, BigNumber b)
        {
            if (a.mantissa == 0) return b;
            if (b.mantissa == 0) return a;

            int diff = a.exponent - b.exponent;

            BigNumber result;
            if (diff >= 0)
            {
                result.exponent = a.exponent;
                result.mantissa = a.mantissa + b.mantissa / GetPowerOfTen(diff);
            }
            else
            {
                result.exponent = b.exponent;
                result.mantissa = a.mantissa / GetPowerOfTen(-diff) + b.mantissa;
            }

            return result.Normalize();
        }
        public static BigNumber operator +(BigNumber a, double b) => a + new BigNumber(b);
        public static BigNumber operator +(double a, BigNumber b) => new BigNumber(a) + b;
        
        public static BigNumber operator -(BigNumber a, BigNumber b) => a + (-b);
        public static BigNumber operator -(BigNumber a, double b) => a + new BigNumber(b * -1);
        public static BigNumber operator -(double a, BigNumber b) => new BigNumber(a) - b;
        
        public static BigNumber operator *(BigNumber a, BigNumber b)
        {
            BigNumber result;
            result.mantissa = a.mantissa * b.mantissa;
            result.exponent = a.exponent + b.exponent;
            return result.Normalize();
        }
        public static BigNumber operator *(BigNumber a, double b) => a * new BigNumber(b);
        public static BigNumber operator *(double a, BigNumber b) => new BigNumber(a) * b;
        
        public static BigNumber operator /(BigNumber a, BigNumber b)
        {
            if (b.mantissa == 0)
                throw new DivideByZeroException("Cannot divide by zero. BigNumber denominator is zero.");
    
            if (a.mantissa == 0)
                return new BigNumber(0);
    
            if (double.IsNaN(a.mantissa) || double.IsNaN(b.mantissa) ||
                double.IsInfinity(a.mantissa) || double.IsInfinity(b.mantissa))
            {
                throw new ArithmeticException("Operation contains invalid values (NaN/Infinity).");
            }

            double newMantissa = a.mantissa / b.mantissa;
            int newExponent = a.exponent - b.exponent;
    
            if (double.IsInfinity(newMantissa) || double.IsNaN(newMantissa))
            {
                throw new ArithmeticException("Division result is too large or undefined.");
            }

            BigNumber result = new BigNumber { mantissa = newMantissa, exponent = newExponent };
            return result.Normalize();
        }
        public static BigNumber operator /(BigNumber a, double b) => a / new BigNumber(b);
        public static BigNumber operator /(double a, BigNumber b) => new BigNumber(a) / b;
        
        public static BigNumber operator -(BigNumber a) => new BigNumber { mantissa = -a.mantissa, exponent = a.exponent };
        public static bool TryDivide(BigNumber a, BigNumber b, out BigNumber result)
        {
            try
            {
                result = a / b;
                return true;
            }
            catch (DivideByZeroException)
            {
                result = default;
                return false;
            }
        }
        
        public int CompareTo(BigNumber other)
        {
            if (mantissa == 0 && other.mantissa == 0) return 0;
    
            bool thisNonNegative = mantissa >= 0;
            bool otherNonNegative = other.mantissa >= 0;
    
            if (thisNonNegative != otherNonNegative)
                return thisNonNegative ? 1 : -1;
    
            if (exponent != other.exponent)
            {
                int expComparison = exponent.CompareTo(other.exponent);
                return thisNonNegative ? expComparison : -expComparison;
            }
    
            return mantissa.CompareTo(other.mantissa);
        }
        
        public bool Equals(BigNumber other) => mantissa.Equals(other.mantissa) && exponent == other.exponent;
        public override bool Equals(object obj) => obj is BigNumber other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(mantissa, exponent);
        
        public override string ToString()
        {
            if (mantissa == 0) return "0";
            return $"{mantissa.ToString("0.##",CultureInfo.InvariantCulture)}e{exponent}";
        }
    }
}