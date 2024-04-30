using System;
using Solis.Data.JSON;

namespace Solis.Data
{
    [Serializable]
    public class SaveData
    {
        public int currentLevel = 0;

        public void WriteToJson(JsonObject @object)
        {
        }

        public void ReadFromJson(JsonObject @object)
        {
        }
    }
}