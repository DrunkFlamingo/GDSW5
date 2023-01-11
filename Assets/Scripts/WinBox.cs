using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinBox : MonoBehaviour
{
    // a trigger box which tells the game to win when the player enters it.

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            Game.Instance.StartGameWin();
        }
    }
}
