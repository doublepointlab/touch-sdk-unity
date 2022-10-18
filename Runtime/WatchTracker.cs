using UnityEngine;

public class WatchTracker : MonoBehaviour
{
    [SerializeField] GameObject hand;
    [SerializeField] WatchManager manager;

    [SerializeField] Vector3 handToWristOffset;
    [SerializeField] Vector3 wristToWatchOffset;
    [SerializeField] float calibrationDistance = 0.01f;

    [SerializeField] bool physiologicalConstraints = true;

    private bool calibrated = false;
    private Quaternion watchReference = new Quaternion(0f, 0f, 0f, 1f);
    private Quaternion handReference = new Quaternion(0f, 0f, 0f, 1f);

    // Calibration helpers
    [SerializeField] GameObject handHelper;
    [SerializeField] GameObject wristHelper;
    [SerializeField] bool deleteHelpers = true;

    private float maxHandDeviation = Mathf.Sin(Mathf.PI / 12);
    private bool deactivated = false;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("watchTracker start: " + hand.transform);

        if (hand == null)
        {
            Debug.LogError("No hand object");
            deactivated = true;
        }
        if (manager == null)
        {
            Debug.LogError("No watch manager");
            deactivated = true;
        }
        wristHelper.transform.SetParent(transform);
        wristHelper.transform.SetPositionAndRotation(transform.position, transform.rotation);
        wristHelper.transform.Translate(-wristToWatchOffset);

        handHelper.transform.SetParent(hand.transform);
        handHelper.transform.SetPositionAndRotation(hand.transform.position, hand.transform.rotation);
        handHelper.transform.Translate(handToWristOffset);
    }

    private static Quaternion DecomposeSwing
    (
      Quaternion q,
      Vector3 twistAxis
    )
    {
        Vector3 r = new Vector3(q.x, q.y, q.z);

        Vector3 p = Vector3.Project(r, twistAxis);
        var twist = new Quaternion(p.x, p.y, p.z, q.w);
        twist.Normalize();
        return q * Quaternion.Inverse(twist);
    }

    void TryCalibrate()
    {
        if (Vector3.Distance(handHelper.transform.position, wristHelper.transform.position) < calibrationDistance && manager.IsConnected)
        {
            calibrated = true;
            watchReference = Quaternion.Inverse(manager.Orientation);
            // Hand reference is needed since the hand does not point forward in the calibration pose. Instead
            // the object is rotated by a lot, perhaps by some nice straight angles. This calibration does require
            // the user to keep their hand pointing forward so that the rotation axes align properly.
            handReference = Quaternion.Inverse(hand.transform.rotation);
            if (deleteHelpers)
            {
                Destroy(wristHelper);
                Destroy(handHelper);
            }
        }
    }

    void Update()
    {
        if (deactivated)
            return;
        if (!calibrated)
        {
            TryCalibrate();
            return;
        }
        if (manager.IsConnected)
        {
            var wristOrientation = manager.Orientation;
            Debug.Log("WatchTracker update: " + wristOrientation);

            if (!physiologicalConstraints)
            {
                wristOrientation = watchReference * wristOrientation;
                transform.SetPositionAndRotation(hand.transform.position, wristOrientation);
            }
            else
            {
                // Remove twist around z (should correspond to wrist pronation)
                // by finding twist along z and calculating swing.
                // Pronation needs to be in local coordinates!

                var rotToHand = hand.transform.rotation * handReference; // Tracked hand does not point to I at calibration so we need this
                var rotHandToWrist = Quaternion.Inverse(rotToHand) * watchReference * wristOrientation;
                var swing = DecomposeSwing(rotHandToWrist, new Vector3(0, 0, 1));
                swing.y = Mathf.Clamp(swing.y, -0.1f, 0.1f);
                swing.Normalize();
                transform.SetPositionAndRotation(hand.transform.position, rotToHand * swing);
            }
        }

        transform.Translate(handToWristOffset, hand.transform);
        transform.Translate(wristToWatchOffset);
    }
}
