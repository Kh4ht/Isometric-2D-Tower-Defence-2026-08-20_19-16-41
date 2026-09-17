using KH;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;

public class CameraController : KHManagedBehaviour, IKHManagedUpdate
{
    #region FIELDS

    private const float MIN_ZOOM = 2f;

    private Camera camera;

    private bool isDragging;
    private Vector3 lastMouseWorldPos;

    // GETTERS
    private Vector2 ConstraintsSize => new(maxConstraints.x - minConstraints.x, maxConstraints.y - minConstraints.y);
    private Vector2 CameraSize => camera.GetCameraOrthographicSize();
    private bool IsCamSizeGreaterThanConstraintsSize => CameraSize.x >= ConstraintsSize.x || CameraSize.y >= ConstraintsSize.y;
    private float LerpSpeed => lerpSpeed * Time.deltaTime;

    // INSPECTOR

    [Header("Pan Settings")]
    [SerializeField] private float panSpeed = 1f; // multiplier if you want to tune feel
    [SerializeField] private Vector2 minConstraints;
    [SerializeField] private Vector2 maxConstraints;

    [SerializeField] private Vector2 sizeConstraints;

    [Space(30), Header("Zoom Settings")]
    [SerializeField] private bool canZoom = true;

    [EnableIf(nameof(canZoom))]
    [SerializeField] private float zoomSpeed = 1f;
    [EndIf]

    [Space(30), Header("Other Settings")]
    [SerializeField] private float lerpSpeed = 1f;

    [Tooltip("Move the camera to the center when you zoom to the maximum")]
    [SerializeField] private bool centerCamPosOnMaxZoom = true;

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        camera = Camera.main;
    }

    public void KHUpdate()
    {
        DragCamera();

        if (canZoom)
            ScaleCamera();

        if (IsCamSizeGreaterThanConstraintsSize)
        {
            if (centerCamPosOnMaxZoom)
                CenterCameraPos();
        }
        else
            ClampCamPos();

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 bottomLeft = new(minConstraints.x, minConstraints.y, transform.position.z);
        Vector3 topLeft = new(minConstraints.x, maxConstraints.y, transform.position.z);
        Vector3 topRight = new(maxConstraints.x, maxConstraints.y, transform.position.z);
        Vector3 bottomRight = new(maxConstraints.x, minConstraints.y, transform.position.z);

        Gizmos.DrawLine(bottomLeft, topLeft);
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
    }

    #endregion
    #region PRIVATE

    private void DragCamera()
    {
        if (IsCamSizeGreaterThanConstraintsSize)
            return;

        if (Mouse.current.rightButton.wasPressedThisFrame || Mouse.current.middleButton.wasPressedThisFrame)
        {
            isDragging = true;
            lastMouseWorldPos = Kh.GetMouseWorldPos();
        }

        if (Mouse.current.rightButton.wasReleasedThisFrame || Mouse.current.middleButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 currentMouseWorldPos = Kh.GetMouseWorldPos();
            Vector3 delta = lastMouseWorldPos - currentMouseWorldPos;

            camera.transform.position += delta * panSpeed;

            ClampCamPos();

            lastMouseWorldPos = Kh.GetMouseWorldPos();
        }
    }

    private void ScaleCamera()
    {
        float scrollInput = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Approximately(scrollInput, 0f))
            return;

        Vector3 mouseWorldPosBefore = Kh.GetMouseWorldPos();

        float newSize = camera.orthographicSize - scrollInput * zoomSpeed;
        camera.orthographicSize = Mathf.Clamp(newSize, MIN_ZOOM, maxConstraints.y);

        if (!IsCamSizeGreaterThanConstraintsSize)
        {
            Vector3 mouseWorldPosAfter = Kh.GetMouseWorldPos();
            Vector3 delta = mouseWorldPosBefore - mouseWorldPosAfter;
            camera.transform.position += delta;
        }
    }

    private void CenterCameraPos()
    {
        camera.transform.position = new Vector3(Mathf.Lerp(camera.transform.position.x, 0, LerpSpeed),
                                                Mathf.Lerp(camera.transform.position.y, 0, LerpSpeed),
                                                camera.transform.position.z);
    }

    private void ClampCamPos()
    {
        Vector2 cameraHalfSize = CameraSize / 2;

        if (camera.transform.position.x + cameraHalfSize.x > maxConstraints.x)
            camera.transform.position = new(Mathf.Lerp(camera.transform.position.x, maxConstraints.x - cameraHalfSize.x, LerpSpeed),
                                            camera.transform.position.y,
                                            camera.transform.position.z);

        else if (camera.transform.position.x - cameraHalfSize.x < minConstraints.x)
            camera.transform.position = new(Mathf.Lerp(camera.transform.position.x, minConstraints.x + cameraHalfSize.x, LerpSpeed),
                                            camera.transform.position.y,
                                            camera.transform.position.z);

        if (camera.transform.position.y + cameraHalfSize.y > maxConstraints.y)
            camera.transform.position = new(camera.transform.position.x,
                                            Mathf.Lerp(camera.transform.position.y, maxConstraints.y - cameraHalfSize.y, LerpSpeed),
                                            camera.transform.position.z);

        else if (camera.transform.position.y - cameraHalfSize.y < minConstraints.y)
            camera.transform.position = new(camera.transform.position.x,
                                            Mathf.Lerp(camera.transform.position.y, minConstraints.y + cameraHalfSize.y, LerpSpeed),
                                            camera.transform.position.z);


        // camera.transform.position = new Vector3(Mathf.Clamp(camera.transform.position.x,
        //                                                     minConstraints.x + cameraHalfSize.x,
        //                                                     maxConstraints.x - cameraHalfSize.x),
        //                                         Mathf.Clamp(camera.transform.position.y,
        //                                                     minConstraints.y + cameraHalfSize.y,
        //                                                     maxConstraints.y - cameraHalfSize.y),
        //                                         camera.transform.position.z);
    }

    #endregion
}