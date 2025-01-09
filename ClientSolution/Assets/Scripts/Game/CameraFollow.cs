using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Camera mainCamera;
    private Camera fovCamera;
    [SerializeField] 
    private RenderTexture fovRenderTexture;
    public Transform target;

    public float smoothTime = 0.3f;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        fovCamera = transform.GetChild(0).GetComponent<Camera>();
        UpdateRenderTextureResolution();
    }
    private RenderTexture currentRenderTexture;

    private void UpdateRenderTextureResolution()
    {
        if (fovRenderTexture != null)
        {
            // Release the current render texture to apply new dimensions
            fovRenderTexture.Release();

            // Set the render texture to match the screen size
            fovRenderTexture.width = Screen.width;
            fovRenderTexture.height = Screen.height;


            Debug.Log($"Updated rt res: {fovRenderTexture.width} x {fovRenderTexture.height}");

            // Reinitialize the render texture
            fovRenderTexture.Create();

            fovCamera.targetTexture = fovRenderTexture;
        }
        else
        {
            Debug.LogWarning("Render Texture is not assigned.");
        }
    }

    private Vector2 lastScreenSize = Vector2.zero;

    private bool ScreenHasChanged()
    {
        Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
        if (currentScreenSize != lastScreenSize)
        {
            Debug.Log($"Changed screen res: {lastScreenSize.x / lastScreenSize.y} -> {currentScreenSize.x / currentScreenSize.y}");
            lastScreenSize = currentScreenSize;
            return true;
        }
        return false;
    }


    void Update()
    {
        if (ScreenHasChanged())
        {
            UpdateRenderTextureResolution();
        }
    }

    void FixedUpdate()
    {
        if (target == null) { return; }
        Vector3 targetPosition =
               target.position + new Vector3(0, 0, -1);
        transform.position = Vector3.SmoothDamp(
               transform.position,
               targetPosition,
               ref velocity,
               smoothTime);
    }
    void LateUpdate()
    {
        fovCamera.orthographicSize = mainCamera.orthographicSize;
        fovCamera.aspect = mainCamera.aspect; // Matches the aspect ratio dynamically
    }

}
