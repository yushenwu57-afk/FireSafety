using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera TargetCamera { get; set; }

    private void LateUpdate()
    {
        Camera target = TargetCamera != null ? TargetCamera : Camera.main;
        if (target == null)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(transform.position - target.transform.position);
    }
}
