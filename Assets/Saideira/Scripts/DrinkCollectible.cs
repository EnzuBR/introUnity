using UnityEngine;

public class DrinkCollectible : MonoBehaviour
{
    public Transform visual;
    public SpriteRenderer sprite;
    Vector3 visualOrigin;
    void Awake() { visualOrigin = visual.localPosition; }
    void Update()
    {
        visual.localPosition = visualOrigin + Vector3.up * (Mathf.Sin(Time.time * 3.3f + transform.position.x) * 0.08f);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PartyPlayer>() != null) PartyGame.Instance.Collect(this);
    }
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.GetComponent<PartyPlayer>() != null) PartyGame.Instance.Collect(this);
    }
    public void ResetDrink() { gameObject.SetActive(true); }
}

