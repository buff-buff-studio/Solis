using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ToStringInUI : MonoBehaviour
{
    private Slider slider;
    private TextMeshProUGUI text;
    public string prefix;
    public string suffix;
    public string format = "{0}";
    
    #if UNITY_EDITOR
    public string placeholder = "N/A";
    #endif
    
    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        slider = GetComponentInParent<Slider>();
        if (slider == null) Destroy(this.gameObject);
    }

    private void FixedUpdate()
    {
        SetText(slider.value);
    }

    public void SetText(float value)
    {
        text.text = $"{prefix}{value.ToString(format)}{suffix}";
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();
        text.text = $"{prefix}{placeholder}{suffix}";
    }
    
#endif
}
