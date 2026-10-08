using Unity.Cinemachine;
using UnityEngine;

public class cinemachienButtonControlled : MonoBehaviour
{
    public string horizontalAxisName = "CameraX";
    public string verticalAxisName = "CameraY";

    public float xSpeed = 70f;
    public float ySpeed = 35f;
    public GameObject objectCamera;
    private CinemachineOrbitalFollow orbital;

    private float horizontalAxis;
    private float verticalAxis = 20f;


    private void Start()
    {
        orbital = GetComponent<CinemachineOrbitalFollow>();
    }

    private void Update()
    {
        float x = SimpleInput.GetAxis(horizontalAxisName);
        float y = SimpleInput.GetAxis(verticalAxisName);

        horizontalAxis += x * xSpeed * Time.deltaTime;
        verticalAxis -= y * ySpeed * Time.deltaTime;

        verticalAxis = Mathf.Clamp(verticalAxis, -20f, 70f);

        orbital.HorizontalAxis.Value = horizontalAxis;
        orbital.VerticalAxis.Value = verticalAxis;

    }
}
