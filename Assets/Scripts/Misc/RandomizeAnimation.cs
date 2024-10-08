using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomizeAnimation : MonoBehaviour
{
    [SerializeField]
    private Vector2 speedRange = new Vector2(0.2f, 0.3f);
    private void Start()
    {
        if (TryGetComponent(out Animator anim))
        {
            var state = anim.GetCurrentAnimatorStateInfo(0);
            float randomStart = UnityEngine.Random.Range(0f, state.length);
            anim.SetFloat("Speed", Random.Range(speedRange.x, speedRange.y));
            anim.Play(state.fullPathHash, 0, randomStart / state.length);
        }
    }
}
