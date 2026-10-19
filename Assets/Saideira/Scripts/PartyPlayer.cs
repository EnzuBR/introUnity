using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PartyPlayer : MonoBehaviour
{
    public float speed = 5.6f;
    public Transform visual;
    public Transform pointer;
    public SpriteRenderer sprite;
    public Sprite[] walkFrames = new Sprite[3];
    public Vector2 spawnPoint = new Vector2(0, -4);
    public float SpeedMultiplier { get; private set; } = 1f;

    Rigidbody2D body;
    Vector2 movement;
    float blinkUntil, animationTime;
    Vector3 visualOrigin, pointerOrigin;
    Transform pointerArrow;
    TextMesh pointerLabel;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        if (visual != null) visualOrigin = visual.localPosition;
        if (pointer != null)
        {
            pointerOrigin = pointer.localPosition;
            pointerArrow = pointer.Find("Seta");
            pointerLabel = pointer.GetComponentInChildren<TextMesh>();
        }
    }

    void Update()
    {
        movement = Vector2.zero;
        bool playing = PartyGame.Instance != null && PartyGame.Instance.IsPlaying;
        if (playing)
        {
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) movement.x--;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) movement.x++;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) movement.y--;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) movement.y++;
            }
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 stick = gamepad.leftStick.ReadValue();
                if (stick.sqrMagnitude > 0.04f) movement = stick;
                Vector2 pad = gamepad.dpad.ReadValue();
                if (pad.sqrMagnitude > 0) movement = pad;
            }
            movement = Vector2.ClampMagnitude(movement, 1f);
        }

        bool walking = movement.sqrMagnitude > 0.01f;
        if (walking) animationTime += Time.deltaTime;
        else animationTime = 0;
        int beat = Mathf.FloorToInt(animationTime * 10f) % 4;
        SetFrame(walking ? (beat == 0 ? 1 : beat == 2 ? 2 : 0) : 0);
        if (sprite != null)
        {
            if (Mathf.Abs(movement.x) > 0.05f) sprite.flipX = movement.x < 0;
            sprite.color = Time.time < blinkUntil && Mathf.FloorToInt(Time.time * 12f) % 2 == 0
                ? new Color(1, 1, 1, 0.25f) : Color.white;
        }
        if (visual != null)
            visual.localPosition = visualOrigin + Vector3.up * (walking ? Mathf.Sin(Time.time * 17f) * 0.05f : 0);
        UpdatePointer();
    }

    void UpdatePointer()
    {
        if (pointer == null) return;
        bool below = transform.position.y > 2.3f;
        pointer.localPosition = (below ? new Vector3(0, -0.65f, 0) : pointerOrigin)
            + Vector3.up * (Mathf.Sin(Time.time * 4f) * 0.05f);
        if (pointerArrow != null)
            pointerArrow.localRotation = Quaternion.Euler(0, 0, below ? 180f : 0);
        if (pointerLabel != null)
        {
            Vector3 labelPosition = pointerLabel.transform.localPosition;
            labelPosition.y = below ? -0.42f : 0.42f;
            pointerLabel.transform.localPosition = labelPosition;
        }
    }
    void FixedUpdate()
    {
        var game = PartyGame.Instance;
        SpeedMultiplier = 1f;
        if (game == null || !game.IsPlaying)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }
        if (game.spills != null)
        {
            foreach (var spill in game.spills)
                if (spill != null && spill.gameObject.activeSelf && spill.Contains(body.position))
                    SpeedMultiplier = Mathf.Min(SpeedMultiplier, spill.slowMultiplier);
        }
        body.linearVelocity = movement * speed * SpeedMultiplier;
    }

    void SetFrame(int index)
    {
        if (sprite != null && walkFrames != null && index < walkFrames.Length && walkFrames[index] != null)
            sprite.sprite = walkFrames[index];
    }

    public void Stop()
    {
        movement = Vector2.zero;
        animationTime = 0;
        if (body != null) body.linearVelocity = Vector2.zero;
        SetFrame(0);
        if (visual != null) visual.localPosition = visualOrigin;
    }

    public void BlinkFor(float seconds) { blinkUntil = Time.time + seconds; }

    public void ResetPlayer()
    {
        if (body == null) body = GetComponent<Rigidbody2D>();
        Stop();
        body.position = spawnPoint;
        transform.position = spawnPoint;
        SpeedMultiplier = 1f;
        blinkUntil = 0;
        if (sprite != null) { sprite.color = Color.white; sprite.flipX = false; }
        UpdatePointer();
    }
}