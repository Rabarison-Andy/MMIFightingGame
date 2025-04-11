using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorManager : MonoBehaviour
{
    [System.Serializable]
    public class CharacterAnimations
    {
        public RuntimeAnimatorController monkeyController;
        public RuntimeAnimatorController samuraiController;
    }

    [SerializeField] private CharacterAnimations characterAnimations;
    [SerializeField] private SelectPlayer selectPlayer;
    [SerializeField] private Transform characterParent;

    private Animator currentAnimator;
    private string currentCharacterType;

    public string CurrentCharacterType => currentCharacterType;
    public bool IsReady { get; private set; }

    private void Start()
    {
        selectPlayer.OnCharacterSelected.AddListener(HandleCharacterSelected);
        InitializeCurrentCharacter();
    }

    private void InitializeCurrentCharacter()
    {
        foreach (Transform child in characterParent)
        {
            if (child.gameObject.activeSelf)
            {
                currentCharacterType = child.name.Contains("monkey") ? "monkey" : "samurai";
                UpdateAnimatorController();
                break;
            }
        }
    }

    private void HandleCharacterSelected(int playerNumber, string characterType)
    {
        string playerTag = playerNumber == 1 ? "Player1" : "Player2";
        
        if (gameObject.CompareTag(playerTag))
        {
            currentCharacterType = characterType;
            UpdateAnimatorController();
        }
    }

    private void UpdateAnimatorController()
    {
        currentAnimator = GetComponent<Animator>();
    
        switch (currentCharacterType)
        {
            case "monkey":
                currentAnimator.runtimeAnimatorController = characterAnimations.monkeyController;
                break;
            case "samurai":
                currentAnimator.runtimeAnimatorController = characterAnimations.samuraiController;
                break;
        }
    
    IsReady = true; // Nouveau flag
}

    private void OnDestroy()
    {
        if (selectPlayer != null)
        {
            selectPlayer.OnCharacterSelected.RemoveListener(HandleCharacterSelected);
        }
    }
}