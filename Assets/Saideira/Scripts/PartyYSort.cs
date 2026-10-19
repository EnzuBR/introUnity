using UnityEngine;

public class PartyYSort : MonoBehaviour
{
    public SpriteRenderer sprite;
    void LateUpdate() { sprite.sortingOrder = 100 - Mathf.RoundToInt(transform.position.y * 10); }
}
