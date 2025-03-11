using UnityEngine;

public class Rocket : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 20f;
    public float explosionRadius = 3f;
    public float explosionForce = 30f;
    public float minExplosionForce = 5f;   // Minimum force applied by explosion
    public float maxExplosionForce = 20f;  // Maximum force applied by explosion
    public LayerMask explosionMask;
    public GameObject explosionParticlesPrefab; // Reference to the explosion particles prefab

    private Vector2 direction;
    private bool hasExploded;

    public void Initialize(Vector2 dir)
    {
        direction = dir;
    }

    void Update()
    {
        if (hasExploded) return;

        // Move using raycast checks
        float moveDistance = speed * Time.deltaTime;
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction,
            speed * Time.deltaTime,
            LayerMask.GetMask("Platform") // Only detect platforms
        );
        if (hit.collider != null && !hit.collider.CompareTag("Player"))
        {
            Explode(hit.point);
        }
        else
        {
            transform.Translate(direction * moveDistance, Space.World);
        }
    }

    void Explode(Vector2 position)
    {
        hasExploded = true;
        Debug.Log("Boom!");

        // Instantiate explosion particles
        Instantiate(explosionParticlesPrefab, position, Quaternion.identity);

        // Show explosion radius
        Debug.DrawRay(position, Vector2.up * explosionRadius, Color.red, 1f);
        Debug.DrawRay(position, Vector2.down * explosionRadius, Color.red, 1f);
        Debug.DrawRay(position, Vector2.left * explosionRadius, Color.red, 1f);
        Debug.DrawRay(position, Vector2.right * explosionRadius, Color.red, 1f);

        // Cast 36 rays in all directions
        for (int i = 0; i < 36; i++)
        {
            float angle = i * 10f;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(
                position,
                dir,
                explosionRadius,
                explosionMask
            );

            Debug.DrawRay(position, dir * explosionRadius, Color.yellow, 1f);

            if (hit.collider != null)
            {
                // Apply force to player
                PlayerController player = hit.collider.GetComponent<PlayerController>();
                if (player != null)
                {
                    float distance = Vector2.Distance(position, hit.point);
                    float force = explosionForce * (1 - distance / explosionRadius);
                    force = Mathf.Clamp(force, minExplosionForce, maxExplosionForce);
                    player.ApplyRocketForce(position, force);
                }

                // Apply force to other objects
                Rigidbody2D rb = hit.collider.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    float distance = Vector2.Distance(position, hit.point);
                    float force = explosionForce * (1 - distance / explosionRadius);
                    force = Mathf.Clamp(force, minExplosionForce, maxExplosionForce);
                    rb.AddForce(dir * force, ForceMode2D.Impulse);
                }
            }
        }

        Destroy(gameObject);
    }
}