using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Tracking")]
    public Transform player;
    public float smoothSpeed = 0.15f;
    public Vector3 offset = new Vector3(0, 0, -10);
    public float verticalFollowStrength = 2f;

    [Header("Bounds")]
    public float horizontalBound = 8f;
    public float maxCameraDrop = -5f; // Prevent camera going too low

    private float initialYPosition;

    void Start()
    {
        initialYPosition = transform.position.y;
    }

    void LateUpdate()
    {
        // Calculate target position with vertical following
        float targetY = Mathf.Lerp(
            transform.position.y, 
            player.position.y + offset.y, 
            verticalFollowStrength * Time.deltaTime
        );

        // Prevent camera going below initial position
        targetY = Mathf.Max(targetY, maxCameraDrop);

        Vector3 targetPosition = new Vector3(
            transform.position.x, // Maintain current X
            targetY,
            offset.z
        );

        // Apply smooth movement
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed
        );

        // Keep player in horizontal bounds
        Vector3 playerPos = player.position;
        playerPos.x = Mathf.Clamp(playerPos.x, -horizontalBound, horizontalBound);
        player.position = playerPos;
    }
}