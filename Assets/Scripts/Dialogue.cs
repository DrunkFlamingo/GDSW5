using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Dialogue : MonoBehaviour
{
    [SerializeField] private List<string> dialogueLines;
    [SerializeField] private float timeBetweenCharacters = 0.05f;
    [SerializeField] private float fullStopTime = 0.5f;
    [SerializeField] private string nextScene; 

    private string currentDialogue;
    private int currentDialogueIndex = 0;
    private int currentCharacter = 0;

    private string currentText = "";
    private bool isTyping = false;

    bool isFullStop(char c) {
        return c == '.' || c == '!' || c == '?' || c == ';';
    }

    private IEnumerator TypeSpeakerDialogue()
    {
        
        foreach (char letter in currentDialogue.ToCharArray())
        {
            currentText += letter;

            if (isFullStop(letter)) 
            {
                yield return new WaitForSeconds(fullStopTime);
            }
            else
            {
                yield return new WaitForSeconds(timeBetweenCharacters);
            }
        }
        currentText += "\n";
        isTyping = false;
    }

    private IEnumerator SceneChange()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(nextScene);
    }

    void OnGUI()
    {
        GetComponent<TextMeshProUGUI>().text = currentText;
        if (currentDialogue == null) {
            currentDialogue = dialogueLines[0];
            currentDialogueIndex = 0;
            isTyping = true;
            StartCoroutine(TypeSpeakerDialogue());
        }
        if (!isTyping) {
            int nextDialogueIndex = currentDialogueIndex + 1;
            if (nextDialogueIndex < dialogueLines.Count) {
                currentDialogue = dialogueLines[nextDialogueIndex];
                currentDialogueIndex = nextDialogueIndex;
                isTyping = true;
                StartCoroutine(TypeSpeakerDialogue());
            } else if (nextScene != null && nextScene != "") {
                StartCoroutine(SceneChange());
            }
        }
    }
}
