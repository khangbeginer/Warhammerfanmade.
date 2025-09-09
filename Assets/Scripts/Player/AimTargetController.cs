using UnityEngine;

public class AimTargetController : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform aimTarget;
    [SerializeField] private float defaultDistance = 20f;
    [SerializeField] private LayerMask aimLayerMask = ~0; // default everything
    [SerializeField] private float maxDistance = 100f;

    void LateUpdate()
    {
        if (playerCamera == null || aimTarget == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, aimLayerMask))
        {
            aimTarget.position = hit.point;
        }
        else
        {
            aimTarget.position = playerCamera.transform.position + playerCamera.transform.forward * defaultDistance;
        }

        // Optional: match rotation so constraints using Up/Aim axis have consistent reference
        aimTarget.rotation = Quaternion.LookRotation(playerCamera.transform.forward, Vector3.up);
    }
}
