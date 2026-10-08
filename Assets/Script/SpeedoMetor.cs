using TMPro;
using UnityEngine;

public class SpeedoMetor : MonoBehaviour
{
    [Header("Vehicle")]
    public Rigidbody vehicleRb;

    [Header("Needle")]
    public RectTransform needle;
    public float minAngle = 120f;
    public float maxAngle = -120f;
    public float maxSpeed = 200f;

    [Header("Speed Text")]
    public TMP_Text speedText;

    void Update()
    {
        if (vehicleRb == null) return;

        float speed = vehicleRb.linearVelocity.magnitude * 3.6f; // km/h

        speed = Mathf.Clamp(speed, 0, maxSpeed);

        float angle = Mathf.Lerp(
            minAngle,
            maxAngle,
            speed / maxSpeed
        );

        needle.localRotation = Quaternion.Euler(0, 0, angle);

        if (speedText != null)
        {
            speedText.text = Mathf.RoundToInt(speed).ToString();
        }
    }
}
