using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gère les mouvements et la rotation du personnage
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerControler : MonoBehaviour
{
    // Vitesses de déplacement
    [SerializeField]
    private float walkSpeed = 3.0f;
    private float runSpeed;

    //Animator du personnage
    private Animator animatorMonkey;

    // Direction initiale du personnage
    [SerializeField]
    private bool isLookingLeft = false;

    // Dégâts des attaques
    [SerializeField]
    private float punchDamage = 10f;
    [SerializeField]
    private float kickDamage = 20f;
    [SerializeField]
    private float attackRange = 1.5f;

    // Durée des animations d'attaque
    [SerializeField]
    private float punchDuration = 0.5f;
    [SerializeField]
    private float kickDuration = 0.7f;

    // Canva qui apparait lors de la mort d'un jour
    [SerializeField]
    private GameObject deathCanvasPrefab;

    // Composants et états
    private CharacterController characterController;
    private HealthBar healthBar;
    private Vector2 moveDirection = Vector2.zero;
    private bool isRunning = false;
    private bool isFacingLeft;
    private bool isDead = false;
    
    // États des attaques
    private bool isPunching = false;
    private bool isKicking = false;
    private float currentPunchTime = 0f;
    private float currentKickTime = 0f;

    // IDs des paramètres de l'Animator pour l'optimisation
    private readonly int triggerRunMonkey = Animator.StringToHash("TriggerRunMonkey");
    private readonly int triggerPunchMonkey = Animator.StringToHash("TriggerPunchMonkey");
    private readonly int triggerKickMonkey = Animator.StringToHash("TriggerKickMonkey");
    private readonly int triggerChasedStepMonkey = Animator.StringToHash("TriggerChasedStepMonkey");
    private readonly int boolIsRunning = Animator.StringToHash("IsRunning"); // Supposition: un booléen pour l'état de course est plus courant
    private readonly int triggerDeath = Animator.StringToHash("TriggerDeath");

    private void Awake()
    {
        // On s'assure d'avoir les composants dès le début
        characterController = GetComponent<CharacterController>();
        animatorMonkey = GetComponent<Animator>();
        healthBar = GetComponent<HealthBar>();
        
        if (characterController == null)
        {
            Debug.LogError("CharacterController non trouvé sur " + gameObject.name);
            enabled = false; // Désactive le script si pas de CharacterController
            return;
        }
        if (animatorMonkey == null)
        {
            Debug.LogError("Animator non trouvé sur " + gameObject.name);
            enabled = false; // Désactive le script si pas d'Animator
            return;
        }
        if (healthBar == null)
        {
            Debug.LogError("HealthBar manquant sur " + gameObject.name);
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        InitializeComponents();
        SetupInitialState();
    }

    // Initialise les composants nécessaires
    private void InitializeComponents()
    {
        runSpeed = walkSpeed * 3.75f;
        
        // Vérification supplémentaire avant d'ajouter le listener
        if (healthBar != null)
        {
            healthBar.OnDeath.AddListener(HandleDeath);
        }
        else
        {
            Debug.LogError("Référence HealthBar non assignée");
            enabled = false;
        }
    }

    // Configure l'état initial du personnage
    private void SetupInitialState()
    {
        isFacingLeft = isLookingLeft;
        UpdatePlayerRotation();
    }

    private void HandleDeath(GameObject deadPlayer)
    {
        if (isDead) return;

        isDead = true;
        
        // Arrêt des mouvements
        walkSpeed = 0;
        runSpeed = 0;
        moveDirection = Vector2.zero;
        
        // Désactivation des composants
        characterController.enabled = false;
        
        // Animation de mort
        animatorMonkey.SetTrigger(triggerDeath);
        
        // Affichage du canvas de mort
        if (deathCanvasPrefab != null)
        {
            Instantiate(deathCanvasPrefab, Vector3.zero, Quaternion.identity);
        }
        
        // Désactive ce script après un court délai
        Invoke("DisableScript", 0.5f);
    }

    /// Désactive ce script
    private void DisableScript()
    {
        enabled = false;
    }

    // Gère le déplacement du personnage
    public void OnMove(InputAction.CallbackContext context) 
    {
        if (isDead) return;

        moveDirection = context.ReadValue<Vector2>();
        UpdatePlayerDirection();

        // Déclenche l'animation de marche/pas chassé si on bouge mais ne court pas
        if (!isRunning && moveDirection.x != 0 && !IsAttacking())
        {
            animatorMonkey?.SetTrigger(triggerChasedStepMonkey);
        }
    }

    // Met à jour la direction du personnage pendant la course
    private void UpdatePlayerDirection()
    {
        if (isRunning && moveDirection.x != 0)
        {
            isFacingLeft = moveDirection.x < 0;
            UpdatePlayerRotation();
        }
    }

    // Applique la rotation du personnage
    private void UpdatePlayerRotation()
    {
        float rotationAngle = isFacingLeft ? -90 : 90;
        transform.rotation = Quaternion.Euler(0, rotationAngle, 0);
    }

    // Gère l'état de course du personnage
    public void OnRun(InputAction.CallbackContext context) 
    {
        if (context.started || context.performed)
        {
            StartRunning();
        }
        else if (context.canceled)
        {
            StopRunning();
        }
    }

    // Active le mode course
    private void StartRunning()
    {
        isRunning = true;
        animatorMonkey?.SetBool(boolIsRunning, true); // Utilise un booléen pour la course
        // animatorMonkey?.SetTrigger(triggerRunMonkey); // Alternative si vous préférez un trigger
        if (moveDirection.x != 0)
        {
            isFacingLeft = moveDirection.x < 0;
            UpdatePlayerRotation();
        }
    }

    // Désactive le mode course
    private void StopRunning()
    {
        isRunning = false;
        animatorMonkey?.SetBool(boolIsRunning, false); // Utilise un booléen pour la course
    }

    // Gère l'attaque de coup de poing
    public void OnPunch(InputAction.CallbackContext context)
    {
        if (isDead) return;

        if (context.started && CanAttack())
        {
            StartPunch();
        }
    }

    // Gère l'attaque de coup de pied
    public void OnKick(InputAction.CallbackContext context)
    {
        if (isDead) return;

        if (context.started && CanAttack())
        {
            StartKick();
        }
    }

    // Vérifie si le personnage peut attaquer
    private bool CanAttack()
    {
        return !isPunching && !isKicking && !isRunning; // Empêche d'attaquer en courant ou pendant une autre attaque
    }

    // Démarre l'animation de coup de poing
    private void StartPunch()
    {
        isPunching = true;
        currentPunchTime = 0f;
        animatorMonkey?.SetTrigger(triggerPunchMonkey);
        TryAttack(punchDamage);
    }

    // Démarre l'animation de coup de pied
    private void StartKick()
    {
        isKicking = true;
        currentKickTime = 0f;
        animatorMonkey?.SetTrigger(triggerKickMonkey);
        TryAttack(kickDamage);
    }

    // Met à jour les animations d'attaque
    private void UpdateAttacks()
    {
        if (isPunching)
        {
            currentPunchTime += Time.deltaTime;
            if (currentPunchTime >= punchDuration)
            {
                isPunching = false;
            }
        }

        if (isKicking)
        {
            currentKickTime += Time.deltaTime;
            if (currentKickTime >= kickDuration)
            {
                isKicking = false;
            }
        }
    }

    /// Essaie d'attaquer un adversaire dans la portée
    private void TryAttack(float damage)
    {
        // Détermine la direction de l'attaque
        Vector3 attackDirection = isFacingLeft ? Vector3.left : Vector3.right;
        
        // Lance un rayon pour détecter l'adversaire
        RaycastHit[] hits = Physics.RaycastAll(transform.position, attackDirection, attackRange);
        
        foreach (RaycastHit hit in hits)
        {
            // Vérifie si on a touché un autre joueur
            HealthBar enemyHealth = hit.collider.GetComponent<HealthBar>();
            if (enemyHealth != null && hit.collider.gameObject != gameObject)
            {
                enemyHealth.TakeDamage(damage);
                break; // Ne frappe que le premier adversaire touché
            }
        }
    }

    // Mise à jour du mouvement à chaque frame
    void Update()
    {
        if (characterController != null)
        {
            UpdateAttacks();
            if (!IsAttacking()) // Ne permet le mouvement que si on n'attaque pas
            {
                MovePlayer();
            }
        }
    }

    // Déplace le personnage
    private void MovePlayer()
    {
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        Vector3 movement = new Vector3(moveDirection.x, 0, 0);
        characterController.Move(movement * Time.deltaTime * currentSpeed);
    }

    // Retourne si le personnage est en train d'attaquer
    public bool IsAttacking()
    {
        return isPunching || isKicking;
    }
}
