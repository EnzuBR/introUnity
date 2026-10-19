using UnityEngine;

public class PartyCooler : MonoBehaviour
{
    public float radius = 1.9f;
    public SpriteRenderer ring;
    public TextMesh label;
    Vector3 ringScale;
    void Awake() { if (ring) ringScale = ring.transform.localScale; }
    void Update()
    {
        var game = PartyGame.Instance;
        if (game == null) return;
        bool loaded = game.IsPlaying && game.Carrying > 0;
        if (ring)
        {
            ring.color = loaded ? new Color32(255, 210, 115, 230) : new Color32(134, 230, 196, 90);
            ring.transform.localScale = ringScale * (loaded ? 1 + Mathf.Sin(Time.time * 5) * .035f : 1);
        }
        if (label) label.text = loaded ? "ENTREGAR" : "ISOPOR";
        if (loaded) game.TryDeposit();
    }
}
