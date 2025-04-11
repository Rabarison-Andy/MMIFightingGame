using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    [SerializeField]
    private float pushForce = 5f;

    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("CharacterController manquant sur " + gameObject.name);
            enabled = false;
            return;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        PushOtherCharacter(hit);
    }

    private void PushOtherCharacter(ControllerColliderHit hit)
    {
        CharacterController otherCharacter = hit.collider.GetComponent<CharacterController>();
        if (otherCharacter == null) return;
        Vector3 pushDirection = CalculatePushDirection(hit);
        ApplyPushForce(otherCharacter, pushDirection);
    }

    private Vector3 CalculatePushDirection(ControllerColliderHit hit)
    {
        Vector3 direction = hit.transform.position - transform.position;
        direction.y = 0;
        direction.Normalize();
        return direction;
    }

    private void ApplyPushForce(CharacterController target, Vector3 direction)
    {
        target.Move(direction * pushForce * Time.deltaTime);
    }
}
