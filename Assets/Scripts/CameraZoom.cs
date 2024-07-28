using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    private float zoomSpeed = 2.0f;    // Speed of zooming
    private float moveSpeed = .5f;    // Speed of moving the camera vertically
    private float minZoom = 3.5f;      // Minimum zoom level
    private float maxZoom = 11.0f;     // Maximum zoom level

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        float scrollData = 0;

        // Mouse scroll wheel zoom
        if (Input.mousePresent)
        {
            scrollData = Input.GetAxis("Mouse ScrollWheel");
        }

        if (Input.touchSupported && Input.touchCount == 2)
        {
            GameManager.i.isZooming = true;
            // Get touch positions
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (distance) between the touches in each frame
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame
            scrollData = (prevTouchDeltaMag - touchDeltaMag) * 0.01f; // Scale down for smoother pinch zoom
        }
        else
        {
            GameManager.i.isZooming = false;
        }

        if (scrollData != 0)
        {
            // Adjust the orthographic size based on scroll data
            float newOrthographicSize = cam.orthographicSize - scrollData * zoomSpeed;
            newOrthographicSize = Mathf.Clamp(newOrthographicSize, minZoom, maxZoom);

            // Calculate the Y position change
            float yPosChange = (cam.orthographicSize - newOrthographicSize) * moveSpeed;

            // Update camera orthographic size and Y position
            cam.orthographicSize = newOrthographicSize;
            cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y + yPosChange, cam.transform.position.z);
        }
    }
}
