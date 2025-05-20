using System.Diagnostics;
using UnityEngine;

public class WaterTrap : MonoBehaviour
{
    [Header("Water Settings")]
    public GameObject waterObject;
    public float targetYPosition = 5f;
    private float initialYPosition;

    [Header("Trigger Settings")]
    public GameObject triggerPoint;
    public float triggerRadius = 2f;

    [Header("Instruction Dependency")]
    public GameObject waterInstruction;
    private bool instructionShown = false;
    private bool triggerEnabled = false;

    [Header("Water Movement")]
    private bool isRising = false;
    private bool isResetting = false;
    private float riseSpeed = 1f;
    private float elapsedTime = 0f;
    public float totalRiseDuration; // 5 minutes

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip waterTrapMusic;
    public AudioClip backgroundMusic;

    [Header("UI & Reset")]
    public GameObject gameOverUI;
    public GameObject continueButton;
    public Transform resetPosition;
    public GameObject player;

    [Header("Extra Trigger Effect")]
    public GameObject escapeObject;
    private bool hasTriggered = false;
    private bool isPermanentlyDisabled = false;

    private void Start()
    {
        if (waterObject == null || triggerPoint == null || waterInstruction == null)
        {
            UnityEngine.Debug.LogWarning("Missing one or more required references.");
            return;
        }

        initialYPosition = waterObject.transform.position.y;
    }

    private void Update()
    {
        if (!triggerEnabled)
        {
            if (waterInstruction.activeSelf)
            {
                instructionShown = true;
            }
            else if (instructionShown && !waterInstruction.activeSelf)
            {
                triggerEnabled = true;
            }
        }

        if (triggerEnabled && !isRising && !(gameOverUI != null && gameOverUI.activeSelf))
        {
            CheckForTrigger();
        }

        if (isRising)
            RaiseWater();

        if (isResetting)
            ResetWater();
    }

    private void CheckForTrigger()
    {
        if (isPermanentlyDisabled) return;

        Collider[] hits = Physics.OverlapSphere(triggerPoint.transform.position, triggerRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                UnityEngine.Debug.Log("Water rising triggered by player.");
                StartWaterRise();

                if (escapeObject != null)
                    escapeObject.SetActive(false);

                if (audioSource != null)
                {
                    audioSource.Stop();
                    audioSource.clip = waterTrapMusic;
                    audioSource.Play();
                }

                break;
            }
        }
    }

    public void StartWaterRise()
    {
        if (hasTriggered || isPermanentlyDisabled || (gameOverUI != null && gameOverUI.activeSelf))
            return;

        hasTriggered = true;
        isRising = true;
        isResetting = false;
        elapsedTime = 0f;
        riseSpeed = 1f / totalRiseDuration;
    }

    public void SpeedUpWaterRise(float multiplier)
    {
        riseSpeed *= multiplier;
    }

    public void PermanentlyDisableWater()
    {
        isPermanentlyDisabled = true;
        isRising = false;
        isResetting = false;
        hasTriggered = true;

        // Immediately reset the water position
        if (waterObject != null)
        {
            UnityEngine.Debug.Log($"Resetting water Y from {waterObject.transform.position.y} to {initialYPosition}");
            Vector3 currentPos = waterObject.transform.position;
            waterObject.transform.position = new Vector3(currentPos.x, initialYPosition, currentPos.z);
        }

        // Reset player and audio as needed 
        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.Stop();
            audioSource.clip = backgroundMusic;
            audioSource.Play();
        }

        if (escapeObject != null)
            escapeObject.SetActive(true);
    }

    public void ResetWaterPosition()
    {
        isResetting = true;
        isRising = false;
        elapsedTime = 0f;
        hasTriggered = false;

        if (player != null && resetPosition != null)
        {
            player.transform.position = resetPosition.position;
        }

        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.Stop();
            audioSource.clip = backgroundMusic;
            audioSource.Play();
        }

        if (escapeObject != null)
            escapeObject.SetActive(true);
    }

    private void RaiseWater()
    {
        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime * riseSpeed);
        Vector3 currentPos = waterObject.transform.position;
        float newY = Mathf.Lerp(initialYPosition, targetYPosition, t);
        waterObject.transform.position = new Vector3(currentPos.x, newY, currentPos.z);

        if (Mathf.Approximately(newY, targetYPosition))
        {
            isRising = false;

            if (gameOverUI != null)
                gameOverUI.SetActive(true);
        }
    }

    private void ResetWater()
    {
        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / totalRiseDuration);
        Vector3 currentPos = waterObject.transform.position;
        float newY = Mathf.Lerp(currentPos.y, initialYPosition, t);
        waterObject.transform.position = new Vector3(currentPos.x, newY, currentPos.z);

        if (Mathf.Approximately(newY, initialYPosition))
            isResetting = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (triggerPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(triggerPoint.transform.position, triggerRadius);
        }
    }
}
