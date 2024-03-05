using System;
using UnityEngine;

namespace SolarBuff.Data
{
    public class SaveManager : SingletonBehaviour<SaveManager>
    {
        public class SaveInfo
        {
            public DateTime ModificationTime { get; set; }
            public Texture2D Snapshot { get; set; }
        }
    }
}