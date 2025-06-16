using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Script_PlayerInfoUIInit : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] Image playerAvatar;

    public void Init(string playerName, Texture2D playerIcon)
    {
        nameText.text = playerName;
        playerAvatar.sprite = Sprite.Create(playerIcon, new Rect(0, 0, playerIcon.width, playerIcon.height), new Vector2(0.5f, 0.5f));
    }
}
