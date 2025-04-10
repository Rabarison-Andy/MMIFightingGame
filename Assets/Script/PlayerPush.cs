using UnityEngine;

/// <summary>
/// Gère la mécanique de poussée entre les personnages
/// </summary>
public class PlayerPush : MonoBehaviour
{
    // Force de poussée appliquée à l'autre personnage
    [SerializeField]
    [Tooltip("Force avec laquelle le personnage pousse les autres")]
    private float pushForce = 5f;

    private CharacterController characterController;

    private void Awake()
    {
        // Vérifie que le composant nécessaire est présent
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("CharacterController manquant sur " + gameObject.name);
            enabled = false;
            return;
        }
    }

    /// <summary>
    /// Appelé lorsque le CharacterController entre en collision avec un autre collider
    /// </summary>
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        PushOtherCharacter(hit);
    }

    /// <summary>
    /// Pousse l'autre personnage s'il possède un CharacterController
    /// </summary>
    private void PushOtherCharacter(ControllerColliderHit hit)
    {
        // Vérifie si l'objet touché a un CharacterController
        CharacterController otherCharacter = hit.collider.GetComponent<CharacterController>();
        if (otherCharacter == null) return;

        // Calcule la direction de poussée
        Vector3 pushDirection = CalculatePushDirection(hit);

        // Applique le mouvement à l'autre personnage
        ApplyPushForce(otherCharacter, pushDirection);
    }

    /// <summary>
    /// Calcule la direction dans laquelle pousser l'autre personnage
    /// </summary>
    private Vector3 CalculatePushDirection(ControllerColliderHit hit)
    {
        Vector3 direction = hit.transform.position - transform.position;
        direction.y = 0; // Garde le mouvement horizontal
        direction.Normalize();
        return direction;
    }

    /// <summary>
    /// Applique la force de poussée à l'autre personnage
    /// </summary>
    private void ApplyPushForce(CharacterController target, Vector3 direction)
    {
        target.Move(direction * pushForce * Time.deltaTime);
    }
}
