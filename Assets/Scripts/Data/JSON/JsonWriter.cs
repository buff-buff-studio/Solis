using System.Collections.Generic;
using System.Text;

namespace Solis.Data.JSON
{
    /// <summary>
    /// JSON writer class. Writes JSON data to a string.
    /// </summary>
    public class JsonWriter
    {
        #region Private Fields
        private readonly bool _pretty;
        private readonly StringBuilder _builder = new();
        private readonly Stack<bool> _emptyStack = new();
        private readonly char _indent;
        #endregion

        #region Public Constructor
        public JsonWriter(bool pretty, char indent = '\t')
        {
            _pretty = pretty;
            _indent = indent;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the JSON string.
        /// </summary>
        /// <returns></returns>
        public string GetResult()
        {
            return _builder.ToString();
        }

        /// <summary>
        /// Begins a JSON object.
        /// </summary>
        public void BeginObject()
        {
            _builder.Append("{");
            _emptyStack.Push(true);
        }

        /// <summary>
        /// Ends a JSON object.
        /// </summary>
        public void EndObject()
        {
            if (!_emptyStack.Pop() && _pretty)
            {
                _builder.Append('\n');
                _builder.Append(_indent, _emptyStack.Count);
            }

            _builder.Append("}");
        }

        /// <summary>
        /// Begins a JSON array.
        /// </summary>
        public void BeginArray()
        {
            _builder.Append("[");
            _emptyStack.Push(true);
        }

        /// <summary>
        /// Ends a JSON array.
        /// </summary>
        public void EndArray()
        {
            if (!_emptyStack.Pop() && _pretty)
            {
                _builder.Append('\n');
                _builder.Append(_indent, _emptyStack.Count);
            }

            _builder.Append("]");
        }

        /// <summary>
        /// Adds a comma separator.
        /// </summary>
        public void Comma()
        {
            _builder.Append(_pretty ? "," : ", ");
        }

        /// <summary>
        /// Writes a JSON value.
        /// </summary>
        /// <param name="value"></param>
        public void Write(JsonValue value)
        {
            if (!_emptyStack.Peek())
                Comma();
            else
            {
                _emptyStack.Pop();
                _emptyStack.Push(false);
            }

            if (_pretty)
            {
                _builder.Append('\n');
                _builder.Append(_indent, _emptyStack.Count);
            }

            if (value == null)
                WriteString("null");
            else
                value.Serialize(this);
        }

        /// <summary>
        /// Writes a JSON key-value pair.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Write(string key, JsonValue value)
        {
            if (!_emptyStack.Peek())
                Comma();
            else
            {
                _emptyStack.Pop();
                _emptyStack.Push(false);
            }

            if (_pretty)
            {
                _builder.Append('\n');
                _builder.Append(_indent, _emptyStack.Count);
            }

            WriteString($"\"{key}\"");
            _builder.Append(": ");

            if (value == null)
                WriteString("null");
            else
                value.Serialize(this);
        }

        /// <summary>
        /// Writes a string.
        /// </summary>
        /// <param name="text"></param>
        public void WriteString(string text)
        {
            _builder.Append(text);
        }
        #endregion
    }
}