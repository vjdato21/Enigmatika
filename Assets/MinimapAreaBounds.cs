using System.Diagnostics;
using UnityEngine;

public class MinimapAreaBounds : MonoBehaviour
{
    public string areaName;

    [Header("Link to UI Minimap Area")]
    public RectTransform uiBoundsRect;
    [HideInInspector] public Rect uiBoundsInMinimap;

    [HideInInspector] public Vector3 worldMin;
    [HideInInspector] public Vector3 worldMax;

    private void Awake()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        Vector3 center = transform.position + col.center;
        Vector3 size = col.size;

        worldMin = center - size * 0.5f;
        worldMax = center + size * 0.5f;

        if (uiBoundsRect != null)
        {
            Vector2 anchoredPos = uiBoundsRect.anchoredPosition;
            Vector2 sizeDelta = uiBoundsRect.sizeDelta;
            uiBoundsInMinimap = new Rect(
                anchoredPos.x - sizeDelta.x / 2f,
                anchoredPos.y - sizeDelta.y / 2f,
                sizeDelta.x,
                sizeDelta.y
            );
        }
        else
        {
            UnityEngine.Debug.LogWarning($"[{name}] No UI RectTransform assigned to MinimapAreaBounds.");
        }
    }

    public Vector2 GetNormalizedPosition(Vector3 playerWorldPos)
    {
        float normX = Mathf.InverseLerp(worldMin.x, worldMax.x, playerWorldPos.x);
        float normY = Mathf.InverseLerp(worldMin.z, worldMax.z, playerWorldPos.z); // z-axis for forward

        return new Vector2(normX, normY);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (TryGetComponent(out BoxCollider col))
        {
            Vector3 center = transform.position + col.center;
            Vector3 size = col.size;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
