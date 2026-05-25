using UnityEngine;
using UnityEngine.SceneManagement;

public class NewPlayerTeleporter : MonoBehaviour
{
    [Header("Teleport Settings")]
    public NewPlayerTeleporter targetPortal;
    public float teleportCooldown = 0.1f;

    [Header("Final Portal")]
    public bool loadCreditsInstead = false;
    public string creditsSceneName = "Credit Scene";

    private float nextAllowableTeleportTime = 0f;
    [HideInInspector] public bool ignoreNextTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Portal touched by: " + other.name);

        if (!other.CompareTag("Player")) return;

        if (ignoreNextTrigger)
        {
            ignoreNextTrigger = false;
            return;
        }

        if (Time.time < nextAllowableTeleportTime) return;

        if (loadCreditsInstead)
        {
            Debug.Log("Loading Credit Scene");
            SceneManager.LoadScene(creditsSceneName);
            return;
        }

        if (targetPortal == null) return;

        targetPortal.ignoreNextTrigger = true;
        targetPortal.SetCooldown(Time.time + teleportCooldown);

        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Vector3 localPos = transform.InverseTransformPoint(other.transform.position);
        float dotProduct = Vector3.Dot(transform.forward, other.transform.forward);
        Quaternion relativeRotation = Quaternion.Inverse(transform.rotation) * other.transform.rotation;

        if (dotProduct > 0f)
        {
            localPos = new Vector3(localPos.x, localPos.y, -localPos.z);
            other.transform.position = targetPortal.transform.TransformPoint(localPos);
            other.transform.rotation = targetPortal.transform.rotation * relativeRotation;
        }
        else
        {
            localPos = new Vector3(-localPos.x, localPos.y, -localPos.z);
            other.transform.position = targetPortal.transform.TransformPoint(localPos);
            other.transform.rotation = targetPortal.transform.rotation * Quaternion.Euler(0f, 180f, 0f) * relativeRotation;
        }

        if (cc != null) cc.enabled = true;

        nextAllowableTeleportTime = Time.time + teleportCooldown;
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