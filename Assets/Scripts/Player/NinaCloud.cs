using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NetBuff.Components;
using NetBuff.Misc;
using Solis.Circuit.Interfaces;
using Solis.Core;
using UnityEngine;

public class NinaCloud : NetworkBehaviour
{
    [Header("CHECK")]
    public float checkTickRate = 2;
    public Vector3 checkOffset = new(0, 0.1f, 0);
    public Vector3 checkSize = new(0.5f, 0.1f, 0.5f);

    private const float MaxLifeTime = 10;
    private FloatNetworkValue _lifeTime = new(0);

    private void OnEnable()
    {
        WithValues(_lifeTime);

        if (_lifeTime.CheckPermission())
            _lifeTime.Value = MaxLifeTime;
    }

    public override void OnSpawned(bool isRetroactive)
    {
        base.OnSpawned(isRetroactive);
        
        InvokeRepeating(nameof(CheckTick), 0, 1f / checkTickRate);
    }

    private void Update()
    {
        if(!HasAuthority) return;
        
        _lifeTime.Value -= Time.deltaTime;
        if (_lifeTime.Value <= 0)
        {
            Despawn();
        }
    }

    private void CheckTick()
    {
        if(!HasAuthority) return;
        if(!_CheckPlatform())
        {
            _lifeTime.Value = 0;
        }
    }
    
    private bool _CheckPlatform()
    {
        Collider[] results = Array.Empty<Collider>();
        var count = Physics.OverlapBoxNonAlloc(transform.position + checkOffset, checkSize / 2, results, transform.rotation);

        for (var i = 0; i < count; i++)
        {
            if (results[i] == null)
                continue;
                
            if (results[i].TryGetComponent(out IHeavyObject _))
                return false;
        }
            
        return !results.Take(count).Any(col => col.TryGetComponent(out IHeavyObject _));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + checkOffset, checkSize);
    }
}
