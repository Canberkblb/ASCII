using UnityEngine;

public class LookCamera : MonoBehaviour
{
    private Camera mainCamera;
    private Canvas canvas;

    void Start()
    {
        // Ana kamerayı bul
        mainCamera = Camera.main;
        // Bu scriptin bağlı olduğu objedeki Canvas'ı al
        canvas = GetComponent<Canvas>();
    }

    void Update()
    {
        // Canvas'ın kameraya bakmasını sağla
        if (mainCamera != null && canvas != null)
        {
            transform.LookAt(mainCamera.transform);
        }
    }
}
