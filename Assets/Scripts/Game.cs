using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Game : MonoBehaviour
{
    [SerializeField] AudioClip soundtrackClip;

    public static Game Instance { get; private set; }


    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void StartMainGame() {
        Debug.Log("StartMainGame");
    }

    public void StartGameOver() {
        Debug.Log("StartGameOver");

    }

    public void StartGameWin() {
        Debug.Log("StartGameWin");
    }
}
