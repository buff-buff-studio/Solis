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
            @object["currentLevel"] = currentLevel;
        }

        public void ReadFromJson(JsonObject @object)
        {
            currentLevel = @object.Get("currentLevel");
        }
    }
}