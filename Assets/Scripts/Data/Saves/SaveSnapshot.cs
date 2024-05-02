using System;
using UnityEngine;

namespace Solis.Data.Saves
{
    [Serializable]
    public class SaveSnapshot
    {
        public DateTime lastModificationTime;
        public Texture2D preview;
        public float playTime;
        public string name;
    }
}