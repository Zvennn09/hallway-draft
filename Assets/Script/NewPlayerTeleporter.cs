using UnityEngine;

public class NewPlayerTeleporter : MonoBehaviour
{
    public enum PortalType { EndPortalForward, StartPortalBack }
    
    [Header("Portal Identity")]
    public PortalType portalType;

    [Header("Seamless Destination")]
    [Tooltip("Drag the STARTING SPAWN ANCHOR (Transform) of the NEXT level that this portal should seamlessly send the player to.")]
    public Transform targetDestination;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameLoopManager.Instance == null) return;

            // Cooldown Check: Ignore collision completely if it happens too fast
            if (!GameLoopManager.Instance.CanTeleport()) return;

            // 1. Perform the Seamless Teleport Math first
            ExecuteSeamlessWarp(other.gameObject);

            // 2. Report the action to the Game Loop Manager to update the scores/levels
            ExecuteGameFlowRouting();
        }
    }

    private void ExecuteSeamlessWarp(GameObject player)
    {
        if (targetDestination == null)
        {
            Debug.LogWarning($"[Teleporter] Target Destination is missing on {gameObject.name}!");
            return;
        }

        CharacterController playerController = player.GetComponent<CharacterController>();
        FPSController fps = player.GetComponent<FPSController>();

        if (playerController != null)
        {
            // Calculate how far off-center the player is from THIS portal's position
            Vector3 relativePosition = player.transform.position - transform.position;

            // Temporarily turn off character controller physics to allow moving coordinates directly
            playerController.enabled = false;

            // Place the player at the exact same relative offset at the target destination
            player.transform.position = targetDestination.position + relativePosition;

            // Synchronize look direction angles cleanly
            if (fps != null)
            {
                fps.SyncOrientation(targetDestination.rotation);
            }
            else
            {
                player.transform.rotation = targetDestination.rotation;
            }

            // ANTI-FLICKER FIX: Force Unity to completely update its internal physics matrix 
            // right now before rendering the next visual frame.
            Physics.SyncTransforms();

            // Turn physics back on safely
            playerController.enabled = true;
        }
    }

    private void ExecuteGameFlowRouting()
    {
        if (portalType == PortalType.EndPortalForward)
        {
            GameLoopManager.Instance.PlayerWalkedForward();
        }
        else if (portalType == PortalType.StartPortalBack)
        {
            GameLoopManager.Instance.PlayerTurnedBack();
        }
    }
}