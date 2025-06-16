using UnityEngine;

public interface I_Mods
{
    public enum Rarity { Common, Rare, Epic, Legendary, EXOTIC}
    public Rarity rarity { get; set; }
    public string modName {get; set;}
    public string modDescription {get; set;}
    public Sprite modIcon { get; set; }

    public void Activate();
    public void Deactivate();
}
