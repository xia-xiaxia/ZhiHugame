using System.ComponentModel;
using JetBrains.Annotations;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameEventNode
{
    public GameEvent hEt;
    public GameEventNode next;
    public GameEventNode(GameEvent e,GameEventNode n = null)
    {
        hEt = e;
        next = n;
    }
}
