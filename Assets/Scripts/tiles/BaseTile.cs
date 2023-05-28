using System;
using UnityEngine;

[Serializable]
public abstract class BaseTile : MonoBehaviour
{
    public abstract void PlayerArrived(Player player);
    
}