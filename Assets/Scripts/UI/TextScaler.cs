using System;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class TextScaler : MonoBehaviour
    {
        public TMP_Text text;
        [Range(0f, 1f)] public float progress = 0;
        [SerializeField] private float typeDuration = 2;
        public bool isWriting;
        private Action onFinishWriting;
        private bool _canApplyEffects;
        public List<EffectsAndWords> effectsAndWords;
        private string _currentText;

        public float shakeIntensity = 100f; // Intensidade do shake
        public float shakeSpeed = 100; // Velocidade do shake

        private TMP_MeshInfo[] cachedMeshInfo; // Armazena o estado original do mesh
        public float glitchIntensity = 1.0f; // Mais intensidade no deslocamento dos vértices
        public float glitchSpeed = 20.0f;    // Glitch mais rápido
        public float glitchFrequency = 0.3f; // Aumento na frequência dos glitches
        public Gradient rainbow;
     private void Update()
        {
            SetProgress();
            WriteText();
        }

        public void SetText(string dialog, Action callback)
        {
            _currentText = dialog;
            text.text = ""; // Esconder o texto inicialmente
            progress = 0;
            isWriting = true;
            onFinishWriting = callback;
            _canApplyEffects = false;

            text.ForceMeshUpdate();
            if (text.textInfo != null && text.textInfo.characterCount > 0)
            {
                cachedMeshInfo = text.textInfo.CopyMeshInfoVertexData();
            }
            else
            {
                Debug.LogWarning("Falha ao copiar dados do mesh: textInfo ainda não foi gerado corretamente.");
            }
        }

        private void SetProgress()
        {
            if (!isWriting) return;

            progress += Time.deltaTime / typeDuration;

            if (progress >= 1)
            {
                progress = 1;
                onFinishWriting?.Invoke();
                isWriting = false;
                _canApplyEffects = true;
            }
        }

        private void WriteText()
        {
            if (text == null || text.textInfo == null) return;
            if(_currentText == null) return;
            // Variáveis para processar o texto
            int charactersToShow = Mathf.FloorToInt(progress * _currentText.Length);
            string displayedText = ""; // Texto a ser exibido
            int actualCharCount = 0; // Contagem de caracteres reais (ignorando as tags)

            for (int i = 0; i < _currentText.Length && actualCharCount < charactersToShow; i++)
            {
                // Ignora tags como <sprite> ou <color> e trata como um único caractere
                if (_currentText[i] == '<')
                {
                    // Pula até o fechamento da tag
                    int closingTagIndex = _currentText.IndexOf('>', i);
                    if (closingTagIndex != -1)
                    {
                        displayedText += _currentText.Substring(i, closingTagIndex - i + 1);
                        i = closingTagIndex; // Pula para o final da tag
                        continue; // Não contamos a tag como um caractere visível
                    }
                }
                else
                {
                    // Adiciona caracteres normais ao texto
                    displayedText += _currentText[i];
                    actualCharCount++;
                }
            }

            // Atualiza o texto exibido
            text.text = displayedText;
            text.ForceMeshUpdate(); // Garante que o mesh está atualizado

            // Aplica efeitos de animação, como escalonamento e shake
            for (var i = 0; i < text.textInfo.characterCount; i++)
            {
                var charInfo = text.textInfo.characterInfo[i];

                if (!charInfo.isVisible) continue;

                var meshInfo = text.textInfo.meshInfo[charInfo.materialReferenceIndex];
                var vertexIndex = charInfo.vertexIndex;
                var vertices = meshInfo.vertices;

                var center = (vertices[vertexIndex + 1] + vertices[vertexIndex + 3]) / 2;
                var charProgress = Mathf.Clamp01(progress * _currentText.Length - i);

                // Aplica o progresso à escala das letras
                for (var j = 0; j < 4; j++)
                {
                    vertices[vertexIndex + j] = center + (vertices[vertexIndex + j] - center) * charProgress;
                }

                ApplyEffectsToCharacter(i);
            }

            // Atualiza os dados do TMP com as modificações feitas nos vértices
            text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }



        private void ApplyEffectsToCharacter(int charIndex)
        {
            TMP_TextInfo textInfo = text.textInfo;
            TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];

            if (!charInfo.isVisible) return;

            foreach (var effectAndWord in effectsAndWords)
            {
                int wordIndex = _currentText.IndexOf(effectAndWord.word, StringComparison.Ordinal);
                if (charInfo.index >= wordIndex && charInfo.index < wordIndex + effectAndWord.word.Length)
                {
                    Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                    int vertexIndex = charInfo.vertexIndex;

                    switch (effectAndWord.effects)
                    {
                        case Effects.Shake:
                            ApplyShake(vertices, vertexIndex, charIndex);
                            break;
                        case Effects.Big:
                            ApplyScale(vertices, vertexIndex, 1.25f);
                            break;
                        case Effects.Small:
                            ApplyScale(vertices, vertexIndex, 0.8f);
                            break;
                        case Effects.Rainbow:
                            ApplyRainbow(vertices, vertexIndex);
                            break;
                        case Effects.Glitch:
                            ApplyGlitch(vertices, vertexIndex, charIndex);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private void ApplyShake(Vector3[] vertices, int vertexIndex, int charIndex)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 originalPosition = vertices[vertexIndex + i];
                Vector3 shakeOffset = new Vector3(
                    Mathf.Sin(Time.time * shakeSpeed + i + charIndex) * shakeIntensity,
                    Mathf.Cos(Time.time * shakeSpeed + i + charIndex) * shakeIntensity,
                    0);

                vertices[vertexIndex + i] = originalPosition + shakeOffset;
            }

            text.mesh.vertices = vertices;
            text.canvasRenderer.SetMesh(text.mesh);
        }

        private void ApplyScale(Vector3[] vertices, int vertexIndex, float scale)
        {
            Vector3 center = (vertices[vertexIndex] + vertices[vertexIndex + 2]) / 2;
            for (int i = 0; i < 4; i++)
            {
                vertices[vertexIndex + i] = center + (vertices[vertexIndex + i] - center) * scale;
            }
        }

        private void ApplyRainbow(Vector3[] vertices, int vertexIndex)
        {
            Color32[] colors = text.textInfo.meshInfo[0].colors32;

            if (colors == null || colors.Length == 0)
            {
                colors = new Color32[text.textInfo.meshInfo[0].vertices.Length];
            }

            colors[vertexIndex] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[vertexIndex].x * 0.001f, 1f));
            colors[vertexIndex + 1] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[vertexIndex + 1].x * 0.001f, 1f));
            colors[vertexIndex + 2] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[vertexIndex + 2].x * 0.001f, 1f));
            colors[vertexIndex + 3] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[vertexIndex + 3].x * 0.001f, 1f));

            text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        private void ApplyGlitch(Vector3[] vertices, int vertexIndex, int charIndex)
        {
            if (Random.value < glitchFrequency)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector3 originalPosition = vertices[vertexIndex + i];
                    Vector3 glitchOffset = new Vector3(
                        Random.Range(-glitchIntensity, glitchIntensity),
                        Random.Range(-glitchIntensity, glitchIntensity),
                        0);

                    vertices[vertexIndex + i] = originalPosition + glitchOffset;
                }

                Color32[] colors = text.mesh.colors32;
                if (colors.Length == vertices.Length)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        colors[vertexIndex + i] = new Color32(
                            (byte)Random.Range(150, 255),
                            (byte)Random.Range(0, 100),
                            (byte)Random.Range(150, 255),
                            255);
                    }

                    text.mesh.colors32 = colors;
                }
            }

            text.mesh.vertices = vertices;
            text.canvasRenderer.SetMesh(text.mesh);
            text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }
}