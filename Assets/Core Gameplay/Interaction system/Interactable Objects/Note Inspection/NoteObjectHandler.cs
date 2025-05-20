using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

public class NoteObjectHandler : MonoBehaviour
{
    [Header("Note Page")]
    public List<GameObject> notePages; // List to store pages of the note

    [Header("Trigger Event when Note is active")]
    public UnityEvent onNoteUIActive;  // UnityEvent to trigger when note UI is active
    
    private bool eventTriggered = false; // Ensure event triggers only once
    private bool isChecking = false;     // Prevent multiple coroutine starts

    private void Start()
    {
        if (notePages == null || notePages.Count == 0)
        {
            UnityEngine.Debug.LogWarning($"[{name}] No pages assigned to the NoteObjectHandler.");
            return;
        }

        // Register this note with the NoteInspectionManager
        NoteInspectionManager.Instance.RegisterNoteUI(this, notePages);
    }

    private void Update()
    {
        if (eventTriggered || isChecking) return;

        foreach (var notePage in notePages)
        {
            if (notePage.activeInHierarchy)
            {
                StartCoroutine(DelayedEventTrigger());
                break;
            }
        }
    }

    private IEnumerator DelayedEventTrigger()
    {
        isChecking = true;
        UnityEngine.Debug.Log($"[{name}] Note UI detected. Waiting 3 seconds before triggering event...");
        yield return new WaitForSeconds(1f);

        // Double-check if still active
        foreach (var notePage in notePages)
        {
            if (notePage.activeInHierarchy)
            {
                onNoteUIActive?.Invoke();
                UnityEngine.Debug.Log($"[{name}] Note event triggered.");
                eventTriggered = true;
                yield break;
            }
        }

        UnityEngine.Debug.Log($"[{name}] Note UI no longer active. Event canceled.");
        isChecking = false;
    }
}
