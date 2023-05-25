using UnityEngine;

[CreateAssetMenu(fileName = "TileVariant", menuName = "ScriptableObjects/TileVariant", order = 1)]
public class TileVariant : ScriptableObject
{
    public string displayName;
    public Sprite icon;
    public Color spriteColor;
    public int cost;
    public int fee;
    public string description;
    public bool government;
    public GameObject extraEvents;
}