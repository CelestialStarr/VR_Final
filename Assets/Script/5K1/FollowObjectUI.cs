using UnityEngine;

public class FollowObjectUI : MonoBehaviour
{
    public Transform targetObject; 
    public Vector3 offset = new Vector3(0, 0.2f, 0); 

    void LateUpdate()
    {
        if (targetObject == null) return;

        transform.position = targetObject.position + offset;

        Transform cam = Camera.main.transform;
        Vector3 direction = transform.position - cam.position;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
}
