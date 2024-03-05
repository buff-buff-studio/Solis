using SolarBuff.i18n;
using TMPro;
using UnityEngine;

namespace SolarBuff.Interface
{
    [RequireComponent(typeof(TMP_Text))]
    public class Label : MonoBehaviour
    {
        private TMP_Text _text;
        private string _rawString;

        private void OnEnable()
        {
            _text = GetComponent<TMP_Text>();
            _rawString = _text.text;
            
            LanguageManager.Instance.onLanguageChange += Localize;
            Localize();
        }

        private void OnDisable()
        {
            if(LanguageManager.Instance != null)
                LanguageManager.Instance.onLanguageChange -= Localize;
            
            _text.text = _rawString;
            _text = null;
        }

        public void Localize()
        {
            _text.text = LanguageManager.Localize(_rawString);
        }
    }
}