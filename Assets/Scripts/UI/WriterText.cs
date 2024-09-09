/*using System;
using System.Collections.Generic;
using _Scripts.Helpers;
using TMPro;
using UnityEngine;

namespace _Scripts.Helpers
{
    /// <summary>
    /// Basic typewriter text for texts
    /// </summary>
    public class WriterText : MonoBehaviour
    {
        private static WriterText _instance;
        public static WriterText Instance => _instance ? _instance : FindFirstObjectByType<WriterText>();
        
        
        private List<TextWriterSingle> _textWriterSingle = new List<TextWriterSingle>();

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
        }

        public TextWriterSingle AddWriter(TextMeshProUGUI uiText, string textToWrite, float timerPerCharacter, bool invisibleCharacters, bool removeWriterBeforeAdd, Action onFinishWriting = null)
        {
            if (removeWriterBeforeAdd)
            {
                RemoveWriter(uiText);
            }

            TextWriterSingle textWriter =
                new TextWriterSingle(uiText, textToWrite, timerPerCharacter, invisibleCharacters, onFinishWriting);
            _textWriterSingle.Add(textWriter);
            return textWriter;
        }
        public void ResetWriter()
        {
            foreach (var text in _textWriterSingle)
            {
                text?.ResetWriter();
            }
        }
        
        public void RemoveWriter(TextMeshProUGUI uiText)
        {
            for (int i = 0; i < _textWriterSingle.Count; i++)
            {
                if (_textWriterSingle[i].GetText() != uiText) continue;
                _textWriterSingle.RemoveAt(i);
                i--;
            }
        }

        private void Update()
        {
            for (int i =0; i< _textWriterSingle.Count; i++)
            {
                var destroyInstance = _textWriterSingle[i].Update();
                
                if (!destroyInstance) continue;
                _textWriterSingle.RemoveAt(i);
                i--;
            }
        }
    }
}

public class TextWriterSingle
{
    private float _timer;
    private TextMeshProUGUI _uiText;
    private string _textToWrite;
    private float _timePerCharacter;
    private int _characterIndex;
    private bool _invisibleCharacters;
    public Action onFinishWriting;
    public bool isWriting = false;

    public TextWriterSingle(TextMeshProUGUI uiText, string textToWrite, float timerPerCharacter, bool invisibleCharacters, Action onFinishWriting = null)
    {
        _uiText = uiText;
        _textToWrite = textToWrite;
        _timePerCharacter = timerPerCharacter;
        _characterIndex = 0;
        _invisibleCharacters = invisibleCharacters;
        this.onFinishWriting = onFinishWriting;
    }
    
    /// <summary>
    /// Reset the text
    /// </summary>
    public void ResetWriter()
    {
        _characterIndex = 0;
        _uiText = null;
    }
    //return true on complete
    public bool Update()
    {
        _timer -= Time.deltaTime;
        
        while (_timer <= 0f)
        {
            //display next character
            _timer += _timePerCharacter;
            _characterIndex++;
            if (_characterIndex >= _textToWrite.Length)
            {
                return true;
            }
            string text = _textToWrite.Substring(0, _characterIndex);
            Debug.Log(text);
            var currentCharacter = _textToWrite.Substring(_characterIndex,1);
            if (currentCharacter == "<")
            {
                string auxString = currentCharacter;
                
                while (currentCharacter != ">")
                {
                    _characterIndex++;
                    currentCharacter = _textToWrite.Substring(_characterIndex,1);
                    auxString += currentCharacter;
                }
                
                text += auxString;
            }
            else
            {
                if (_invisibleCharacters)
                    text += _textToWrite.Substring(0, _characterIndex);
            }


            if (_uiText != null)
            {
                _uiText.text = "";
                _uiText.text = text;
            }

            if (_characterIndex >= _textToWrite.Length)
            {
                _uiText = null;
                onFinishWriting?.Invoke();
                return true;
            }

        }

        return false;
    }

    public TextMeshProUGUI GetText()
    {
        return _uiText;
    }

    public bool IsActive()
    {
        return _characterIndex < _textToWrite.Length;
    }

    public void WriteAllAndDestroy()
    {
        _uiText.text = _textToWrite;
        isWriting = false;
        _characterIndex = _textToWrite.Length;
        WriterText.Instance.RemoveWriter(_uiText);
        onFinishWriting?.Invoke();
    }
}*/