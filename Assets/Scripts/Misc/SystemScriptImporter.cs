#if UNITY_EDITOR
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;
#endif

namespace SolarBuff.Misc
{
    #if UNITY_EDITOR
    //unity .lua asset scriptable importer
    [ScriptedImporter(1, "lua")]
    public class SystemScriptImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var text = File.ReadAllText(ctx.assetPath);
            var script = new TextAsset(text);
            ctx.AddObjectToAsset("script", script);
            ctx.SetMainObject(script);
        }
    }
    #endif
}