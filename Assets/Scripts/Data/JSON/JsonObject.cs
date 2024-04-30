using System.Collections;
using System.Collections.Generic;

namespace Solis.Data.JSON
{
    /// <summary>
    /// Represents a JSON object.
    /// </summary>
    public class JsonObject : JsonValue, IEnumerable<KeyValuePair<string, JsonValue>>
    {
        #region Private Fields
        private readonly Dictionary<string, JsonValue> _values = new Dictionary<string, JsonValue>();
        #endregion
        
        #region Public Properties
        /// <summary>
        /// Returns the number of values in the object.
        /// </summary>
        public int Count => _values.Count;

        /// <summary>
        /// Used to access the value at the specified key.
        /// </summary>
        /// <param name="key"></param>
        public JsonValue this[string key]
        {
            get => _values[key];
            set => _values[key] = value;
        }
        #endregion

        #region Abstract Methods Implementation
        public override void Serialize(JsonWriter writer)
        {
            writer.BeginObject();
            foreach (var pair in _values)
            {
                writer.Write(pair.Key, pair.Value);
            }

            writer.EndObject();
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(false);
        }
        
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns></returns>
        public string ToString(bool beautify)
        {
            JsonWriter writer = new(beautify);
            Serialize(writer);
            return writer.GetResult();
        }
        
        /// <summary>
        /// Adds a new key-value pair to the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, JsonValue value)
        {
            _values.Add(key, value);
        }

        /// <summary>
        /// Removes the value with the specified key.
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            _values.Remove(key);
        }

        /// <summary>
        /// Returns whether the object contains the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return _values.ContainsKey(key);
        }

        /// <summary>
        /// Returns the value at the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
        public JsonValue Get(string key, JsonValue fallback = null)
        {
            return _values.GetValueOrDefault(key, fallback);
        }

        /// <summary>
        /// Returns the value at the specified key as the specified type.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fallback"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(string key, T fallback = default)
        {
            return (T)CastInverse(_values.GetValueOrDefault(key, Cast(fallback)), typeof(T));
        }

        /// <summary>
        /// Returns the value at the specified key as a list.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public JsonList GetList(string key)
        {
            return (JsonList)_values.GetValueOrDefault(key, new JsonList());
        }

        /// <summary>
        /// Returns the value at the specified key as an object.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public JsonObject GetObject(string key)
        {
            return (JsonObject)_values.GetValueOrDefault(key, new JsonObject());
        }

        /// <summary>
        /// Clears all values from the object.
        /// </summary>
        public void Clear()
        {
            _values.Clear();
        }
        #endregion

        #region IEnumerable Methods
        /// <summary>
        /// Returns an enumerator that iterates through the list.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    
        /// <summary>
        /// Returns an enumerator that iterates through the list.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, JsonValue>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }
        #endregion
    }
}