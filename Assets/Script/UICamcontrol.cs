using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Cinemachine;

public class MobileCameraInput : MonoBehaviour
{
    public float xSpeed = 100f;
    public float ySpeed = 2f;

    private CinemachineOrbitalFollow orbital;

    private float horizontalAxis;
    private float verticalAxis = 20f;

    void Start()
    {
        orbital = GetComponent<CinemachineOrbitalFollow>();
    }

    void Update()
    {
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        // Ignore UI touches
        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            return;

        // Right-side screen only
        if (touch.position.x < Screen.width / 2)
            return;

        if (touch.phase == TouchPhase.Moved)
        {
            horizontalAxis += touch.deltaPosition.x * xSpeed * Time.deltaTime;

            verticalAxis -= touch.deltaPosition.y * ySpeed * Time.deltaTime;

            verticalAxis = Mathf.Clamp(verticalAxis, -20f, 70f);

            orbital.HorizontalAxis.Value = horizontalAxis;
            orbital.VerticalAxis.Value = verticalAxis;
        }
    }
}