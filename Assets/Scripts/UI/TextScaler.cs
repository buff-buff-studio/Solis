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

        private void Update()
        {
            SetProgress();

            WriteText();

            // Aplicar os efeitos continuamente após o texto ser escrito
            /*if (!isWriting)
            {
                ApplyEffects();
            }
            else
            {
                WriteText();
            }*/
        }

        public void SetText(string dialog, Action callback)
        {
            text.text = dialog;
            _currentText = dialog;
            progress = 0;
            isWriting = true;
            onFinishWriting = callback;
            _canApplyEffects = false;

            // Cache para manter o estado original do mesh, evitando sobrescrever os efeitos
            text.ForceMeshUpdate(); // Atualiza o estado inicial do texto
            if (text.textInfo != null && text.textInfo.characterCount > 0)
            {
                // Cache para manter o estado original do mesh, evitando sobrescrever os efeitos
                cachedMeshInfo = text.textInfo.CopyMeshInfoVertexData(); // Caching dos dados do mesh
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

            if (!(progress >= 1)) return;
            progress = 1;
            onFinishWriting?.Invoke();
            isWriting = false;
            _canApplyEffects = true;
        }

        private void ApplyEffectsToCharacter(int charIndex)
        {
         
            TMP_TextInfo textInfo = text.textInfo;
            TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];

            if (!charInfo.isVisible) return;

            foreach (var effectAndWord in effectsAndWords)
            {
                int wordIndex = text.text.IndexOf(effectAndWord.word, StringComparison.Ordinal);
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
        public float glitchIntensity = 1.0f; // Mais intensidade no deslocamento dos vértices
        public float glitchSpeed = 20.0f;    // Glitch mais rápido
        public float glitchFrequency = 0.3f; // Aumento na frequência dos glitches
       private void ApplyGlitch(Vector3[] vertices, int vertexIndex, int charIndex)
{

    // Condicional para aplicar o efeito glitch de forma intermitente
    if (Random.value < glitchFrequency)
    {
        // Aplica a distorção nos quatro vértices que compõem o caractere
        for (int i = 0; i < 4; i++)
        {
            Vector3 originalPosition = vertices[vertexIndex + i]; // Posição original do vértice

            // Cria um deslocamento glitch baseado em valores aleatórios e o tempo
            Vector3 glitchOffset = new Vector3(
                Random.Range(-glitchIntensity, glitchIntensity),  // Deslocamento aleatório no eixo X
                Random.Range(-glitchIntensity, glitchIntensity),  // Deslocamento aleatório no eixo Y
                0);                                               // Sem alteração no eixo Z

            // Aplica o offset de glitch nos vértices do caractere
            vertices[vertexIndex + i] = originalPosition + glitchOffset;
        }

        // Opcional: Modificar as cores dos vértices para dar um efeito visual mais forte de glitch
        Color32[] colors = text.mesh.colors32;
        if (colors.Length == vertices.Length)
        {
            for (int i = 0; i < 4; i++)
            {
                // Aplica uma cor aleatória dentro de uma faixa para simular flashes de glitch
                colors[vertexIndex + i] = new Color32(
                    (byte)Random.Range(150, 255), // R
                    (byte)Random.Range(0, 100),   // G
                    (byte)Random.Range(150, 255), // B
                    255);                         // A (opacidade total)
            }

            // Atualiza as cores da malha
            text.mesh.colors32 = colors;
        }
    }

    // Atualiza os vértices e a malha do texto, independentemente se o glitch foi aplicado ou não
    text.mesh.vertices = vertices;
    text.canvasRenderer.SetMesh(text.mesh);
    text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
}
        public Gradient rainbow;
        private void ApplyRainbow(Vector3[] vertices, int vertexIndex)
        {
            // Certifique-se de que o array de cores seja do tipo correto (Color32)
            Color32[] colors = text.textInfo.meshInfo[0].colors32;

            if (colors == null || colors.Length == 0)
            {
                // Inicializa o array de cores com o tamanho correto, caso esteja vazio
                colors = new Color32[text.textInfo.meshInfo[0].vertices.Length];
            }

            // Aplica a cor arco-íris nos 4 vértices do caractere
            colors[vertexIndex] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[vertexIndex].x * 0.001f, 1f));
            colors[vertexIndex + 1] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[vertexIndex + 1].x * 0.001f, 1f));
            colors[vertexIndex + 2] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[vertexIndex + 2].x * 0.001f, 1f));
            colors[vertexIndex + 3] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[vertexIndex + 3].x * 0.001f, 1f));

            // Atualiza o mesh com os novos valores de cores
            text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
        private void ApplyScale(Vector3[] vertices, int vertexIndex, float scale)
        {
            Vector3 center = (vertices[vertexIndex] + vertices[vertexIndex + 2]) / 2;
            for (int i = 0; i < 4; i++)
            {
                vertices[vertexIndex + i] = center + (vertices[vertexIndex + i] - center) * scale;
            }
        }

        private void ApplyShake(Vector3[] vertices, int vertexIndex, int charIndex)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 originalPosition = vertices[vertexIndex + i]; // Usando diretamente os vértices atuais
                Vector3 shakeOffset = new Vector3(
                    Mathf.Sin(Time.time * shakeSpeed + i + charIndex) * shakeIntensity,
                    Mathf.Cos(Time.time * shakeSpeed + i + charIndex) * shakeIntensity,
                    0);

                // Aplica o offset de shake diretamente nos vértices
                vertices[vertexIndex + i] = originalPosition + shakeOffset;
            }
            
            text.mesh.vertices = vertices;
            text.canvasRenderer.SetMesh(text.mesh);
        }

        private void WriteText()
        {
            if (text == null || text.mesh == null) return;
            
            text.ForceMeshUpdate(); // Atualiza o mesh uma única vez para obter o estado dos vértices

            for (var i = 0; i < text.textInfo.characterCount; i++)
            {
                var charInfo = text.textInfo.characterInfo[i];

                if (!charInfo.isVisible) continue;

                var meshInfo = text.textInfo.meshInfo[charInfo.materialReferenceIndex];
                var vertexIndex = charInfo.vertexIndex;
                var vertices = meshInfo.vertices;

                var center = (vertices[vertexIndex + 1] + vertices[vertexIndex + 3]) / 2;
                var charProgress = Mathf.Clamp01(progress * text.textInfo.characterCount - i);

                // Aplica o progresso à escala das letras
                for (var j = 0; j < 4; j++)
                {
                    vertices[vertexIndex + j] = center + (vertices[vertexIndex + j] - center) * charProgress;
                }

                // Aplica os efeitos à medida que as letras aparecem
                ApplyEffectsToCharacter(i);
            }

            // Atualiza o mesh com as modificações feitas nos vértices
            text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }

        private void ApplyEffects()
        {
            if (text == null || text.mesh == null) return;

            text.ForceMeshUpdate(); // Garante que o mesh está atualizado

            for (var i = 0; i < text.textInfo.characterCount; i++)
            {
                var charInfo = text.textInfo.characterInfo[i];

                if (!charInfo.isVisible) continue;

                // Aplica os efeitos à medida que as letras aparecem
                ApplyEffectsToCharacter(i);
            }

            // Força a atualização do mesh após a aplicação dos efeitos
            text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }
    }
}