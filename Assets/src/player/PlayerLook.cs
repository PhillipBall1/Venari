using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public float mouseSensitivity = 1f;
    public Transform playerBody;
    public float smoothing = 0.05f;
    float xRotation = 0f;
    Vector2 currentMouseDelta = Vector2.zero;
    private Vector2 recoilDelta = Vector2.zero;
    private Vector2 targetRecoilDelta = Vector2.zero;
    public float recoilSmoothTime = 0.1f;
    private Vector2 recoilVelocity = Vector2.zero;
    public UI_Manager uiManager;
    public float normalFOV = 60f; // Normal field of view
    public float zoomedFOV = 40f; // Zoomed-in field of view for aiming
    public float zoomSpeed = 10f; // How quickly the zoom reaches the target FOV
    private float targetFOV;


    private void Start()
    {
        targetFOV = normalFOV;
    }
    void Update()
    {
        if (uiManager.currentUIState != UI_Manager.UIState.None) return;

        Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        targetMouseDelta *= mouseSensitivity;

        // Apply smooth recoil on top of mouse movement
        recoilDelta = Vector2.SmoothDamp(recoilDelta, targetRecoilDelta, ref recoilVelocity, recoilSmoothTime);
        targetMouseDelta += recoilDelta;

        currentMouseDelta = Vector2.Lerp(currentMouseDelta, targetMouseDelta, 1f / smoothing);
        xRotation -= currentMouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * currentMouseDelta.x);
        if (Camera.main.fieldOfView != targetFOV)
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFOV, zoomSpeed * Time.deltaTime);
        }
        // Gradually reduce target recoil back to zero
        targetRecoilDelta = Vector2.Lerp(targetRecoilDelta, Vector2.zero, smoothing * Time.deltaTime);
        DecayRecoil();
    }

    public void AddRecoil(Vector2 recoil)
    {
        targetRecoilDelta += recoil; // Set the target recoil which we will smooth damp towards
    }

    private void DecayRecoil()
    {
        targetRecoilDelta = Vector2.zero;
    }

    public void ZoomIn()
    {
        targetFOV = zoomedFOV;
    }

    public void ZoomOut()
    {
        targetFOV = normalFOV;
    }
}
