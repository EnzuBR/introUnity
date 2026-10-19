using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PartyGuest : MonoBehaviour
{
    public bool followsPlayer;
    public Vector2 pointA, pointB;
    public float speed = 1.6f, chaseSpeed = 2.4f, chaseRadius = 4.5f;
    public Transform visual;
    public SpriteRenderer sprite;
    public Sprite[] walkFrames = new Sprite[3];
    public LayerMask obstacles = 256;

    Rigidbody2D body;
    Vector2 destination;
    Vector3 visualOrigin;
    float difficulty = 1f, animationTime;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        if (visual != null) visualOrigin = visual.localPosition;
        destination = pointB;
    }

    void Update()
    {
        if (PartyGame.Instance == null || !PartyGame.Instance.IsPlaying) return;
        animationTime += Time.deltaTime;
        int beat = Mathf.FloorToInt(animationTime * 10f) % 4;
        SetFrame(beat == 0 ? 1 : beat == 2 ? 2 : 0);
        if (visual != null)
            visual.localPosition = visualOrigin + Vector3.up * (Mathf.Sin(Time.time * 10f) * 0.05f);
    }

    void FixedUpdate()
    {
        var game = PartyGame.Instance;
        if (game == null || !game.IsPlaying)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }
        Vector2 toPlayer = (Vector2)game.player.transform.position - body.position;
        bool chasing = followsPlayer && toPlayer.sqrMagnitude < chaseRadius * chaseRadius
            && !Physics2D.Linecast(body.position, game.player.transform.position, obstacles);
        if (!chasing && Vector2.Distance(body.position, destination) < 0.15f) ReversePatrol();
        Vector2 target = chasing ? (Vector2)game.player.transform.position : destination;
        Vector2 direction = (target - body.position).normalized;
        float step = (chasing ? chaseSpeed : speed) * difficulty * Time.fixedDeltaTime;

        if (Blocked(direction, step))
        {
            Vector2 side = new Vector2(-direction.y, direction.x);
            if (!Blocked(side, step)) direction = side;
            else if (!Blocked(-side, step)) direction = -side;
            else { direction = Vector2.zero; ReversePatrol(); }
        }
        body.MovePosition(body.position + direction * step);
        if (sprite != null && Mathf.Abs(direction.x) > 0.1f) sprite.flipX = direction.x < 0;
    }

    bool Blocked(Vector2 direction, float step)
    {
        return Physics2D.CircleCast(body.position, 0.31f, direction, step + 0.08f, obstacles);
    }

    void ReversePatrol()
    {
        destination = Vector2.Distance(destination, pointA) < 0.1f ? pointB : pointA;
    }

    void SetFrame(int index)
    {
        if (sprite != null && walkFrames != null && index < walkFrames.Length && walkFrames[index] != null)
            sprite.sprite = walkFrames[index];
    }

    void OnTriggerEnter2D(Collider2D other) { Touch(other); }
    void OnTriggerStay2D(Collider2D other) { Touch(other); }
    void Touch(Collider2D other)
    {
        if (other.GetComponent<PartyPlayer>() != null && PartyGame.Instance != null)
            PartyGame.Instance.TakeDamage();
    }

    public void Stop()
    {
        if (body != null) body.linearVelocity = Vector2.zero;
        animationTime = 0;
        SetFrame(0);
        if (visual != null) visual.localPosition = visualOrigin;
    }

    public void ResetGuest(float multiplier = 1f)
    {
        if (body == null) body = GetComponent<Rigidbody2D>();
        difficulty = Mathf.Max(0, multiplier);
        Stop();
        body.position = pointA;
        transform.position = pointA;
        destination = pointB;
        if (sprite != null) sprite.flipX = false;
    }
}