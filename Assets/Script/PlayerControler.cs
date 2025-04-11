using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(HealthBar))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float runSpeedMultiplier = 3.75f;
    [SerializeField] private bool initialFacingLeft = false;
    [SerializeField] private float punchDamage = 10f;
    [SerializeField] private float kickDamage = 20f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float punchDuration = 0.5f;
    [SerializeField] private float kickDuration = 0.7f;
    [SerializeField] private GameObject deathCanvasPrefab;
    [SerializeField] private float deathDisableDelay = 0.5f;
    [SerializeField] private float groundedHeight = 0.5f;

    private CharacterController characterController;
    private Animator characterAnimator;
    private HealthBar healthBar;
    private Vector2 moveInput;
    private float currentRunSpeed;
    private bool isRunning;
    private bool isFacingLeft;
    private bool isDead;
    private bool isPunching;
    private bool isKicking;
    private float elapsedPunchTime;
    private float elapsedKickTime;
    private bool isSamurai;

    private const string SamuraiPrefabName = "samuraiPrefab";
    private int isRunningAnimHash;
    private int deathTriggerAnimHash;
    private int deathBoolAnimHash;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        healthBar = GetComponent<HealthBar>();
        characterAnimator = GetComponentInChildren<AnimatorManager>()?.GetComponent<Animator>();

        ValidateComponents();
        InitializeAnimatorHashes();
    }

    private void Start()
    {
        currentRunSpeed = walkSpeed * runSpeedMultiplier;
        isFacingLeft = initialFacingLeft;
        UpdateCharacterRotation();
        DetectCharacterType();
        healthBar.OnDeath.AddListener(HandleDeath);
    }

    private void ValidateComponents()
    {
        if (characterController == null || characterAnimator == null || healthBar == null)
        {
            Debug.LogError("Missing required components on " + gameObject.name);
            enabled = false;
        }
    }

    private void InitializeAnimatorHashes()
    {
        isRunningAnimHash = Animator.StringToHash("IsRunning");
        deathTriggerAnimHash = Animator.StringToHash("TriggerDeath");
    }

    private void DetectCharacterType()
    {
        isSamurai = transform.Find(SamuraiPrefabName)?.gameObject.activeSelf ?? false;
        deathBoolAnimHash = Animator.StringToHash(isSamurai ? "DeadSamurai" : "DeadMonkey");
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (isDead) return;

        moveInput = context.ReadValue<Vector2>();
        UpdateFacingDirection();

        if (!isRunning && moveInput.x != 0 && !IsAttacking())
        {
            TriggerMovementAnimation();
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (isDead) return;

        if (context.performed) StartRunning();
        else if (context.canceled) StopRunning();
    }

    private void StartRunning()
    {
        if (characterAnimator.runtimeAnimatorController == null) return;

        isRunning = true;
        characterAnimator.SetBool(isRunningAnimHash, true);
        TriggerCharacterSpecificAnimation("Run");
    }

    private void StopRunning()
    {
        isRunning = false;
        characterAnimator.SetBool(isRunningAnimHash, false);
    }

    private void UpdateFacingDirection()
    {
        if (isRunning && moveInput.x != 0)
        {
            isFacingLeft = moveInput.x < 0;
            UpdateCharacterRotation();
        }
    }

    private void UpdateCharacterRotation()
    {
        float yRotation = isFacingLeft ? -90f : 90f;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    private void TriggerMovementAnimation()
    {
        TriggerCharacterSpecificAnimation("ChasedStep");
    }

    public void OnPunch(InputAction.CallbackContext context)
    {
        if (context.started && CanAttack()) StartPunch();
    }

    private void StartPunch()
    {
        isPunching = true;
        elapsedPunchTime = 0f;
        TriggerCharacterSpecificAnimation("Punch");
        PerformAttack(punchDamage);
    }

    public void OnKick(InputAction.CallbackContext context)
    {
        if (context.started && CanAttack()) StartKick();
    }

    private void StartKick()
    {
        isKicking = true;
        elapsedKickTime = 0f;
        TriggerCharacterSpecificAnimation("Kick");
        PerformAttack(kickDamage);
    }

    private bool CanAttack() => !isDead && !isPunching && !isKicking && !isRunning;

    private void PerformAttack(float damage)
    {
        Vector3 attackDirection = isFacingLeft ? Vector3.left : Vector3.right;
        RaycastHit[] hits = Physics.RaycastAll(transform.position, attackDirection, attackRange);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == gameObject) continue;

            HealthBar targetHealth = hit.collider.GetComponent<HealthBar>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
                break;
            }
        }
    }

    private void HandleDeath(GameObject deadCharacter)
    {
        if (isDead) return;

        isDead = true;
        DisableMovement();
        characterAnimator.SetTrigger(deathTriggerAnimHash);
        ShowDeathCanvas();
        Invoke(nameof(DisableScript), deathDisableDelay);
    }

    private void DisableMovement()
    {
        walkSpeed = 0f;
        currentRunSpeed = 0f;
        moveInput = Vector2.zero;
        characterController.enabled = false;
    }

    private void ShowDeathCanvas()
    {
        Instantiate(deathCanvasPrefab, Vector3.zero, Quaternion.identity);
    }

    private void DisableScript() => enabled = false;

    private void Update()
    {
        if (isDead) return;

        UpdateAttackTimers();
        if (!IsAttacking()) MoveCharacter();
        MaintainGroundedHeight();
    }

    private void UpdateAttackTimers()
    {
        if (isPunching && (elapsedPunchTime += Time.deltaTime) >= punchDuration) isPunching = false;
        if (isKicking && (elapsedKickTime += Time.deltaTime) >= kickDuration) isKicking = false;
    }

    private void MoveCharacter()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, 0f);
        float currentSpeed = isRunning ? currentRunSpeed : walkSpeed;
        characterController.Move(movement * currentSpeed * Time.deltaTime);
    }

    private void MaintainGroundedHeight()
    {
        Vector3 position = transform.position;
        position.y = groundedHeight;
        transform.position = position;
    }

    private void TriggerCharacterSpecificAnimation(string animationBaseName)
    {
        if (characterAnimator == null) return;

        string triggerName = $"Trigger{animationBaseName}{(isSamurai ? "Samurai" : "Monkey")}";
        characterAnimator.SetTrigger(Animator.StringToHash(triggerName));
    }

    private bool IsAttacking() => isPunching || isKicking;
}