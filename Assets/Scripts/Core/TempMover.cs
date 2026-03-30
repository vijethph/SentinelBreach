using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Temporary script to test corridor spawning — DELETE after Day 4
public class TempMover : MonoBehaviour
{
    public float speed = 8f;
    void Update() { transform.Translate(0, 0, speed * Time.deltaTime); }
}
