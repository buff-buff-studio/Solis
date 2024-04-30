using System.Collections;
using System.Collections.Generic;

namespace Solis.Data.JSON
{
    /// <summary>
    /// Represents a JSON list of values.
    /// </summary>
    public class JsonList : JsonValue, IEnumerable<JsonValue>
    {
        #region Private Fields
        private readonly List<JsonValue> _values = new();
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the number of values in the list.
        /// </summary>
        public int Count => _values.Count;

        /// <summary>
        /// Used to access the value at the specified index.
        /// </summary>
        /// <param name="index"></param>
        public JsonValue this[int index]
        {
            get => _values[index];
            set => _values[index] = value;
        }
        #endregion

        #region Abstract Methods Implementation
        public override void Serialize(JsonWriter writer)
        {
            writer.BeginArray();
            foreach (var t in _values)
            {
                writer.Write(t);
            }

            writer.EndArray();
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
        /// <param name="beautify"></param>
        /// <returns></returns>
        public string ToString(bool beautify)
        {
            JsonWriter writer = new(beautify);
            Serialize(writer);
            return writer.GetResult();
        }

        /// <summary>
        /// Adds a value to the list.
        /// </summary>
        /// <param name="value"></param>
        public void Add(JsonValue value)
        {
            _values.Add(value);
        }

        /// <summary>
        /// Removes the value at the specified index.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            _values.RemoveAt(index);
        }

        /// <summary>
        /// Returns the value at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(int index)
        {
            return (T)(object)_values[index];
        }

        /// <summary>
        /// Gets the value at the specified index as a list.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public JsonList GetList(int index)
        {
            return (JsonList)_values[index];
        }

        /// <summary>
        /// Returns the value at the specified index as an object.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public JsonObject GetObject(int index)
        {
            return (JsonObject)_values[index];
        }

        /// <summary>
        /// Clears the list.
        /// </summary>
        public void Clear()
        {
            _values.Clear();
        }

        /// <summary>
        /// Adds a range of values to the list.
        /// </summary>
        /// <param name="collection"></param>
        public void AddRange(IEnumerable<JsonValue> collection)
        {
            _values.AddRange(collection);
        }

        /// <summary>
        /// Inserts a value at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void Insert(int index, JsonValue value)
        {
            _values.Insert(index, value);
        }

        /// <summary>
        /// Removes a value from the list.
        /// </summary>
        /// <param name="value"></param>
        public void Remove(JsonValue value)
        {
            _values.Remove(value);
        }

        /// <summary>
        /// Returns whether the list contains the specified value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(JsonValue value)
        {
            return _values.Contains(value);
        }

        /// <summary>
        /// Returns the index of the specified value. If the value is not found, -1 is returned.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int IndexOf(JsonValue value)
        {
            return _values.IndexOf(value);
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
        public IEnumerator<JsonValue> GetEnumerator()
        {
            return _values.GetEnumerator();
        }
        #endregion
    }
}