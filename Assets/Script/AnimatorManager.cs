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

    [Header("Configuration")]
    [SerializeField] private CharacterAnimations characterAnimations;

    [Header("Références")]
    [SerializeField] private SelectPlayer selectPlayer;
    [SerializeField] private Transform characterParent;

    private Animator currentAnimator;
    private string currentCharacterType;

    private void Start()
    {
        // Correction du nom de la méthode
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
        // Vérification du tag du joueur
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
    }

    private void OnDestroy()
    {
        // Nettoyage des listeners
        if (selectPlayer != null)
        {
            selectPlayer.OnCharacterSelected.RemoveListener(HandleCharacterSelected);
        }
    }
}