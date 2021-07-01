using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents inst;

    public event Action<Block> BlockDropEvent;
    public void BlockDrop(Block blk) {
        BlockDropEvent?.Invoke(blk);
    }

    void Start()
    {
        if (inst == null) inst = this;
        else Destroy(this);
    }
}
