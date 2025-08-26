using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using JetBrains.Annotations;

public abstract class BasePlayer : MonoBehaviour, Imovement, Ijumpback, Iattack, Iinterract
{
    // Components
    protected Animator animator;
    protected CharacterController characterController;
    [SerializeField] private Transform upperSpine;

    // Movement
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected float sprintSpeed = 8f;
    [SerializeField] protected float rotationSmoothTime = 0.1f;
    [SerializeField] protected float duration = 1f;
    protected float turnSmoothVelocity;
    protected float currentSpeed;

    // Input
    protected PlayerInputActions inputActions;
    protected Vector2 moveInput;
    protected bool isSprinting;
    protected bool isJumpingBack;
    protected bool isMoving;

    // Camera
    public Transform cameraTransform;

    // ---------------------- UNITY METHODS ----------------------

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        characterController = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();
        inputActions.Enable();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    protected virtual void OnEnable()
    {
        inputActions?.Enable();
    }

    protected virtual void OnDisable()
    {
        inputActions?.Disable();
    }

    protected virtual void OnDestroy()
    {
        inputActions?.Disable();
    }

    // ---------------------- HANDLE METHODS ----------------------

    public virtual void HandleSprint()
    {
        if (isMoving && isSprinting)

        {
            currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
            animator.SetBool("isSprinting", isSprinting);
        }
        else
        {
            currentSpeed = moveSpeed;
            animator.SetBool("isSprinting", false);
        }
    }

    public virtual void HandleMovement()
    {
        if (isJumpingBack) return; 
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        if (moveInput.magnitude >= 0.1f)
        {
            isSprinting = inputActions.Player.Sprint.ReadValue<float>() > 0.1f; 
            // Tính hướng di chuyển dựa trên camera
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Kết hợp input với hướng camera
            Vector3 moveDirection = cameraForward * moveInput.y + cameraRight * moveInput.x;

            // Tính góc xoay nhân vật (chỉ xoay khi có input)
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Di chuyển
            characterController.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);
            animator.SetBool("isMoving", true);
            isMoving = true;
        }
        else
        {
            animator.SetBool("isMoving", false);
            isMoving = false;
            // Tự động xoay nhân vật theo hướng camera khi không di chuyển
            float targetAngle = cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
        HandleAnimation();
    }





    public virtual void HandleJumpBack()
    {
        if (inputActions.Player.Jumpback.triggered && !isJumpingBack && !isSprinting)
        {
            isJumpingBack = true;
            moveInput = Vector2.zero; // reset input
            animator.SetTrigger("jumpBack");
            animator.SetBool("isjumpback", true);
            StartCoroutine(JumpBackMovement());
        }
        
    }

    private IEnumerator JumpBackMovement()
    {
        float duration = 1f;
        float timer = 0f;

        while (timer < duration)
        {
            characterController.Move(-transform.forward * moveSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        isJumpingBack = false;
        animator.SetBool("isjumpback", false);  
    }

    protected virtual void HandleAnimation()
    {
        // if (isJumpingBack) return;
        if (animator == null) return;

        float speedValue = moveInput.magnitude;
        animator.SetFloat("speed", speedValue);
        HandleSprint();

        // Handle aiming
        bool isAiming = inputActions.Player.Aim.ReadValue<float>() > 0.1f;
        animator.SetBool("isAiming", isAiming);
        
        // Optionally set layer weight if you want smooth transitions
        // animator.SetLayerWeight(1, isAiming ? 1f : 0f);
        // animator.SetLayerWeight(2, isSprinting ? 1f : 0f);



        // if (isJumpingBack)
        // {
        //     AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        //     if (stateInfo.IsTag("JumpBack") && stateInfo.normalizedTime >= 0.95f)
        //     {
        //         isJumpingBack = false;
        //         animator.ResetTrigger("jumpBack");
        //         animator.SetBool("isjumpback",false);
        //     }
        // }
    }

    // ---------------------- INTERFACE METHODS ----------------------

    public virtual void jumpback()
    {
        // Có thể ghi đè nếu cần
    }

    public virtual void attack()
    {
        // Ghi đè ở lớp con
    }

    public virtual void interract()
    {
        // Ghi đè ở lớp con
    }
}
