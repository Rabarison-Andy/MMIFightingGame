using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private UnityEvent<GameObject> onDeath;

    private float currentHealth;
    private bool isDead;
    private float lastHealthUpdate;

    public UnityEvent<GameObject> OnDeath => onDeath;

    private void Start() => InitializeHealth();

    private void Update()
    {
        if (Mathf.Abs(healthSlider.value - currentHealth) > 0.01f)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, currentHealth, Time.deltaTime * 10f);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthVisual();

        if (currentHealth <= 0)
        {
            HandleDeath();
        }
    }

    public void ResetHealth() => InitializeHealth();

    public bool IsAlive() => !isDead;

    private void InitializeHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        healthSlider.value = currentHealth;
    }

    private void HandleDeath()
    {
        isDead = true;
        onDeath.Invoke(gameObject);
    }

    private void UpdateHealthVisual()
    {
        // Immediately update slider value when taking damage
        healthSlider.value = currentHealth;
    }
}