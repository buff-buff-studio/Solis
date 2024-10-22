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

    private const float MaxLifeTime = 4.75f;
    private FloatNetworkValue _lifeTime = new(0);
    private float _positionY;
    private Rigidbody _rigidbody;

    private void OnEnable()
    {
        WithValues(_lifeTime);

        InvokeRepeating(nameof(CheckTick), 0, 1f / checkTickRate);
        _rigidbody = GetComponent<Rigidbody>();

        _positionY = transform.position.y;
        if (_lifeTime.CheckPermission())
            _lifeTime.Value = MaxLifeTime;
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(CheckTick));
    }

    private void Update()
    {
        if(!HasAuthority) return;

        //if the cloud have a different Y position, the cloud will return to the original Y position
        if (!Mathf.Approximately(transform.position.y, _positionY))
        {
            var distance = _positionY - transform.position.y;
            _rigidbody.AddForce(Vector3.up * (distance * 10 * Time.deltaTime), ForceMode.Force);
        }

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
