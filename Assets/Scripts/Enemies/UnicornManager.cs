﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnicornManager : Manager<UnicornManager>
{
    private const float BLINK_TIME = 2f;

    private bool _blinkState = false;
    private List<Unicorn> _unicorns = null;

    private void Awake()
    {
        _unicorns = GameObject.FindObjectsOfType<Unicorn>().ToList();
        StartCoroutine(BlinkCoroutine());
    }

    private IEnumerator BlinkCoroutine()
    {
        List<Unicorn> _blinkyUnicorns = (from x in _unicorns where x.IsBlink select x).ToList();

        if (_blinkyUnicorns.Count == 0)
            yield break;
        while (true)
        {
            _blinkyUnicorns.ForEach(SetUnicornLight);
            yield return new WaitForSeconds(BLINK_TIME);
            _blinkState = !_blinkState;
        }
    }

    private void SetUnicornLight(Unicorn unicorn)
    {
        if (!unicorn.IsStun)
            unicorn.SetCornLight(_blinkState);
    }
}
