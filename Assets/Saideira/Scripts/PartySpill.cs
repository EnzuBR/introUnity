using UnityEngine;

public class PartySpill : MonoBehaviour
{
    public float radius = .85f, slowMultiplier = .55f;
    public SpriteRenderer visual;
    public bool Contains(Vector2 point)
    {
        Vector2 offset = point - (Vector2)transform.position;
        offset.y /= .52f;
        return offset.sqrMagnitude < radius * radius;
    }
    void Update()
    {
        if (visual) visual.color = new Color(1, 1, 1, .85f + Mathf.Sin(Time.time * 2 + transform.position.x) * .08f);
    }
}

