using UnityEngine;

// Kinematic motion and rider transport run on the same physics clock.
[RequireComponent(typeof(BoxCollider2D))]
public sealed class TrialPlatform : MonoBehaviour
{
    [SerializeField] private Vector2 travel;
    [SerializeField, Min(0.1f)] private float seconds = 4f;
    private Vector2 origin;
    private float elapsed;
    private Rigidbody2D platform;
    private Rigidbody2D rider;

    private void Awake()
    {
        platform = gameObject.AddComponent<Rigidbody2D>();
        platform.bodyType = RigidbodyType2D.Kinematic;
        origin = platform.position;
    }

    private void FixedUpdate()
    {
        elapsed += Time.fixedDeltaTime;
        float phase = Mathf.PingPong(elapsed / seconds, 1f);
        Vector2 target = origin + travel * Mathf.SmoothStep(0f, 1f, phase);
        // Vertical transport is resolved by the kinematic collider itself.
        if (rider != null && rider.linearVelocity.y < 1f)
            rider.position += Vector2.right * (target.x - platform.position.x);
        platform.MovePosition(target);
    }

    private void OnCollisionStay2D(Collision2D hit)
    {
        if (hit.rigidbody == null || !hit.rigidbody.TryGetComponent<PlayerMovement2D>(out _))
            return;
        rider = null;
        for (int i = 0; i < hit.contactCount; i++)
            if (hit.GetContact(i).normal.y < -0.65f)
                rider = hit.rigidbody;
    }

    private void OnCollisionExit2D(Collision2D hit)
    {
        if (hit.rigidbody == rider) rider = null;
    }
}
