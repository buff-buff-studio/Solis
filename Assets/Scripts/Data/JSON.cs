using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace SolarBuff.Data
{
    public class JsonWriter
    {
        private readonly bool _pretty;
        private readonly StringBuilder _builder = new();
        private readonly Stack<bool> _emptyStack = new();
        private readonly char _indent;
        
        public JsonWriter(bool pretty, char indent = '\t')
        {
            _pretty = pretty;
            _indent = indent;
        }
        
        public string GetResult()
        {
            return _builder.ToString();
        }

        public void BeginObject()
        {
            _builder.Append("{");
            _emptyStack.Push(true);
        }

        public void EndObject()
        {
            if(!_emptyStack.Pop() && _pretty)
            {
                _builder.Append('\n');
                _builder.Append(_indent, _emptyStack.Count);
            }
            _builder.Append("}");
        }

        public void BeginArray()
        {
            _builder.Append("[");
            _emptyStack.Push(true);
        }

        public void EndArray()
        {
            if(!_emptyStack.Pop() && _pretty)
            {
                _builder.Append('\n');
                _builder.Append(_indent, _emptyStack.Count);
            }
            _builder.Append("]");
        }

        public void Comma()
        {
            _builder.Append(_pretty ? "," : ", ");
        }
        
        public void Write(JsonValue value)
        {
            if(!_emptyStack.Peek())
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
            if(value == null)
                WriteString("null");
            else
                value.Serialize(this);
        }
        
        public void Write(string key, JsonValue value)
        {
            if(!_emptyStack.Peek())
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
            
            if(value == null)
                WriteString("null");
            else
                value.Serialize(this);
        }

        public void WriteString(string text)
        {
            _builder.Append(text);
        }
    }
    
    public abstract class JsonValue
    {
        public abstract void Serialize(JsonWriter writer);
        
        public static implicit operator JsonValue(bool value) => new JsonBool() {value = value};
        public static implicit operator bool(JsonValue value) => (value as JsonBool)!.value;
        
        public static implicit operator JsonValue(double value) => new JsonNumber() {value = value};
        public static implicit operator double(JsonValue value) => (value as JsonNumber)!.value;
        
        public static implicit operator JsonValue(float value) => new JsonNumber() {value = value};
        public static implicit operator float(JsonValue value) => (float)(value as JsonNumber)!.value;
        
        public static implicit operator JsonValue(int value) => new JsonNumber() {value = value};
        public static implicit operator int(JsonValue value) => (int)(value as JsonNumber)!.value;
        
        public static implicit operator JsonValue(byte value) => new JsonNumber() {value = value};
        public static implicit operator byte(JsonValue value) => (byte)(value as JsonNumber)!.value;
        
        public static implicit operator JsonValue(string value) => new JsonString() {value = value};
        public static implicit operator string(JsonValue value) => (value as JsonString)!.value;
        
        public static implicit operator JsonValue(Color value) => new JsonString() {value = $"#{ColorUtility.ToHtmlStringRGBA(value)}"};
        public static implicit operator Color(JsonValue value) => ColorUtility.TryParseHtmlString((value as JsonString)!.value, out var color) ? color : Color.white;
        
        public static implicit operator JsonValue(Vector2 value) => new JsonList() {new JsonNumber() {value = value.x}, new JsonNumber() {value = value.y}};
        public static implicit operator Vector2(JsonValue value) => new Vector2((float)(value as JsonList)![0], (float)(value as JsonList)![1]);
        
        public static implicit operator JsonValue(Vector3 value) => new JsonList() {new JsonNumber() {value = value.x}, new JsonNumber() {value = value.y}, new JsonNumber() {value = value.z}};
        public static implicit operator Vector3(JsonValue value) => new Vector3((float)(value as JsonList)![0], (float)(value as JsonList)![1], (float)(value as JsonList)![2]);
        
        public static implicit operator JsonValue(Vector4 value) => new JsonList() {new JsonNumber() {value = value.x}, new JsonNumber() {value = value.y}, new JsonNumber() {value = value.z}, new JsonNumber() {value = value.w}};
        public static implicit operator Vector4(JsonValue value) => new Vector4((float)(value as JsonList)![0], (float)(value as JsonList)![1], (float)(value as JsonList)![2], (float)(value as JsonList)![3]);
        
        public static implicit operator JsonValue(Quaternion value) => new JsonList() {new JsonNumber() {value = value.x}, new JsonNumber() {value = value.y}, new JsonNumber() {value = value.z}, new JsonNumber() {value = value.w}};
        public static implicit operator Quaternion(JsonValue value) => new Quaternion((float)(value as JsonList)![0], (float)(value as JsonList)![1], (float)(value as JsonList)![2], (float)(value as JsonList)![3]);
        
        
        public static JsonValue Parse(string json)
        {
            for (var i = 0; i < json.Length; i++)
            {
                if (char.IsWhiteSpace(json[i]))
                    continue;
                if (json[i] == '{')
                    return ParseObject(json, ref i);
                if (json[i] == '[')
                    return ParseList(json, ref i);
                if (json[i] == '"')
                    return ParseString(json, ref i);
                if (char.IsDigit(json[i]) || json[i] == '-')
                    return ParseNumber(json, ref i);
                if (json[i] == 't' || json[i] == 'f')
                    return ParseBool(json, ref i);
                if (json[i] == 'n')
                    return ParseNull(json, ref i);
                throw new Exception($"Invalid JSON at {i}");
            }
            
            throw new Exception($"Invalid JSON at 0");
        }
        
        private static JsonObject ParseObject(string json, ref int i)
        {
            var obj = new JsonObject();
            i++;
            while (i < json.Length)
            {
                if (char.IsWhiteSpace(json[i]))
                {
                    i++;
                    continue;
                }
                if (json[i] == '}')
                    return obj;
                var key = ParseString(json, ref i);
                
                SkipWhiteSpaces(json, ref i);
                
                if (json[i] != ':')
                    throw new Exception($"Invalid JSON at {i}");
                i++;
                
                SkipWhiteSpaces(json, ref i);
                
                obj.Add(key, Parse(json, ref i));
                
                SkipWhiteSpaces(json, ref i);
                
                if (json[i] == ',')
                {
                    i++;
                    continue;
                }

                if (json[i] == '}')
                {
                    i++;
                    return obj;
                }

                throw new Exception($"Invalid JSON at {i}");
            }
            return null;
        }
        
        private static void SkipWhiteSpaces(string json, ref int i)
        {
            while (char.IsWhiteSpace(json[i]))
            {
                i++;
                if (i >= json.Length)
                    throw new Exception($"Invalid JSON at {i}");
            }
        }
        
        private static JsonList ParseList(string json, ref int i)
        {
            var list = new JsonList();
            i++;
            while (i < json.Length)
            {
                if (char.IsWhiteSpace(json[i]))
                {
                    i++;
                    continue;
                }
                if (json[i] == ']')
                    return list;
                list.Add(Parse(json, ref i));
                
                SkipWhiteSpaces(json, ref i);
                
                if (json[i] == ',')
                {
                    i++;
                    continue;
                }

                if (json[i] == ']')
                {
                    i++;
                    return list;
                }
                throw new Exception($"Invalid JSON at {i}");
            }
            
            return null;
        }
        
        private static JsonString ParseString(string json, ref int i)
        {
            var sb = new StringBuilder();
            i++;
            while (i < json.Length)
            {
                if (json[i] == '"')
                {
                    i++;
                    return sb.ToString();
                }
                if (json[i] == '\\')
                {
                    i++;
                    if (i >= json.Length)
                        throw new Exception($"Invalid JSON at {i}");
                    if (json[i] == 'u')
                    {
                        i++;
                        if (i + 4 >= json.Length)
                            throw new Exception($"Invalid JSON at {i}");
                        var hex = json.Substring(i, 4);
                        if (!int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var code))
                            throw new Exception($"Invalid JSON at {i}");
                        sb.Append((char) code);
                        i += 4;
                    }
                    else
                    {
                        sb.Append(json[i]);
                        i++;
                    }
                }
                else
                {
                    sb.Append(json[i]);
                    i++;
                }
            }
            throw new Exception($"Invalid JSON at {i}");
        }
        
        private static JsonNumber ParseNumber(string json, ref int i)
        {
            var sb = new StringBuilder();
            while (i < json.Length && (char.IsDigit(json[i]) || json[i] == '.' || json[i] == '-'))
            {
                sb.Append(json[i]);
                i++;
            }
            if (!double.TryParse(sb.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                throw new Exception($"Invalid JSON at {i}");
            return value;
        }
        
        private static JsonBool ParseBool(string json, ref int i)
        {
            if (json[i] == 't')
            {
                if (i + 4 >= json.Length || json.Substring(i, 4) != "true")
                    throw new Exception($"Invalid JSON at {i}");
                i += 4;
                return true;
            }
            if (json[i] == 'f')
            {
                if (i + 5 >= json.Length || json.Substring(i, 5) != "false")
                    throw new Exception($"Invalid JSON at {i}");
                i += 5;
                return false;
            }
            throw new Exception($"Invalid JSON at {i}");
        }
        
        private static JsonValue ParseNull(string json, ref int i)
        {
            if (i + 4 >= json.Length || json.Substring(i, 4) != "null")
                throw new Exception($"Invalid JSON at {i}");
            i += 4;
            return null;
        }
        
        private static JsonValue Parse(string json, ref int i)
        {
            for (; i < json.Length; i++)
            {
                if (char.IsWhiteSpace(json[i]))
                    continue;
                if (json[i] == '{')
                    return ParseObject(json, ref i);
                if (json[i] == '[')
                    return ParseList(json, ref i);
                if (json[i] == '"')
                    return ParseString(json, ref i);
                if (char.IsDigit(json[i]) || json[i] == '-')
                    return ParseNumber(json, ref i);
                if (json[i] == 't' || json[i] == 'f')
                    return ParseBool(json, ref i);
                if (json[i] == 'n')
                    return ParseNull(json, ref i);
                throw new Exception($"Invalid JSON at {i}");
            }
            throw new Exception($"Invalid JSON at {i}");
        }
    }
    
    [Serializable]
    public class JsonNumber : JsonValue
    {
        public double value;
        
        public override void Serialize(JsonWriter writer)
        {
            writer.WriteString(value.ToString(CultureInfo.InvariantCulture));
        }

        public override string ToString()
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        
        public static implicit operator JsonNumber(double value) => new JsonNumber() {value = value};
        public static implicit operator double(JsonNumber value) => value.value;
        
        public static implicit operator JsonNumber(float value) => new JsonNumber() {value = value};
        public static implicit operator float(JsonNumber value) => (float)value.value;
        
        public static implicit operator JsonNumber(int value) => new JsonNumber() {value = value};
        public static implicit operator int(JsonNumber value) => (int)value.value;
        
        public static implicit operator JsonNumber(byte value) => new JsonNumber() {value = value};
        public static implicit operator byte(JsonNumber value) => (byte)value.value;
        
        public static bool operator ==(JsonNumber a, JsonNumber b)
        {
            return a.value == b.value;
        }
        
        public static bool operator !=(JsonNumber a, JsonNumber b)
        {
            return a.value != b.value;
        }
        
        public override bool Equals(object obj)
        {
            return obj is JsonNumber number && value.Equals(number.value);
        }
        
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
    
    [Serializable]
    public class JsonBool : JsonValue
    {
        public bool value;
        
        public override void Serialize(JsonWriter writer)
        {
            writer.WriteString(value ? "true" : "false");
        }
        
        public static implicit operator JsonBool(bool value) => new JsonBool() {value = value};
        public static implicit operator bool(JsonBool value) => value.value;
        
        public static bool operator ==(JsonBool a, JsonBool b)
        {
            return a.value == b.value;
        }
        
        public static bool operator !=(JsonBool a, JsonBool b)
        {
            return a.value != b.value;
        }
        
        public override bool Equals(object obj)
        {
            return obj is JsonBool @bool && value == @bool.value;
        }
        
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
    
    [Serializable]
    public class JsonString : JsonValue
    {
        public string value;
        
        public override void Serialize(JsonWriter writer)
        {
            writer.WriteString($"\"{value}\"");
        }
        
        public override string ToString()
        {
            return value;
        }
        
        public static implicit operator JsonString(string value) => new JsonString() {value = value};
        public static implicit operator string(JsonString value) => value.value;
        
        public static bool operator ==(JsonString a, JsonString b)
        {
            return a.value == b.value;
        }
        
        public static bool operator !=(JsonString a, JsonString b)
        {
            return a.value != b.value;
        }
        
        public override bool Equals(object obj)
        {
            return obj is JsonString @string && value == @string.value;
        }
        
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }

    [Serializable]
    public class JsonObject : JsonValue, IEnumerable<KeyValuePair<string, JsonValue>>
    {
        [SerializeReference]
        private SerializedDictionary<string, JsonValue> values = new();
        
        public int Count => values.Count;
        
        public JsonValue this[string key]
        {
            get => values[key];
            set => values[key] = value;
        }
 
        public override void Serialize(JsonWriter writer)
        {
            writer.BeginObject();
            foreach (var pair in values)
            {
                writer.Write(pair.Key, pair.Value);
            }
            writer.EndObject();
        }
        
        public override string ToString()
        {
            return ToString(false);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string ToString(bool beautify)
        {
            JsonWriter writer = new (beautify);
            Serialize(writer);
            return writer.GetResult();
        }

        public void Add(string key, JsonValue value)
        {
            values.Add(key, value);
        }
        
        public void Remove(string key)
        {
            values.Remove(key);
        }
        
        public bool ContainsKey(string key)
        {
            return values.ContainsKey(key);
        }
        
        public JsonValue Get(string key, JsonValue fallback = null)
        {
            return values.GetValueOrDefault(key, fallback);
        }
        
        public T Get<T>(string key, T fallback = default)
        {
            return (T) (object) values.GetValueOrDefault(key, fallback as JsonValue);
        }
        
        public JsonList GetList(string key)
        {
            return (JsonList) values.GetValueOrDefault(key, new JsonList());
        }
        
        public JsonObject GetObject(string key)
        {
            return (JsonObject) values.GetValueOrDefault(key, new JsonObject());
        }
        
        public void Clear()
        {
            values.Clear();
        }
        
        public IEnumerator<KeyValuePair<string, JsonValue>> GetEnumerator()
        {
            return values.GetEnumerator();
        }
    }

    [Serializable]
    public class JsonList : JsonValue, IEnumerable<JsonValue>
    {
        [SerializeReference] 
        private List<JsonValue> values = new();

        public int Count => values.Count;
        
        public JsonValue this[int index]
        {
            get => values[index];
            set => values[index] = value;
        }
        
        public override void Serialize(JsonWriter writer)
        {
            writer.BeginArray();
            foreach (var t in values)
            {
                writer.Write(t);
            }
            writer.EndArray();
        }
        
        public override string ToString()
        {
            return ToString(false);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string ToString(bool beautify)
        {
            JsonWriter writer = new (beautify);
            Serialize(writer);
            return writer.GetResult();
        }
        
        public void Add(JsonValue value)
        {
            values.Add(value);
        }
        
        public void RemoveAt(int index)
        {
            values.RemoveAt(index);
        }
        
        public T Get<T>(int index)
        {
            return (T) (object) values[index];
        }
        
        public JsonList GetList(int index)
        {
            return (JsonList) values[index];
        }
        
        public JsonObject GetObject(int index)
        {
            return (JsonObject) values[index];
        }
        
        public void Clear()
        {
            values.Clear();
        }
        
        public void AddRange(IEnumerable<JsonValue> collection)
        {
            values.AddRange(collection);
        }
        
        public void Insert(int index, JsonValue value)
        {
            values.Insert(index, value);
        }
        
        public void Remove(JsonValue value)
        {
            values.Remove(value);
        }
        
        public bool Contains(JsonValue value)
        {
            return values.Contains(value);
        }
        
        public int IndexOf(JsonValue value)
        {
            return values.IndexOf(value);
        }

        public IEnumerator<JsonValue> GetEnumerator()
        {
            return values.GetEnumerator();
        }
    }
}