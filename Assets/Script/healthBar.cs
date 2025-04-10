using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Gère la barre de vie du personnage et les événements de mort
/// </summary>
public class HealthBar : MonoBehaviour
{
    [System.Serializable]
    public class DeathEvent : UnityEvent<GameObject> { }

    [Header("Health Settings")]
    [SerializeField]
    [Tooltip("Référence au Slider UI qui représente la barre de vie")]
    private Slider healthSlider;

    [SerializeField]
    [Tooltip("Points de vie maximum du personnage")]
    private float maxHealth = 100f;

    [Header("Death Settings")]
    [SerializeField]
    [Tooltip("Événement déclenché lorsque le personnage meurt")]
    private DeathEvent onDeath;

    private float currentHealth;
    private bool isDead = false;

    public DeathEvent OnDeath => onDeath;

    private void Start()
    {
        InitializeHealth();
    }

    private void Update()
    {
        UpdateHealthDisplay();
    }

    /// <summary>
    /// Initialise les points de vie au maximum
    /// </summary>
    private void InitializeHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        UpdateHealthDisplay();
    }

    /// <summary>
    /// Met à jour l'affichage de la barre de vie
    /// </summary>
    private void UpdateHealthDisplay()
    {
        if (healthSlider != null && healthSlider.value != currentHealth)
        {
            healthSlider.value = currentHealth;
        }
    }

    /// <summary>
    /// Inflige des dégâts au personnage
    /// </summary>
    /// <param name="damage">Quantité de dégâts à infliger</param>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthDisplay();

        if (currentHealth <= 0 && !isDead)
        {
            HandleDeath();
        }
    }

    /// <summary>
    /// Gère la mort du personnage
    /// </summary>
    private void HandleDeath()
    {
        isDead = true;
        Debug.Log(gameObject.name + " est KO !");
        
        // Déclenche l'événement de mort
        onDeath.Invoke(gameObject);
    }

    /// <summary>
    /// Retourne true si le personnage est en vie
    /// </summary>
    public bool IsAlive()
    {
        return !isDead;
    }

    /// <summary>
    /// Réinitialise complètement la santé du personnage
    /// </summary>
    public void ResetHealth()
    {
        InitializeHealth();
    }
}