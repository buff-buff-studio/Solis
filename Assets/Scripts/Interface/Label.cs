using Solis.i18n;
using TMPro;
using UnityEngine;

namespace Interface
{
    [RequireComponent(typeof(TMP_Text))]
    [DisallowMultipleComponent]
    public class Label : MonoBehaviour
    {
        private TMP_Text _text;
        private string _buffer;
        private void OnEnable()
        {
            _text = GetComponent<TMP_Text>();
            _buffer = _text.text;
            
            LanguagePalette.OnLanguageChanged += _Localize;
            _Localize();
        }

        private void OnDestroy()
        {
            LanguagePalette.OnLanguageChanged -= _Localize;
        }

        private void _Localize()
        {
            _text.text = LanguagePalette.Localize(_buffer);
        }
    }
}