using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : BasePlayer
{
    void Start()
    {
        animator.applyRootMotion = false;

    }
    void Update()
    {
        base.HandleJumpBack();
        base.HandleSprint();
        base.HandleMovement();
        
        if (inputActions.Player.Jumpback.triggered)
        {
            Debug.Log("JumpBack Pressed");
        }

        // base.HandleAnimation();

    }
    // void LateUpdate()
    // {   
    //     base.HandleAnimation();
    //     if (isAiming)
    //     {
    //         Vector3 lookTarget = cameraTransform.position + cameraTransform.forward * 50f;
    //         UpperBody.LookAt(lookTarget);
    //     }
    // }
    void FixedUpdate()
    {
        
    }
}
