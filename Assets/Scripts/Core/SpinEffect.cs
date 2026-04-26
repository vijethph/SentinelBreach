using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Continuously rotates the collectible to draw player attention.</summary>
public class SpinEffect : MonoBehaviour
{
    public float degreesPerSecond = 180f;

    void Update()
    {
        transform.Rotate(0f, degreesPerSecond * Time.deltaTime, 0f, Space.World);
    }
}