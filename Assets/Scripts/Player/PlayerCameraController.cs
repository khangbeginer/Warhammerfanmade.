using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera freeLookCam; // Camera di chuyển
    [SerializeField] private CinemachineCamera aimCam;      // Camera khi aim

    [Header("Settings")]
    [SerializeField] private KeyCode aimKey = KeyCode.Mouse1; // Chuột phải để aim
    [SerializeField] private float mouseSen = 0.05f;
    [SerializeField] private float Sen = 1.5f;

    protected bool isAiming = false;

    void Start()
    {
        // Đặt FreeLook là camera mặc định
        SetActiveCamera(freeLookCam, true);
        SetActiveCamera(aimCam, false);
    }

    void Update()
    {
        HandleCameraSwitch();
    }

    private void HandleCameraSwitch()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            isAiming = true;
            SetActiveCamera(freeLookCam, false);
            SetActiveCamera(aimCam, true);
        }
        else if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            isAiming = false;
            SetActiveCamera(freeLookCam, true);
            SetActiveCamera(aimCam, false);
        }
    }

    private void SetActiveCamera(CinemachineCamera cam, bool active)
    {
        if (cam == null) return;

        // Cinemachine 3.x dùng Priority để chọn camera
        cam.Priority = active ? 20 : 0;
        var input = cam.GetComponent<CinemachineInputAxisController>();
        if (input != null)
            input.enabled = active;
    }
}
