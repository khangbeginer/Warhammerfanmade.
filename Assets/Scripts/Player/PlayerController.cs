using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : BasePlayer
{
    
    void Start()
    {
        animator.applyRootMotion = false;
        crosshairUI.SetActive(true);
    }
    void Update()
    {
        base.HandleSprint();
        base.HandleMovement();
        base.HandleCameraRotation();
        // jumpback
        if (Input.GetKeyDown(KeyCode.Space) & !(moveInput.y >= 0))
        {
            JumpBack(); // Gọi method trong BasePlayer
        }
        HandleJumpBack();
        HandleRoll();
        HandleAnimation();
        // HandleCameraZoom();

    }
    void FixedUpdate()
    {
        
    }
}
