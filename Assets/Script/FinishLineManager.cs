using UnityEngine;

public class FinishLineManager : MonoBehaviour
{
    // =========================================================
    // VEHICLE TAGS
    // =========================================================

    [Header("Vehicle Tags")]

    [SerializeField] private string carTag = "PlayerCar";
    [SerializeField] private string truckTag = "Truck";


    // =========================================================
    // TRAILER DETECTOR
    // =========================================================

    [Header("Trailer Detection")]

    [Tooltip("Trigger inside the trailer that detects whether the car is inside.")]
    [SerializeField] private TrailerCarDetector trailerDetector;


    // =========================================================
    // SCORE
    // =========================================================

    [Header("Score")]

    public int carScore = 0;
    public int truckScore = 0;

    [Tooltip("First player to reach this score wins.")]
    public int winningScore = 15;


    // =========================================================
    // FINISH LINE STATE
    // =========================================================

    [Header("Finish Line")]

    [Tooltip("Prevents the same truck crossing from scoring multiple times.")]
    [SerializeField] private bool truckCurrentlyInsideFinish = false;

    private bool matchFinished = false;


    // =========================================================
    // DEBUG
    // =========================================================

    [Header("Debug")]

    [SerializeField] private bool showDebug = true;


    // =========================================================
    // TRUCK ENTERS FINISH
    // =========================================================

    private void OnTriggerEnter(Collider other)
    {
        if (matchFinished)
            return;


        GameObject vehicle = GetVehicleRoot(other);

        if (vehicle == null)
            return;


        // =====================================================
        // TRUCK
        // =====================================================

        if (vehicle.CompareTag(truckTag))
        {
            // IMPORTANT:
            // Ignore additional colliders from the same truck.

            if (truckCurrentlyInsideFinish)
                return;


            truckCurrentlyInsideFinish = true;


            if (showDebug)
            {
                Debug.Log("TRUCK CROSSED FINISH LINE");
            }


            CheckTruckFinish();


            return;
        }


        // =====================================================
        // CAR
        // =====================================================

        if (vehicle.CompareTag(carTag))
        {
            // The car crossing the finish line does NOT
            // award a point.

            if (showDebug)
            {
                Debug.Log("CAR CROSSED FINISH - NO SCORE");
            }

            return;
        }
    }


    // =========================================================
    // TRUCK LEAVES FINISH
    // =========================================================

    private void OnTriggerExit(Collider other)
    {
        GameObject vehicle = GetVehicleRoot(other);

        if (vehicle == null)
            return;


        if (vehicle.CompareTag(truckTag))
        {
            truckCurrentlyInsideFinish = false;


            if (showDebug)
            {
                Debug.Log("TRUCK LEFT FINISH LINE - READY FOR NEXT CROSSING");
            }
        }
    }


    // =========================================================
    // CHECK TRUCK FINISH
    // =========================================================

    private void CheckTruckFinish()
    {
        if (matchFinished)
            return;


        if (trailerDetector == null)
        {
            Debug.LogError(
                "FinishLineManager: Trailer Car Detector is not assigned!"
            );

            return;
        }


        // =====================================================
        // CHECK CAR INSIDE TRAILER
        // =====================================================

        bool carIsInsideTrailer =
            trailerDetector.IsCarInsideTrailer;


        // =====================================================
        // CAR INSIDE TRAILER
        // =====================================================

        if (carIsInsideTrailer)
        {
            GiveCarPoint();


            if (showDebug)
            {
                Debug.Log(
                    "CAR IS INSIDE TRAILER → CAR +1"
                );
            }
        }


        // =====================================================
        // CAR NOT INSIDE TRAILER
        // =====================================================

        else
        {
            GiveTruckPoint();


            if (showDebug)
            {
                Debug.Log(
                    "CAR IS NOT INSIDE TRAILER → TRUCK +1"
                );
            }
        }
    }


    // =========================================================
    // CAR SCORE
    // =========================================================

    private void GiveCarPoint()
    {
        if (matchFinished)
            return;


        carScore++;


        Debug.Log(
            "CAR SCORED!  CAR: " +
            carScore +
            " | TRUCK: " +
            truckScore
        );


        CheckWinner();
    }


    // =========================================================
    // TRUCK SCORE
    // =========================================================

    private void GiveTruckPoint()
    {
        if (matchFinished)
            return;


        truckScore++;


        Debug.Log(
            "TRUCK SCORED!  CAR: " +
            carScore +
            " | TRUCK: " +
            truckScore
        );


        CheckWinner();
    }


    // =========================================================
    // CHECK WINNER
    // =========================================================

    private void CheckWinner()
    {
        if (carScore >= winningScore)
        {
            matchFinished = true;

            Debug.Log("CAR WINS THE MATCH!");

            return;
        }


        if (truckScore >= winningScore)
        {
            matchFinished = true;

            Debug.Log("TRUCK WINS THE MATCH!");

            return;
        }
    }


    // =========================================================
    // RESET FINISH LINE
    // =========================================================

    public void ResetFinishLine()
    {
        truckCurrentlyInsideFinish = false;
    }


    // =========================================================
    // RESET MATCH
    // =========================================================

    public void ResetMatch()
    {
        carScore = 0;
        truckScore = 0;

        matchFinished = false;

        truckCurrentlyInsideFinish = false;


        if (showDebug)
        {
            Debug.Log("MATCH RESET");
        }
    }


    // =========================================================
    // GET VEHICLE ROOT
    // =========================================================

    private GameObject GetVehicleRoot(Collider other)
    {
        Rigidbody rb =
            other.GetComponentInParent<Rigidbody>();


        if (rb != null)
        {
            return rb.gameObject;
        }


        return other.transform.root.gameObject;
    }


    // =========================================================
    // UI
    // =========================================================

    private void OnGUI()
    {
        if (!showDebug)
            return;


        GUIStyle scoreStyle =
            new GUIStyle(GUI.skin.label);

        scoreStyle.alignment =
            TextAnchor.UpperCenter;

        scoreStyle.fontSize =
            32;

        scoreStyle.fontStyle =
            FontStyle.Bold;


        string scoreText =
            "CAR  " +
            carScore +
            "   -   " +
            truckScore +
            "  TRUCK";


        GUI.Label(
            new Rect(
                0,
                20,
                Screen.width,
                50
            ),
            scoreText,
            scoreStyle
        );


        // =====================================================
        // WINNER
        // =====================================================

        if (matchFinished)
        {
            GUIStyle winnerStyle =
                new GUIStyle(GUI.skin.label);

            winnerStyle.alignment =
                TextAnchor.MiddleCenter;

            winnerStyle.fontSize =
                45;

            winnerStyle.fontStyle =
                FontStyle.Bold;


            string winnerText;


            if (carScore >= winningScore)
            {
                winnerText = "CAR WINS!";
            }
            else
            {
                winnerText = "TRUCK WINS!";
            }


            GUI.Label(
                new Rect(
                    0,
                    Screen.height / 2f - 40f,
                    Screen.width,
                    80
                ),
                winnerText,
                winnerStyle
            );
        }
    }
}