using UnityEngine;

public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance;

    [Header("Player Reference")]
    public CharacterController playerController;

    [Header("Level Progress")]
    public int currentLevel = 0;

    [Header("Level Configuration")]
    [Tooltip("Assign the starting portal/spawn transforms for ALL of your physical hallways here in precise order.")]
    public Transform[] levelStartSpawns; 

    [Tooltip("Check this box if the corresponding level index HAS an anomaly. Keep it unchecked if the level is normal.")]
    public bool[] isAnomalyLevel;

    [Header("Teleportation Cooldown")]
    public float teleportCooldown = 0.5f;
    private float lastTeleportTime = -10f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Checks if a portal trigger can be executed right now based on the safety cooldown clock.
    /// </summary>
    public bool CanTeleport()
    {
        return Time.time >= lastTeleportTime + teleportCooldown;
    }

    /// <summary>
    /// Called when the player walks through the exit portal at the end of the hall.
    /// </summary>
    public void PlayerWalkedForward()
    {
        if (!CanTeleport()) return;

        // If this level is NOT marked as an anomaly level, walking forward is CORRECT.
        if (IsCurrentLevelNormal())
        {
            AdvanceToNextLevel();
        }
        else
        {
            // MALI GUESS: Walked forward through a corridor containing an anomaly! Reset.
            ResetToBeginning();
        }
    }

    /// <summary>
    /// Called when the player turns back and runs through the starting doorway.
    /// </summary>
    public void PlayerTurnedBack()
    {
        if (!CanTeleport()) return;

        // If this level IS marked as an anomaly level, turning back is CORRECT.
        if (!IsCurrentLevelNormal())
        {
            AdvanceToNextLevel();
        }
        else
        {
            // MALI GUESS: Panicked and turned back on a completely normal level! Reset.
            ResetToBeginning();
        }
    }

    private bool IsCurrentLevelNormal()
    {
        // Safety bounds check: if array isn't set up, default to normal
        if (isAnomalyLevel == null || currentLevel >= isAnomalyLevel.Length)
        {
            return true; 
        }
        return !isAnomalyLevel[currentLevel];
    }

    private void AdvanceToNextLevel()
    {
        currentLevel++;
        Debug.Log($"Correct Choice! Advancing to Level Index: {currentLevel}");

        if (currentLevel >= levelStartSpawns.Length)
        {
            WinGame();
            return;
        }

        TeleportPlayerToCurrentLevel();
    }

    private void ResetToBeginning()
    {
        Debug.Log("Wrong Decision! Resetting completely back to Level 0.");
        currentLevel = 0;
        TeleportPlayerToCurrentLevel();
    }

    private void TeleportPlayerToCurrentLevel()
    {
        if (currentLevel < 0 || currentLevel >= levelStartSpawns.Length || levelStartSpawns[currentLevel] == null)
        {
            Debug.LogError($"[GameLoopManager] Spawn point reference missing for Level Index {currentLevel}!");
            return;
        }

        // Set the timestamp to start the cooldown countdown
        lastTeleportTime = Time.time;
        Transform targetSpawn = levelStartSpawns[currentLevel];

        if (playerController != null)
        {
            playerController.enabled = false;
            playerController.transform.position = targetSpawn.position;
            
            FPSController fps = playerController.GetComponent<FPSController>();
            if (fps != null)
            {
                fps.ResetMovement();
                fps.SyncOrientation(targetSpawn.rotation);
            }
            else
            {
                playerController.transform.rotation = targetSpawn.rotation;
            }
            
            playerController.enabled = true;
        }
    }

    private void WinGame()
    {
        Debug.Log("Congratulations! You cleanly beat all anomaly stages!");
        currentLevel = 0; 
        TeleportPlayerToCurrentLevel();
    }
}