using System.Globalization;

namespace Solis.Data.JSON
{
    /// <summary>
    /// Represents a JSON boolean value.
    /// </summary>
    public class JsonBool : JsonValue
    {
        #region Public Fields
        public bool value;
        #endregion

        #region Abstract Methods Implementation
        public override void Serialize(JsonWriter writer)
        {
            writer.WriteString(value ? "true" : "false");
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
        public static implicit operator JsonBool(bool value) => new () { value = value };
        public static implicit operator bool(JsonBool value) => value.value;

        public static bool operator ==(JsonBool a, JsonBool b)
        {
            return a!.value == b!.value;
        }

        public static bool operator !=(JsonBool a, JsonBool b)
        {
            return a!.value != b!.value;
        }

        public override bool Equals(object obj)
        {
            return obj is JsonBool @bool && value == @bool.value;
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return value.GetHashCode();
        }
        #endregion
    }
}