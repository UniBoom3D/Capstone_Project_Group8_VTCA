using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryPredictor : MonoBehaviour
{
    private LineRenderer lr;

    [Header("Settings")]
    [Tooltip("How smooth the line is (Higher = smoother)")]
    public int resolution = 30;

    [Tooltip("How many seconds of flight to predict")]
    public float timeLimit = 4f;

    [Tooltip("Layers the line should stop at (Ground, Walls)")]
    public LayerMask collisionMask;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.enabled = false; // Hide by default
    }

    /// <summary>
    /// Calculates and draws the path based on starting position and velocity
    /// </summary>
    public void ShowTrajectory(Vector3 startPos, Vector3 velocity)
    {
        if (lr == null) return;

        lr.enabled = true;
        lr.positionCount = resolution;

        Vector3[] points = new Vector3[resolution];
        lr.SetPosition(0, startPos);

        for (int i = 0; i < resolution; i++)
        {
            float t = (float)i / (float)resolution * timeLimit;

            // 📐 PHYSICS FORMULA: pos = start + (vel * t) + (0.5 * gravity * t^2)
            Vector3 point = startPos + (velocity * t) + (0.5f * Physics.gravity * t * t);

            points[i] = point;

            // Raycast check: Stop drawing if the line hits a wall/ground
            if (i > 0)
            {
                Vector3 direction = (point - points[i - 1]).normalized;
                float dist = Vector3.Distance(points[i - 1], point);

                if (Physics.Raycast(points[i - 1], direction, out RaycastHit hit, dist, collisionMask))
                {
                    // Snap the last point to the hit location and stop
                    lr.positionCount = i + 1;
                    lr.SetPosition(i, hit.point);
                    break;
                }
            }

            lr.SetPosition(i, point);
        }
    }

    public void Hide()
    {
        if (lr != null) lr.enabled = false;
    }
}