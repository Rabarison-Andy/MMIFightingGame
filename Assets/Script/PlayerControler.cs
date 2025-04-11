// Remplace TOUT le contenu actuel de PlayerController.cs par ce code :
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(HealthBar))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float runSpeedMultiplier = 3.75f;
    [SerializeField] private bool initialFacingLeft = false;

    [Header("Combat Settings")]
    [SerializeField] private float punchDamage = 10f;
    [SerializeField] private float kickDamage = 20f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float punchDuration = 0.5f;
    [SerializeField] private float kickDuration = 0.7f;

    [Header("UI Settings")]
    [SerializeField] private GameObject deathCanvasPrefab;
    [SerializeField] private float deathDisableDelay = 0.5f;

    // Components
    private CharacterController characterController;
    private AnimatorManager animatorManager;
    private Animator characterAnimator;
    private HealthBar healthBar;

    // State
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

    // Animation Hashes
    private int isRunningAnimHash;
    private int deathTriggerAnimHash;
    private int deathBoolAnimHash;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        healthBar = GetComponent<HealthBar>();
        animatorManager = GetComponentInChildren<AnimatorManager>();
        characterAnimator = animatorManager?.GetComponent<Animator>();

        ValidateComponents();
        InitializeAnimatorHashes();
    }

    private IEnumerator Start()
{
    currentRunSpeed = walkSpeed * runSpeedMultiplier;
    isFacingLeft = initialFacingLeft;
    UpdateCharacterRotation();

    enabled = false; // désactive les updates jusqu’à setup terminé

    yield return new WaitUntil(() => animatorManager != null && animatorManager.IsReady);
    DetectCharacterType();

    enabled = true; // reactive après setup

    healthBar.OnDeath.AddListener(HandleDeath);
}


    private void ValidateComponents()
    {
        if (characterController == null || characterAnimator == null || healthBar == null || animatorManager == null)
        {
            Debug.LogError($"Missing components on {gameObject.name}!");
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
        isSamurai = animatorManager.CurrentCharacterType == "samurai";
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

        if (context.performed)
            StartRunning();
        else if (context.canceled)
            StopRunning();
    }

    private void StartRunning()
{
    if (!animatorManager.IsReady || characterAnimator.runtimeAnimatorController == null) return;

    isRunning = true;
    characterAnimator.SetBool(isRunningAnimHash, true);

    if (moveInput.x != 0 && !IsAttacking())
        TriggerCharacterSpecificAnimation("ChasedStepAndRun");
    else
        TriggerCharacterSpecificAnimation("Run");
}

    private void StopRunning()
{
    isRunning = false;

    if (characterAnimator == null || characterAnimator.runtimeAnimatorController == null) return;

    characterAnimator.SetBool(isRunningAnimHash, false);
}

    private void UpdateFacingDirection()
    {
        if (moveInput.x != 0)
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

    private void TriggerCharacterSpecificAnimation(string animationBaseName)
    {
        if (!animatorManager.IsReady || characterAnimator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"Animation system not ready for: {animationBaseName}");
            return;
        }

        string triggerName = $"Trigger{animationBaseName}{(isSamurai ? "Samurai" : "Monkey")}";
        characterAnimator.SetTrigger(Animator.StringToHash(triggerName));
    }

    private void TriggerMovementAnimation()
    {
        if (!animatorManager.IsReady || characterAnimator.runtimeAnimatorController == null) return;

        if (moveInput.x != 0 && !isRunning && !IsAttacking())
        {
            TriggerCharacterSpecificAnimation("ChasedStep");
        }
    }

    private void TriggerCombinedAnimation(string action)
    {
        if (!animatorManager.IsReady || characterAnimator.runtimeAnimatorController == null)
            return;

        string prefix = "";

        if (isRunning)
            prefix = "RunAnd";
        else if (moveInput.x != 0)
            prefix = "ChasedStepAnd";

        string finalTriggerName = $"Trigger{prefix}{action}{(isSamurai ? "Samurai" : "Monkey")}";
        characterAnimator.SetTrigger(Animator.StringToHash(finalTriggerName));
    }

    public void OnPunch(InputAction.CallbackContext context)
    {
        if (context.started && CanAttack()) StartPunch();
    }

    private void StartPunch()
    {
        isPunching = true;
        elapsedPunchTime = 0f;

        if (isRunning || moveInput.x != 0)
            TriggerCombinedAnimation("Punch");
        else
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

        if (isRunning || moveInput.x != 0)
            TriggerCombinedAnimation("Kick");
        else
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

            if (hit.collider.TryGetComponent<HealthBar>(out var targetHealth))
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
        if (deathCanvasPrefab != null)
            Instantiate(deathCanvasPrefab, Vector3.zero, Quaternion.identity);
    }

    private void DisableScript() => enabled = false;

    private void Update()
    {
        if (isDead) return;

        UpdateAttackTimers();
        if (!IsAttacking()) MoveCharacter();
    }

    private void UpdateAttackTimers()
    {
        if (isPunching) elapsedPunchTime += Time.deltaTime;
        if (isKicking) elapsedKickTime += Time.deltaTime;

        if (elapsedPunchTime >= punchDuration) isPunching = false;
        if (elapsedKickTime >= kickDuration) isKicking = false;
    }

    private void MoveCharacter()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, 0f);
        float currentSpeed = isRunning ? currentRunSpeed : walkSpeed;
        characterController.Move(movement * currentSpeed * Time.deltaTime);
    }

    private bool IsAttacking() => isPunching || isKicking;
}
