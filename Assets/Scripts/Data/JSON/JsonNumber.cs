using System;
using System.Globalization;

namespace Solis.Data.JSON
{
    /// <summary>
    /// Represents a JSON number value.
    /// </summary>
    public class JsonNumber : JsonValue
    {
        #region Public Fields
        public double value;
        #endregion
        
        #region Abstract Methods Implementation
        public override void Serialize(JsonWriter writer)
        {
            writer.WriteString(value.ToString(CultureInfo.InvariantCulture));
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the string representation of the number.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        #endregion

        #region Public Operator Overloads
        public static implicit operator JsonNumber(double value) => new () { value = value };
        public static implicit operator double(JsonNumber value) => value.value;

        public static implicit operator JsonNumber(float value) => new () { value = value };
        public static implicit operator float(JsonNumber value) => (float)value.value;

        public static implicit operator JsonNumber(int value) => new () { value = value };
        public static implicit operator int(JsonNumber value) => (int)value.value;

        public static implicit operator JsonNumber(byte value) => new () { value = value };
        public static implicit operator byte(JsonNumber value) => (byte)value.value;

        public static bool operator ==(JsonNumber a, JsonNumber b)
        {
            return Math.Abs(a!.value - b!.value) < 0.000001;
        }

        public static bool operator !=(JsonNumber a, JsonNumber b)
        {
            return Math.Abs(a!.value - b!.value) > 0.000001;
        }

        public override bool Equals(object obj)
        {
            return obj is JsonNumber number && value.Equals(number.value);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return value.GetHashCode();
        }
        #endregion
    }
}