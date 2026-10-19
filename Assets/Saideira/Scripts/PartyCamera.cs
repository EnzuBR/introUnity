using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PartyCamera : MonoBehaviour
{
    int lastWidth, lastHeight;
    void Start() { Fit(); }
    void Update() { if (Screen.width != lastWidth || Screen.height != lastHeight) Fit(); }
    void Fit()
    {
        lastWidth = Screen.width; lastHeight = Screen.height;
        float scale = ((float)Screen.width / Mathf.Max(Screen.height, 1)) / (16f / 9f);
        GetComponent<Camera>().rect = scale < 1
            ? new Rect(0, (1 - scale) / 2, 1, scale)
            : new Rect((1 - 1 / scale) / 2, 0, 1 / scale, 1);
    }
}
