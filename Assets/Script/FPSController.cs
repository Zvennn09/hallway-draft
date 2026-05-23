using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;

    public float lookSpeed = 2f;
    public float lookXLimit = 45f;

    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public bool canMove = true;

    CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Auto-assign camera if not set in Inspector
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Re-lock cursor if user clicks back into game window
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        #region Handles Movement
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        #endregion

        #region Handles Jumping
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else if (characterController.isGrounded)
        {
            moveDirection.y = -2f;
        }
        else
        {
            moveDirection.y = movementDirectionY - (gravity * Time.deltaTime);
        }
        #endregion

        #region Handles Rotation
        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove && playerCamera != null)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
        #endregion
    }

    /// <summary>
    /// Forces the player body and camera pitch to align with a target rotation seamlessly.
    /// This removes the sudden frame twitch/snap during teleportation loops.
    /// </summary>
    public void SyncOrientation(Quaternion targetRotation)
    {
        // Overwrite the player capsule transform direction
        transform.rotation = targetRotation;
        
        // Zero out mouse look pitch tracker to align with the new hallway's flat horizontal path
        rotationX = 0f;
        
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.identity;
        }
    }

    /// <summary>
    /// Resets movement direction vectors to zero.
    /// Useful to halt unexpected velocity or physics momentum during loop changes.
    /// </summary>
    public void ResetMovement()
    {
        moveDirection = Vector3.zero;
    }

    /// <summary>
    /// Resets the camera vertical pitch to zero.
    /// </summary>
    public void ResetCameraRotation()
    {
        rotationX = 0f;
        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.identity;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}