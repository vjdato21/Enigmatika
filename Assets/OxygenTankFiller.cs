using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class OxygenTankFiller : MonoBehaviour
{
    [Header("Oxygen Fill UI")]
    public Image fillImage;                   // Fill image (inside the tank)
    public GameObject oxygenTankImage;        // Entire tank GameObject (initially disabled)
    public float fillDuration = 300f;

    [Header("Trigger Settings")]
    public GameObject oxygenCheckpointTrigger;
    public GameObject player;
    public float detectionRadius = 3f;

    [Header("Game Over Reference")]
    public GameObject gameOver;

    [Header("Learning Module Reference")]
    public GameObject learningModule;

    private float timer = 0f;
    private bool isFilling = false;
    private bool playerInRange = false;
    private bool wasGameOverActive = false;

    void Update()
    {
        // Detect game over becoming inactive (transition from active to inactive)
        bool isGameOverNow = gameOver != null && gameOver.activeInHierarchy;

        if (isGameOverNow)
        {
            isFilling = false;
            if (oxygenTankImage != null)
                oxygenTankImage.SetActive(false);

            wasGameOverActive = true; // Mark that game over was active
            return;
        }

        // Catch the moment gameOver becomes inactive
        if (wasGameOverActive && !isGameOverNow)
        {
            wasGameOverActive = false; // Reset the flag

            // If player is still in range, restart the oxygen fill
            if (playerInRange)
            {
                StartFilling();

                if (oxygenTankImage != null)
                    oxygenTankImage.SetActive(true);
            }
        }

        CheckProximity();

        if (isFilling)
        {
            timer += Time.deltaTime;
            float fillAmount = Mathf.Clamp01(timer / fillDuration);
            fillImage.fillAmount = fillAmount;

            if (fillAmount >= 1f)
                isFilling = false;
        }
    }

    private void CheckProximity()
    {
        if (playerInRange) return; // Only check proximity once

        if (oxygenCheckpointTrigger == null || player == null) return;

        Collider[] colliders = Physics.OverlapSphere(oxygenCheckpointTrigger.transform.position, detectionRadius);
        foreach (Collider col in colliders)
        {
            if (col.gameObject == player)
            {
                playerInRange = true;
                StartFilling();

                if (oxygenTankImage != null)
                    oxygenTankImage.SetActive(true);
                break;
            }
        }
    }

    public void StartFilling()
    {
        if (!isFilling)
        {
            UnityEngine.Debug.Log("Player in range. Starting oxygen fill...");
            isFilling = true;
            timer = fillImage.fillAmount * fillDuration; // Continue from where it left off
        }
    }

    public void DisableOxygenTankImage()
    {
        if (oxygenTankImage != null)
            oxygenTankImage.SetActive(false);
    }

    public void ResetTank()
    {
        timer = 0f;
        fillImage.fillAmount = 0f;
        isFilling = false;

        if (oxygenTankImage != null)
            oxygenTankImage.SetActive(false);
    }

    // Optional: shows detection area in scene view
    void OnDrawGizmosSelected()
    {
        if (oxygenCheckpointTrigger != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(oxygenCheckpointTrigger.transform.position, detectionRadius);
        }
    }
}
