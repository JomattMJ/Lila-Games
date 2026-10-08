using UnityEngine;

public class EnablingTheFreelookCamara : MonoBehaviour
{
    public string horizontalAxisName = "CameraX";
    public string verticalAxisName = "CameraY";

    public GameObject objectCamera;

    // Update is called once per frame
    void Update()
    {
        float x = SimpleInput.GetAxis(horizontalAxisName);
        float y = SimpleInput.GetAxis(verticalAxisName);

        if (x != 0f && y != 0f)
        {
            objectCamera.SetActive(true);
        }
       
    }
}
