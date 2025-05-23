using UnityEngine;

public class MinimapAreaTrigger : MonoBehaviour
{
    private MinimapAreaBounds area;

    private void Awake()
    {
        area = GetComponent<MinimapAreaBounds>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MinimapManager.Instance.SetCurrentArea(area);
        }
    }
}
