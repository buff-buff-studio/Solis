using System;
using NetBuff.Components;
using NetBuff.Misc;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class TextScaler : NetworkBehaviour
    {
        public TMP_Text text;
        [Range(0f, 1f)]
        public float progress = 0;
        [SerializeField]private float typeDuration = 2;
        public bool isWriting;
        private Action onFinishWriting;

        public void Update()
        {
            SetProgress();
            WriteText();
        }

        public void SetText(string dialog, Action callback)
        {
            text.text = dialog;
            progress = 0;
            isWriting = true;
            onFinishWriting = callback;
        }

        private void SetProgress()
        {
            if (!isWriting) return;
            
            progress += Time.deltaTime / typeDuration;

            if (!(progress >= 1)) return;
            progress = 1;
            onFinishWriting?.Invoke();
            isWriting = false;
        }

        private void WriteText()
        { 
            //scale characters by progress
            if (text == null) return;
            if(!isWriting) return;
            
            text.ForceMeshUpdate();

            var charVertices = new Vector3[4];
            
            for (var i = 0; i < text.textInfo.characterCount; i++)
            {
                var c = text.textInfo.characterInfo[i];
                
                if(!c.isVisible)
                    continue;
                
                var charInfo = text.textInfo.characterInfo[i];
                var meshInfo = text.textInfo.meshInfo[charInfo.materialReferenceIndex];
                var vertexIndex = charInfo.vertexIndex;
                var vertices = meshInfo.vertices;
                
                for (var j = 0; j < 4; j++)
                    charVertices[j] = vertices[vertexIndex + j];

                var center = (charVertices[1] + charVertices[3]) / 2;
                var charProgress = Mathf.Clamp01(progress * text.textInfo.characterCount - i);
                for (var j = 0; j < 4; j++)
                {
                    vertices[vertexIndex + j] = center + (charVertices[j] - center) * charProgress;
                }
            }
            
            text.UpdateVertexData();
        }
    }
}