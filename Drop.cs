using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drop
{
    public Vector2 pos;
    public Vector2 dir = Vector2.right;
    public float vel, water, sediment;

    public Drop(int gridSize)
    {
        this.pos = new Vector2(
            Random.Range(0, gridSize - 1),
            Random.Range(0, gridSize - 1)
        );
    }
}
