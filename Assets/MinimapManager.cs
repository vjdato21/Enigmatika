using UnityEngine;
using UnityEngine.UI;

public class MinimapManager : MonoBehaviour
{
    public static MinimapManager Instance;

    [Header("Minimap Components")]
    public RectTransform minimapContent;   // The full minimap image or area
    public RectTransform playerIcon;       // Player icon to move
    public Transform playerTransform;      // Reference to the player in world

    [Header("Settings")]
    public float contentFollowSpeed = 5f;   // Speed at which minimap content follows player
    public float iconSensitivity = 0.5f;    // 0 = player icon stays centered, 1 = full movement
    public float iconMoveSmoothSpeed;  // tweak this to your liking

    private MinimapAreaBounds currentArea;
    private Vector2 targetContentPos;

    private void Awake()
    {
        Instance = this;
    }

    public void SetCurrentArea(MinimapAreaBounds newArea)
    {
        currentArea = newArea;

        if (currentArea != null)
        {
            // Start with minimap centered on area center
            Vector2 areaCenter = currentArea.uiBoundsInMinimap.center;
            minimapContent.anchoredPosition = -areaCenter;
            targetContentPos = minimapContent.anchoredPosition;
        }
    }

    private void LateUpdate()
    {
        if (currentArea == null || playerTransform == null) return;

        Vector3 playerWorldPos = playerTransform.position;
        Vector2 normalizedPos = currentArea.GetNormalizedPosition(playerWorldPos);

        // Apply sensitivity
        normalizedPos = Vector2.Lerp(new Vector2(0.5f, 0.5f), normalizedPos, iconSensitivity);

        // Calculate UI pos for player icon
        float uiX = Mathf.Lerp(currentArea.uiBoundsInMinimap.xMin, currentArea.uiBoundsInMinimap.xMax, normalizedPos.x);
        float uiY = Mathf.Lerp(currentArea.uiBoundsInMinimap.yMin, currentArea.uiBoundsInMinimap.yMax, normalizedPos.y);
        Vector2 playerIconPos = new Vector2(uiX, uiY);

        // Smooth player icon movement
        playerIcon.anchoredPosition = Vector2.Lerp(playerIcon.anchoredPosition, playerIconPos, Time.deltaTime * iconMoveSmoothSpeed);

        // Map follows the player icon immediately
        targetContentPos = -playerIcon.anchoredPosition;

        // Clamp targetContentPos if you want, e.g. within minimap image bounds (optional)

        // Smoothly move minimapContent towards target position
        minimapContent.anchoredPosition = Vector2.Lerp(minimapContent.anchoredPosition, targetContentPos, Time.deltaTime * contentFollowSpeed);
    }
}
