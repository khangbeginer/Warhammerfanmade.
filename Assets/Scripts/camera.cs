using UnityEngine;

public class camera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0.5f, 1.6f, -3.5f);
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.LookAt(target.position + Vector3.up * 1.5f); // nhìn vào đầu nhân vật
    }
}
