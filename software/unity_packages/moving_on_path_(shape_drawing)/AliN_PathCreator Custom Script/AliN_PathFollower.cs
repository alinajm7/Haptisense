using UnityEngine;
using PathCreation;
using System.Collections;

public class AliN_PathFollower : MonoBehaviour
{
    [Header("Path Creator")]
    public PathCreator pathCreator;
    public EndOfPathInstruction endOfPathInstruction;
    [Header("Follower Speed")]
    public float speed = 5;
    public float distanceTravelled;
    private float initialDistanceTravelled;
    [Header("Turning Behaviour")]
    public bool smoothTurnning = false;

    [Header("-----------------------------")]
    [Header("Number of Following Objects")]
    public int numberOfFollowingObjects = 1; // Including the original object
    public float distanceBetweenFollowers = 1f; // The distance shift between each follower

    private GameObject[] followers;
    private float[] followerDistances;
    private bool isFollower = false;

    [Header("-----------------------------")]
    [Header("Short Pause by angle")]
    [Tooltip("Short Stop when it rotates at a certain angle")]
    public bool shortStopOnAngleChange = false;
    public float desiredAngle = 45; // 45 degree

    [Header("-----------------------------")]
    [Header("Short Pause by distance")]
    public bool shortStopOnFixedDistance = false;
    public float desiredDistance = 5f; // The distance interval at which to apply a delay
    private float lastDelayAt = 0f; // The distance at which the last delay was applied

    [Header("-----------------------------")]
    [Header("Short Pause by Path Length Division")]
    public bool shortStopOnPathLengthDivision = false;
    public int numberOfDesiredStopInEachRound = 4; // Number of stops per round
    private float segmentLength; // Length of each segment
    private int lastSegment = -1; // Last segment where a stop was made

    [Header("-----------------------------")]
    [Header("Pause settings")]
    public bool useDynamicPause = true; // Toggle for using dynamic stop duration
    public float maxSpeed = 100f; // Maximum speed value for normalization
    [Tooltip("Power factor for adjusting the curve's linearity. 1 for linear, <1 for concave, >1 for convex.")]
    public float powerFactor = 1f; // Power factor to adjust linearity
    public float basePauseDuration = 500; // Base value for stop duration calculation
    public float minPauseDuration = 100; // Minimum stop duration in milliseconds
    public float maxPauseDuration = 1000; // Maximum stop duration in milliseconds

    private Quaternion lastRotation;
    private Quaternion currentRotation;

    float processSpeed;
    bool isDelaying = false;

    private void Awake()
    {
        initialDistanceTravelled = distanceTravelled;
    }
    void Start()
    {
        
        // Prevent followers from instantiating more followers
        if (isFollower) return;

        processSpeed = speed;
        if (pathCreator != null)
        {
            pathCreator.pathUpdated += OnPathChanged;
        }

        // Initialize followers
        followers = new GameObject[numberOfFollowingObjects - 1]; // Exclude the original object
        followerDistances = new float[numberOfFollowingObjects]; // Include the original object

        for (int i = 0; i < numberOfFollowingObjects - 1; i++)
        {
            // Instantiate follower object as a child and store the reference
            GameObject follower = Instantiate(gameObject, transform.position, transform.rotation);
            follower.transform.parent = transform.parent;

            // Set the isFollower flag for the newly instantiated object
            AliN_PathFollower followerScript = follower.GetComponent<AliN_PathFollower>();
            followerScript.isFollower = true; // Set flag to true so it won't instantiate more objects

            followers[i] = follower;
        }

    }

    void Update()
    {
        if (pathCreator == null) return;

        if (!isDelaying)
        {
            processSpeed = speed;
            UpdatePositionAndRotation();
            if (shortStopOnAngleChange) CheckForRotationDelay();
            if (shortStopOnFixedDistance) CheckForIntervalDelay();
            if (shortStopOnPathLengthDivision) CheckForPathLengthDivisionDelay();
        }

        // Update positions of following objects
      UpdateFollowerPositions();
    }

    private void UpdatePositionAndRotation()
    {
        distanceTravelled += processSpeed * Time.deltaTime;
        transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
        currentRotation = smoothTurnning ? pathCreator.path.GetSmoothRotationAtDistance(distanceTravelled, endOfPathInstruction)
                                         : pathCreator.path.GetSharpRotationAtDistance(distanceTravelled, endOfPathInstruction);
        transform.rotation = currentRotation;
  

    }

    private void CheckForRotationDelay()
    {

        float angleDifference = Quaternion.Angle(lastRotation, currentRotation);

        if (angleDifference > desiredAngle)
        {
            StartCoroutine(DelayTheFollower());
        }

        lastRotation = currentRotation;
    }

    private void CheckForIntervalDelay()
    {

        if (Mathf.Abs(distanceTravelled - lastDelayAt) >= desiredDistance)
        {
            lastDelayAt = distanceTravelled;
            StartCoroutine(DelayTheFollower());
        }
    }

    private void CheckForPathLengthDivisionDelay()
    {
        segmentLength = pathCreator.path.length / numberOfDesiredStopInEachRound;
        int currentSegment = Mathf.FloorToInt(distanceTravelled / segmentLength);

        if (currentSegment != lastSegment)
        {
            StartCoroutine(DelayTheFollower());
            lastSegment = currentSegment;
        }
    }

    private float CalculateDynamicStopDuration()
    {
        // Normalize the speed to a value between 0 and 1
        float normalizedSpeed = Mathf.Clamp(speed / maxSpeed, 0, 1);

        // Apply the power factor
        float adjustedSpeed = Mathf.Pow(normalizedSpeed, powerFactor);

        // Calculate stop duration based on the adjusted speed
        float dynamicStopDuration = (1 - adjustedSpeed) * basePauseDuration;

        // Clamp the value between the minimum and maximum durations
        return Mathf.Clamp(dynamicStopDuration, minPauseDuration, maxPauseDuration);
    }

    IEnumerator DelayTheFollower()
    {
        isDelaying = true;
        processSpeed = 0;

        float stopDurationInSeconds;
        if (useDynamicPause)
        {
            stopDurationInSeconds = CalculateDynamicStopDuration() / 1000f; // Convert to seconds
        }
        else
        {
            stopDurationInSeconds = basePauseDuration / 1000f; // Use fixed stop duration
        }

        yield return new WaitForSeconds(stopDurationInSeconds);
        // Check if GameObject is still active before resuming
        if (this.gameObject.activeInHierarchy)
        {
            isDelaying = false;
        }
    }

    void OnEnable()
    {
        distanceTravelled = initialDistanceTravelled;
        // Resume the coroutine if it was delaying when disabled
        if (isDelaying)
        {
            StartCoroutine(DelayTheFollower());
        }
    }

    void OnDisable()
    {
        // If the GameObject is disabled, stop the coroutine
        if (isDelaying)
        {
            StopCoroutine(DelayTheFollower());
            isDelaying = false;
        }
    }


    private void OnPathChanged()
    {
        distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
    }

    private void UpdateFollowerPositions()
    {
        if (followers == null) return;  // Safety check

        float leadDistance = distanceTravelled;  // The distance of the main (lead) object

        for (int i = 0; i < followers.Length; i++)
        {
            if (followers[i] == null) continue;  // Skip null followers

            AliN_PathFollower followerScript = followers[i].GetComponent<AliN_PathFollower>();
            if (followerScript == null) continue;  // Skip if component is missing

            float followerDistance = leadDistance - (i + 1) * distanceBetweenFollowers;  // Calculate the new distance for this follower
            followerScript.speed = speed;
            followerScript.distanceTravelled = followerDistance;
            followerScript.UpdatePositionAndRotation();
        }
    }


}
