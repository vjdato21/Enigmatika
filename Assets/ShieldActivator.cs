using UnityEngine;

public class ShieldActivator : MonoBehaviour
{
    [Header("References")]
    public GameObject shield;          // The shield GameObject to enable/disable
    public GameObject equipUIPanel;    // The UI panel (like an inventory or equip menu)

    void Update()
    {
        if (equipUIPanel != null && shield != null)
        {
            // Enable the shield if the equip UI is active, disable if not
            shield.SetActive(equipUIPanel.activeSelf);
        }
    }
}
