using System;
using Solis.Data.JSON;

namespace Solis.Data.Saves
{
    [Serializable]
    public class SaveData
    {
        public int currentLevel;

        public void WriteToJson(JsonObject @object)
        {
        }

        public void ReadFromJson(JsonObject @object)
        {
        }
    }
}