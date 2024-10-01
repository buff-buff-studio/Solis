using System;
using System.Collections.Generic;
using NetBuff.Components;
using TMPro;
using UI;
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
        private bool _canApplyEffects;
        public List<EffectsAndWords> effectsAndWords;
        public void Update()
        {
            SetProgress();
            WriteText();
            
            if(!_canApplyEffects) return;
            ApplyEffects();
        }

        public void SetText(string dialog, Action callback)
        {
            text.text = dialog;
            progress = 0;
            isWriting = true;
            onFinishWriting = callback;
            _canApplyEffects = false;
        }

        private void SetProgress()
        {
            if (!isWriting) return;
            
            progress += Time.deltaTime / typeDuration;

            if (!(progress >= 1)) return;
            progress = 1;
            onFinishWriting?.Invoke();
            isWriting = false;
            _canApplyEffects = true;
        }

         private void ApplyEffects()
        {
            text.ForceMeshUpdate();
            TMP_TextInfo textInfo = text.textInfo;

            for (int i = 0; i < effectsAndWords.Count; i++)
            {
                var effectAndWord = effectsAndWords[i];
                ApplyEffectToWord(effectAndWord.word, effectAndWord.effects);
            }

            text.UpdateVertexData();
        }

        private void ApplyEffectToWord(string word, Effects effect)
        {
            TMP_TextInfo textInfo = text.textInfo;
            int wordIndex = text.text.IndexOf(word, StringComparison.Ordinal);

            if (wordIndex == -1) return;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                if (!charInfo.isVisible) continue;

                if (charInfo.index >= wordIndex && charInfo.index < wordIndex + word.Length)
                {
                    Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                    int vertexIndex = charInfo.vertexIndex;

                    switch (effect)
                    {
                        case Effects.Shake:
                            ApplyShake(vertices, vertexIndex);
                            break;
                        case Effects.Big:
                            ApplyScale(vertices, vertexIndex, 1.5f);
                            break;
                        case Effects.Small:
                            ApplyScale(vertices, vertexIndex, 0.5f);
                            break;
                    }
                }
            }
        }

        private void ApplyShake(Vector3[] vertices, int vertexIndex)
        {
            float shakeAmount = 0.1f; // Tamanho do "shake"
            for (int i = 0; i < 4; i++)
            {
                Vector3 randomOffset = new Vector3(
                    UnityEngine.Random.Range(-shakeAmount, shakeAmount),
                    UnityEngine.Random.Range(-shakeAmount, shakeAmount),
                    0);
                vertices[vertexIndex + i] += randomOffset;
            }
        }

        private void ApplyScale(Vector3[] vertices, int vertexIndex, float scale)
        {
            Vector3 center = (vertices[vertexIndex] + vertices[vertexIndex + 2]) / 2;
            for (int i = 0; i < 4; i++)
            {
                vertices[vertexIndex + i] = center + (vertices[vertexIndex + i] - center) * scale;
            }
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