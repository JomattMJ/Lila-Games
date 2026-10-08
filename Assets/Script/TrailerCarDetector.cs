using UnityEngine;



public class TrailerCarDetector : MonoBehaviour
{
    [Header("Vehicle Tag")]
    [SerializeField] private string carTag = "PlayerCar";

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    // True when the car is currently inside the trailer trigger.
    public bool IsCarInsideTrailer { get; private set; }

    // Reference to the car currently inside.
    private GameObject carInsideTrailer;


    private void OnTriggerEnter(Collider other)
    {
        GameObject vehicle = GetVehicleRoot(other);

        if (vehicle == null)
            return;

        if (!vehicle.CompareTag(carTag))
            return;

        carInsideTrailer = vehicle;
        IsCarInsideTrailer = true;

        if (showDebug)
        {
            Debug.Log("CAR IS INSIDE THE TRAILER");
        }
    }


    private void OnTriggerExit(Collider other)
    {
        GameObject vehicle = GetVehicleRoot(other);

        if (vehicle == null)
            return;

        if (!vehicle.CompareTag(carTag))
            return;

        // Make sure the car leaving is actually
        // the car we are tracking.
        if (vehicle == carInsideTrailer)
        {
            carInsideTrailer = null;
            IsCarInsideTrailer = false;

            if (showDebug)
            {
                Debug.Log("CAR LEFT THE TRAILER");
            }
        }
    }


    private GameObject GetVehicleRoot(Collider other)
    {
        Rigidbody rb = other.GetComponentInParent<Rigidbody>();

        if (rb != null)
        {
            return rb.gameObject;
        }

        return other.transform.root.gameObject;
    }
}