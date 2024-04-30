namespace Solis.Data.JSON
{
    /// <summary>
    /// Represents a JSON string value.
    /// </summary>
    public class JsonString : JsonValue
    {
        #region Public Fields
        public string value;
        #endregion
        
        #region Abstract Methods Implementation
        public override void Serialize(JsonWriter writer)
        {
            writer.WriteString($"\"{value}\"");
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the string representation of the number.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return value;
        }
        #endregion

        #region Public Operator Overloads
        public static implicit operator JsonString(string value) => new JsonString() { value = value };
        public static implicit operator string(JsonString value) => value.value;

        public static bool operator ==(JsonString a, JsonString b)
        {
            return a!.value == b!.value;
        }

        public static bool operator !=(JsonString a, JsonString b)
        {
            return a!.value != b!.value;
        }

        public override bool Equals(object obj)
        {
            return obj is JsonString @string && value == @string.value;
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return value.GetHashCode();
        }
        #endregion
    }
}