using UnityEngine;

public class CameraToTexture : MonoBehaviour
{
    public Camera myCamera;  // Assign the camera you want to render
    public RenderTexture renderTexture;  // Assign the render texture you created

    void Start()
    {
        // Set the camera's target texture to the render texture
        if (myCamera != null && renderTexture != null)
        {
            myCamera.targetTexture = renderTexture;
        }
    }

    void OnDisable()
    {
        // Reset the camera's target texture to null when the script is disabled
        if (myCamera != null)
        {
            myCamera.targetTexture = null;
        }
    }
}
