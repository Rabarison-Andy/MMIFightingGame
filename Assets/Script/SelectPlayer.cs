using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SelectPlayer : MonoBehaviour
{
    [SerializeField] private GameObject selectionMenu;
    [SerializeField] private Button monkeyButtonP1;
    [SerializeField] private Button samuraiButtonP1;
    [SerializeField] private Button monkeyButtonP2;
    [SerializeField] private Button samuraiButtonP2;
    [SerializeField] private Transform player1Parent;
    [SerializeField] private Transform player2Parent;

    public UnityEvent<int, string> OnCharacterSelected = new UnityEvent<int, string>();

    private const string MonkeyPrefabName = "monkeyPrefab";
    private const string SamuraiPrefabName = "samuraiPrefab";
    private const string MonkeyCharacterType = "Monkey";
    private const string SamuraiCharacterType = "Samurai";
    
    private GameObject player1Monkey;
    private GameObject player1Samurai;
    private GameObject player2Monkey;
    private GameObject player2Samurai;
    
    private bool isPlayer1Selected;
    private bool isPlayer2Selected;

    private void Start()
    {
        PauseGame();
        CacheCharacterPrefabs();
        InitializeCharacterVisibility();
        SetupButtonListeners();
    }

    private void PauseGame() => Time.timeScale = 0;

    private void CacheCharacterPrefabs()
    {
        player1Monkey = FindChildPrefab(player1Parent, MonkeyPrefabName);
        player1Samurai = FindChildPrefab(player1Parent, SamuraiPrefabName);
        player2Monkey = FindChildPrefab(player2Parent, MonkeyPrefabName);
        player2Samurai = FindChildPrefab(player2Parent, SamuraiPrefabName);
    }

    private GameObject FindChildPrefab(Transform parent, string prefabName)
    {
        Transform child = parent.Find(prefabName);
        if (child == null)
        {
            Debug.LogError($"Missing {prefabName} under {parent.name}");
            enabled = false;
            return null;
        }
        return child.gameObject;
    }

    private void InitializeCharacterVisibility()
    {
        SetActiveStateForAllPrefabs(false);
    }

    private void SetActiveStateForAllPrefabs(bool state)
    {
        player1Monkey.SetActive(state);
        player1Samurai.SetActive(state);
        player2Monkey.SetActive(state);
        player2Samurai.SetActive(state);
    }

    private void SetupButtonListeners()
    {
        monkeyButtonP1.onClick.AddListener(() => HandleSelection(1, MonkeyCharacterType));
        samuraiButtonP1.onClick.AddListener(() => HandleSelection(1, SamuraiCharacterType));
        monkeyButtonP2.onClick.AddListener(() => HandleSelection(2, MonkeyCharacterType));
        samuraiButtonP2.onClick.AddListener(() => HandleSelection(2, SamuraiCharacterType));
    }

    private void HandleSelection(int playerNumber, string characterType)
    {
        if (IsSelectionInvalid(playerNumber)) return;

        UpdateCharacterModel(playerNumber, characterType);
        NotifySelection(playerNumber, characterType);
        CheckGameStartCondition();
    }

    private bool IsSelectionInvalid(int playerNumber)
    {
        return (playerNumber == 1 && isPlayer1Selected) || 
               (playerNumber == 2 && isPlayer2Selected);
    }

    private void UpdateCharacterModel(int playerNumber, string characterType)
    {
        bool isMonkey = characterType == MonkeyCharacterType;
        
        if (playerNumber == 1)
        {
            player1Monkey.SetActive(isMonkey);
            player1Samurai.SetActive(!isMonkey);
            isPlayer1Selected = true;
        }
        else
        {
            player2Monkey.SetActive(isMonkey);
            player2Samurai.SetActive(!isMonkey);
            isPlayer2Selected = true;
        }
    }

    private void NotifySelection(int playerNumber, string characterType)
    {
        OnCharacterSelected.Invoke(playerNumber, characterType.ToLower());
    }

    private void CheckGameStartCondition()
    {
        if (isPlayer1Selected && isPlayer2Selected)
        {
            selectionMenu.SetActive(false);
            ResumeGame();
            enabled = false;
        }
    }

    private void ResumeGame() => Time.timeScale = 1;

    public void ResetSelections()
    {
        isPlayer1Selected = false;
        isPlayer2Selected = false;
        SetActiveStateForAllPrefabs(false);
        
        if (selectionMenu != null) selectionMenu.SetActive(true);
        
        ResetAllHealthBars();
        PauseGame();
    }

    private void ResetAllHealthBars()
    {
        foreach (HealthBar healthBar in FindObjectsOfType<HealthBar>())
        {
            healthBar.ResetHealth();
        }
    }
}