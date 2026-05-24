using UnityEngine;

public class NewPlayerTeleporter : MonoBehaviour
{
    [Header("Teleport Settings")]
    [Tooltip("The other portal script this portal is linked to.")]
    public NewPlayerTeleporter targetPortal; 

    [Tooltip("Cooldown time in seconds before this portal can be used again.")]
    public float teleportCooldown = 0.1f; 
    
    private float nextAllowableTeleportTime = 0f;

    [HideInInspector] public bool ignoreNextTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        if (ignoreNextTrigger)
        {
            ignoreNextTrigger = false;
            return;
        }

        if (Time.time < nextAllowableTeleportTime)
        {
            return; 
        }

        if (other.CompareTag("Player"))
        {
            if (targetPortal == null) return;

            // Signal the target portal to temporarily ignore the incoming player
            targetPortal.ignoreNextTrigger = true;
            targetPortal.SetCooldown(Time.time + teleportCooldown);

            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // --- 1. GET RELATIVE POSITION ---
            // Find exactly where the player is relative to this portal
            Vector3 localPos = transform.InverseTransformPoint(other.transform.position);

            // --- 2. DETECT DIRECTION (THE EXIT 8 TRICK) ---
            // Check if the player is walking forward or backward through the portal trigger
            // We look at the player's forward direction relative to the portal's forward direction
            float dotProduct = Vector3.Dot(transform.forward, other.transform.forward);

            if (dotProduct > 0f)
            {
                // MOVING FORWARD: Player crossed going the normal way.
                // We keep them moving forward out of the target portal.
                localPos = new Vector3(localPos.x, localPos.y, -localPos.z);
                other.transform.position = targetPortal.transform.TransformPoint(localPos);

                Quaternion relativeRotation = Quaternion.Inverse(transform.rotation) * other.transform.rotation;
                other.transform.rotation = targetPortal.transform.rotation * relativeRotation;
            }
            else
            {
                // MOVING BACKWARD: Player backed into the portal.
                // To look seamless, they must step OUT backward from the destination portal.
                localPos = new Vector3(-localPos.x, localPos.y, -localPos.z);
                other.transform.position = targetPortal.transform.TransformPoint(localPos);

                Quaternion relativeRotation = Quaternion.Inverse(transform.rotation) * other.transform.rotation;
                // Flip the orientation by 180 degrees ONLY when backing up so the camera alignment 
                // matches the visual continuity of the hallway behind them.
                other.transform.rotation = targetPortal.transform.rotation * Quaternion.Euler(0, 180f, 0) * relativeRotation;
            }

            if (cc != null) cc.enabled = true;
            
            nextAllowableTeleportTime = Time.time + teleportCooldown;
        }
    }

    public void SetCooldown(float availableTime)
    {
        nextAllowableTeleportTime = availableTime;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ignoreNextTrigger = false;
        }
    }
}