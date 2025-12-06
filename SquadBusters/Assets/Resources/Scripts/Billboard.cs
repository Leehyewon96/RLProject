using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera cam = null;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void FixedUpdate()
    {
        transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);
    }
}
