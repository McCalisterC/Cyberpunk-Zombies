using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableObject_Shot", menuName = "Scriptable Objects/ScriptableObject_Shot")]
public class ScriptableObject_Shot : ScriptableObject
{
    public Color shotColor;
    public string shotInternalName;
    public string shotPublicName;
    public string shotDescription;
    public int cost;

    public float scalePercentOrValue;

}
