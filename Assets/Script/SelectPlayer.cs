using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SelectPlayer : MonoBehaviour
{
    [System.Serializable]
    public class CharacterSelectedEvent : UnityEvent<int, string> { }

    [SerializeField] 
    private GameObject selectionMenu;
    public CharacterSelectedEvent OnCharacterSelected = new CharacterSelectedEvent();

    [SerializeField] 
    private Button monkeyButtonP1;
    [SerializeField] 
    private Button samuraiButtonP1;

    [SerializeField] 
    private Button monkeyButtonP2;
    [SerializeField] 
    private Button samuraiButtonP2;

    [SerializeField] 
    private Transform player1;
    [SerializeField] 
    private Transform player2;

    private bool isP1Selected = false;
    private bool isP2Selected = false;

    private GameObject p1Monkey;
    private GameObject p1Samurai;
    private GameObject p2Monkey;
    private GameObject p2Samurai;

    private void Start()
    {
        Time.timeScale = 0;
        if (player1 == null || player2 == null)
        {
            Debug.LogError("Les transforms des joueurs ne sont pas assignés!");
            enabled = false;
            return;
        }

        try
        {
            p1Monkey = player1.Find("monkeyPrefab").gameObject;
            p1Samurai = player1.Find("samuraiPrefab").gameObject;
            p2Monkey = player2.Find("monkeyPrefab").gameObject;
            p2Samurai = player2.Find("samuraiPrefab").gameObject;
        }
        catch (System.NullReferenceException)
        {
            Debug.LogError("Un prefab est manquant sous les transforms des joueurs!");
            enabled = false;
            return;
        }

        p1Monkey.SetActive(false);
        p1Samurai.SetActive(false);
        p2Monkey.SetActive(false);
        p2Samurai.SetActive(false);

        monkeyButtonP1.onClick.AddListener(() => SelectCharacter(1, "monkey"));
        samuraiButtonP1.onClick.AddListener(() => SelectCharacter(1, "samurai"));
        monkeyButtonP2.onClick.AddListener(() => SelectCharacter(2, "monkey"));
        samuraiButtonP2.onClick.AddListener(() => SelectCharacter(2, "samurai"));
    }

    private void SelectCharacter(int playerNumber, string character)
    {
        if (playerNumber == 1 && !isP1Selected)
        {
            isP1Selected = true;
            p1Monkey.SetActive(character == "monkey");
            p1Samurai.SetActive(character == "samurai");
            OnCharacterSelected.Invoke(1, character);
            CheckBothSelected();
        }
        else if (playerNumber == 2 && !isP2Selected)
        {
            isP2Selected = true;
            p2Monkey.SetActive(character == "monkey");
            p2Samurai.SetActive(character == "samurai");
            OnCharacterSelected.Invoke(2, character);
            CheckBothSelected();
        }
    }

    private void CheckBothSelected()
    {
        if (isP1Selected && isP2Selected)
        {
            selectionMenu.SetActive(false);
            Time.timeScale = 1;
            enabled = false;
        }
    }

    public void ResetSelection()
    {
        isP1Selected = false;
        isP2Selected = false;

        p1Monkey.SetActive(false);
        p1Samurai.SetActive(false);
        p2Monkey.SetActive(false);
        p2Samurai.SetActive(false);

        if (selectionMenu != null)
        {
            selectionMenu.SetActive(true);
        }

        foreach (HealthBar healthBar in FindObjectsOfType<HealthBar>())
        {
            healthBar.ResetHealth();
        }

        Time.timeScale = 0;
    }
}