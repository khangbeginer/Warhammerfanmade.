using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using UnityEngine.InputSystem;
using JetBrains.Annotations;
// using System.Numerics;

public abstract class BasePlayer : MonoBehaviour, Imovement, Icamera, Ijumpback, Iattack, Iinterract
{
    // Components
    protected Animator animator;
    protected CharacterController characterController;
    //test camera-------------------
    [SerializeField] protected bool lockRotationToCamera = true;

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
    protected bool isMoving;

    /// JUMPBACk
    [SerializeField] protected float jumpBackDistance = 3f;
    [SerializeField] protected float jumpBackDuration = 0.3f;

    protected bool isJumpingBack = false;
    protected float jumpBackTimer;
    protected Vector3 jumpBackDirection;

    /// ROLLING
    [SerializeField] protected float RollingDis = 3f;
    [SerializeField] protected float RollingDur = 1f;
    protected bool isRolling = false;
    protected float rollingTimer;
    protected Vector3 rollingDir;

    // Camera
    public Transform cameraTransform;

    // Aiming system

    [Header("UI Crosshair")]
    public GameObject crosshairUI;  // gán từ Inspector

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
            animator.SetFloat("speed", 2f);
        }
        else
        {
            currentSpeed = moveSpeed;
            animator.SetBool("isSprinting", false);
        }
    }

    public virtual void HandleMovement()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        isMoving = moveInput.magnitude > 0.1f;

        // Đọc sprint input ngay ở đây
        isSprinting = inputActions.Player.Sprint.ReadValue<float>() > 0.1f;

        if (!isMoving)
        {
            // Không di chuyển
            animator.SetFloat("speed", 0f, 0.05f, Time.deltaTime);
            animator.SetBool("isMoving", false);
            currentSpeed = moveSpeed;
            return;
        }

        // --- TÍNH HƯỚNG DI CHUYỂN THEO CAMERA (STRAFE MOVEMENT) ---
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight   = cameraTransform.right;
        cameraForward.y = 0f;
        cameraRight.y   = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x);
        if (moveDirection.sqrMagnitude > 0.0001f)
            moveDirection.Normalize();

        // --- SPRINT / SPEED ---
        HandleSprint(); // sẽ set currentSpeed, animator isSprinting, animator.speed nếu sprint

        // --- DI CHUYỂN (KHÔNG XOAY TRANSFORM Ở ĐÂY) ---
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

        // --- ANIMATOR ---
        animator.SetBool("isMoving", true);
        // Giữ param "speed" hiện có của bạn: forward/backward/sprint
        if (moveInput.y < -0.1f)
            animator.SetFloat("speed", -1f, 0.05f, Time.deltaTime); // backward
        else if (isSprinting)
            animator.SetFloat("speed", 2f, 0.01f, Time.deltaTime);  // run
        else if (moveInput.y > 0.1f)
            animator.SetFloat("speed", 1f, 0.05f, Time.deltaTime);  // forward
        else
            animator.SetFloat("speed", 0f, 0.05f, Time.deltaTime);  // strafing in place
    }

    public virtual void HandleCameraRotation()
    {
        if (cameraTransform == null) return;

        if (lockRotationToCamera)
        {
            // Luôn xoay model theo hướng camera (chỉ Y axis)
            Vector3 lookDir = cameraTransform.forward;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude < 0.0001f) return;

            float targetAngle = Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref turnSmoothVelocity,
                rotationSmoothTime
            );
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
        else
        {
            // Nếu không lock theo camera: xoay theo hướng di chuyển (cũ)
            if (moveInput.magnitude > 0.1f)
            {
                Vector3 cameraForward = cameraTransform.forward;
                Vector3 cameraRight = cameraTransform.right;
                cameraForward.y = 0f; cameraRight.y = 0f;
                cameraForward.Normalize(); cameraRight.Normalize();

                Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x);
                if (moveDirection.sqrMagnitude > 0.0001f)
                {
                    float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                    float angle = Mathf.SmoothDampAngle(
                        transform.eulerAngles.y,
                        targetAngle,
                        ref turnSmoothVelocity,
                        rotationSmoothTime
                    );
                    transform.rotation = Quaternion.Euler(0f, angle, 0f);
                }
            }
        }
    }


    protected virtual void HandleAnimation()
    {   
        if (animator == null) return;
        float vertical = moveInput.y;
        float horizontal = moveInput.x;
        bool aimPressed = inputActions.Player.Aim.IsPressed();
        bool firePressed = inputActions.Player.Fire.IsPressed();

        Debug.Log($"[Input Check] Aim={aimPressed}, Fire={firePressed}");
        if (vertical < 0) // ấn S
        {
            animator.SetFloat("speed", -1f, 0.05f, Time.deltaTime); // backward
        }
        else if (vertical > 0 || horizontal != 0)
        {
            animator.SetFloat("speed", 1f, 0.05f, Time.deltaTime); // forward / strafe
        }
        else
        {
            animator.SetFloat("speed", 0f, 0.05f, Time.deltaTime); // idle
        }
        if (isSprinting)
        {
            animator.SetFloat("speed", 2f, 0.01f, Time.deltaTime); // run
        }
        // ------------Handle aiming-----------
        bool isAiming = inputActions.Player.Aim.ReadValue<float>() > 0.1f;
        bool isFiring = inputActions.Player.Fire.ReadValue<float>() > 0.1f;
        HandleAim(isAiming, isFiring);
    }


    // method------------handle aiming, aiming shoot, shooting------------
    public virtual void HandleAim(bool isAiming, bool isFiring)
    {
        animator.SetBool("isAiming", isAiming);
        animator.SetBool("isFiring", isFiring);

        crosshairUI.SetActive(isAiming);

        if (isAiming || isFiring)
            animator.SetLayerWeight(1, 1f);
        else
            animator.SetLayerWeight(1, 0f);

        Debug.Log($"Aiming={isAiming}, Firing={isFiring}");
    }



    // --------------------jumpback--------------------
    public virtual void JumpBack()
    {
        if (isJumpingBack) return;

        isJumpingBack = true;
        jumpBackTimer = jumpBackDuration;
        jumpBackDirection = -transform.forward * (jumpBackDistance / jumpBackDuration);
        Debug.Log(isJumpingBack);
        animator.SetTrigger("jumpBack");
        animator.SetBool("isJumpingBack", true);
    }

    protected virtual void HandleJumpBack()
    {
        if (!isJumpingBack) return;
        
        transform.Translate(jumpBackDirection * Time.deltaTime, Space.World);
        jumpBackTimer -= Time.deltaTime;
        animator.ResetTrigger("jumpBack");
        if (jumpBackTimer <= 0f)
        {
            isJumpingBack = false;
            animator.SetBool("isJumpingBack", false);
        }
    }
    //-----------------Roll_----------------------------
    
    public virtual void Roll()
    {
        if (isRolling) return;

        isRolling = true;
        jumpBackTimer = jumpBackDuration;
        jumpBackDirection = transform.forward * (jumpBackDistance / jumpBackDuration);
        Debug.Log(isRolling);
        animator.SetTrigger("isRolling");
        animator.SetBool("isRolling", true);
    }


    public virtual void HandleRoll()
    {
        if (!isRolling) return;

        transform.Translate(jumpBackDirection * Time.deltaTime, Space.World);
        jumpBackTimer -= Time.deltaTime;
        animator.ResetTrigger("jumpBack");
        if (jumpBackTimer <= 0f)
        {
            isJumpingBack = false;
            animator.SetBool("isJumpingBack", false);
        }
    }











    // ---------------------- INTERFACE METHODS ----------------------












    public virtual void attack()
    {
        // Ghi đè ở lớp con
    }

    public virtual void interract()
    {
        // Ghi đè ở lớp con
    }

}
