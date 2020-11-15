﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 dir = Vector2.zero;

    public void Throw(float distance, Vector2 direction, float speed)
    {
        Debug.LogError(speed);
        dir = direction;
        StartCoroutine(MoveCoroutine(distance, direction, speed));
    }

    private IEnumerator MoveCoroutine(float distance, Vector2 direction, float speed)
    {
        for (float curDist = 0f; curDist < distance; curDist += speed * Time.deltaTime)
        {
            transform.position += (Vector3)direction * speed * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        FadeOut();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        Unicorn unicorn = collision.GetComponent<Unicorn>();

       if (unicorn)
       {
           unicorn.Stun();
       }
        
       if (!collision.CompareTag("Player"))
        Explode();
    }

    private void Explode()
    {
        print("explode");
        Destroy(gameObject);
    }

    private void FadeOut()
    {
        print("fade out");
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        StopCoroutine("MoveCoroutine");
    }
}