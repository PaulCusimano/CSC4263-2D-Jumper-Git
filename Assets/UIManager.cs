using UnityEngine;
using TMPro; // ← THIS LINE IS CRUCIAL

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI heightText; // ← Must say "TextMeshProUGUI"
    public PlayerController player;    // ← Must say "PlayerController"

    void Update()
    {
        heightText.text = "Height: " + Mathf.Round(player.GetHeight()) + "m";
    }
}