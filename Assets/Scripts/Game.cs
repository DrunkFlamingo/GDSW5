using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Game : MonoBehaviour
{
    [SerializeField] AudioClip soundtrackClip;
    [SerializeField] List<GameObject> postGameButtons;
    [SerializeField] bool playSoundtrack = true;
    [SerializeField] public float rewindTime = 1.75f;
    [SerializeField] public float snapShotInterval = 0.05f;

    public static Game Instance { get; private set; }
    public bool gameIsOver { get; private set; }
    private AudioSource audioSource;

    private Vector3 spriteScaleIncrement = new Vector3(0.85f, 0.15f, 0.15f);

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
        //DontDestroyOnLoad(gameObject);
        audioSource = gameObject.AddComponent<AudioSource>();
        foreach (GameObject button in postGameButtons) {
            button.SetActive(false);
        }
    }

    void Start()
    {
        if (soundtrackClip == null || !playSoundtrack) {
            return;
        }
        audioSource.loop = true;
        audioSource.clip = soundtrackClip;
        audioSource.Play();

        foreach (GameObject button in postGameButtons) {
            button.SetActive(false);
        }
    }

    public void StartMainGame() {
        Debug.Log("StartMainGame");
    }

    private IEnumerator LoadScene(string sceneName, float delay = 2f) {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator BloodPuddle(GameObject player, bool reverse = false) {
        Transform puddle = player.transform.Find("BloodPuddle");
        float newScale = puddle.localScale.x;
        if (reverse) {
            while (puddle.localScale.x > 1.5f) {
                newScale -= spriteScaleIncrement.x;
                puddle.localScale = new Vector3(newScale, newScale, newScale);
                yield return new WaitForSeconds(0.05f);
            }
        } else {
            while (puddle.localScale.x < 3f) {
                newScale += spriteScaleIncrement.x;
                puddle.localScale = new Vector3(newScale, newScale, newScale);
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    public void StartGameOver(GameObject player) {
        if (gameIsOver) {
            return;
        }
        gameIsOver = true;
        Debug.Log("StartGameOver");
        player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        StartCoroutine(BloodPuddle(player));

        Obstacle[] obstacles = FindObjectsOfType<Obstacle>();
        foreach (Obstacle obstacle in obstacles) {
            obstacle.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        foreach (GameObject button in postGameButtons) {
            button.SetActive(true);
        }
    }

    public void StartGameWin() {
        Debug.Log("StartGameWin");
        StartCoroutine(LoadScene("WinGame"));
    }

    IEnumerator ResumeAfterRewind() {
        yield return new WaitForSeconds(rewindTime+snapShotInterval);
                Intersection[] intersections = FindObjectsOfType<Intersection>();
        foreach (Intersection intersection in intersections) {
            intersection.UpdateIntersectionAfterRewind();
        }
        gameIsOver = false;

    }

    public void RewindGame() {
        Debug.Log("RewindGame");
        gameIsOver = true;
        //LoadScene("MainGame");
        foreach (GameObject button in postGameButtons) {
            button.SetActive(false);
        }
        StartCoroutine(Player.Instance.Rewind());
        Obstacle[] obstacles = FindObjectsOfType<Obstacle>();
        foreach (Obstacle obstacle in obstacles) {
            StartCoroutine(obstacle.Rewind());
        }


        StartCoroutine(ResumeAfterRewind());
        StartCoroutine(BloodPuddle(GameObject.Find("PlayerCharacter"), true));

    }

    public void ContinueGame() {
        StartCoroutine(LoadScene("LoseGame", 0f));
    }
}
