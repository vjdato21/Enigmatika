using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

public class ButtonController : MonoBehaviour
{
    public GameObject[] buttons; // Array of button GameObjects
    public Animator[] animators; // Array of button animator controllers
    public AudioSource[] buttonSounds; // Audio sources for each button
    public Material litMaterial; // Material for lit-up button (white)
    public SpikeTrap spikeTrap; // Reference to the SpikeTrap script
    private bool[] buttonStates; // Track the current state (true = brown/unlit, false = white/lit)
    private Material[] originalMaterials; // Store the original materials for each button
    private bool hasInteracted; // Track if any button has been interacted with
    private int[] pressCount; // Track the number of presses for each button
    private int pressThreshold = 3; // Set the threshold for button presses

    [Header("References to hint system")]
    public HintPointManager hintPointManager;
    public HintUIManager hintUIManager;
    public HintMessageUIManager hintMessageUIManager; // Reference to your hint message UI manager

    [Header("Puzzle Solved Event")]
    public UnityEvent onPuzzleSolved;  // Event to notify listeners


    void Start()
    {
        buttonStates = new bool[buttons.Length];
        originalMaterials = new Material[buttons.Length];
        hasInteracted = false; // No interaction at the start
        pressCount = new int[buttons.Length]; // Initialize the press count for each button
        RegisterButtonHint(); // Register the hint when the threshold is reached

        for (int i = 0; i < buttons.Length; i++)
        {
            buttonStates[i] = true; // Initially all buttons are brown (unlit)
            originalMaterials[i] = buttons[i].GetComponent<Renderer>().material; // Store the original material
        }
    }

    void Update()
    {
        // Loop through all active touches
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            // Only check for the touch phase that started (i.e., when the touch begins)
            if (touch.phase == TouchPhase.Began)
            {
                // Convert the touch position to a ray
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    // Check if the raycast hit any of the buttons
                    for (int j = 0; j < buttons.Length; j++)
                    {
                        if (hit.collider.gameObject == buttons[j])
                        {
                            OnButtonPress(j); // Call OnButtonPress for the corresponding button
                            break;
                        }
                    }
                }
            }
        }
    }

    public void OnButtonPress(int buttonIndex)
    {
        hasInteracted = true; // Mark that interaction has occurred

        // Toggle the button's state and update its material immediately
        if (buttonStates[buttonIndex])
        {
            // If it's brown (unlit), turn it white (lit)
            buttons[buttonIndex].GetComponent<Renderer>().material = litMaterial;
            buttonStates[buttonIndex] = false;
        }
        else
        {
            // If it's white (lit), turn it brown (unlit)
            buttons[buttonIndex].GetComponent<Renderer>().material = originalMaterials[buttonIndex];
            buttonStates[buttonIndex] = true;
        }

        // Increment the press count for the button
        pressCount[buttonIndex]++;

        // Trigger animation for the button press
        animators[buttonIndex].SetTrigger("ButtonPress");

        // Play the audio for the button press
        buttonSounds[buttonIndex].Play();

        // Handle the pattern logic for each button press
        HandleButtonPattern(buttonIndex);

        // Check if the puzzle is unlocked
        CheckPuzzleUnlocked();

        // Check if all buttons have reached the threshold
        CheckPressThreshold();
    }

    private void HandleButtonPattern(int buttonIndex)
    {
        // Toggle the pressed button's state
        buttonStates[buttonIndex] = !buttonStates[buttonIndex];

        // Define the logic: Toggle adjacent buttons' states
        if (buttonIndex > 0) // Toggle the previous button if it exists
        {
            buttonStates[buttonIndex - 1] = !buttonStates[buttonIndex - 1];
        }
        if (buttonIndex < buttonStates.Length - 1) // Toggle the next button if it exists
        {
            buttonStates[buttonIndex + 1] = !buttonStates[buttonIndex + 1];
        }

        // Update the visuals after modifying the states
        UpdateButtonVisuals();
    }

    private void UpdateButtonVisuals()
    {
        // Ensure that all buttons are visually updated
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttonStates[i])
            {
                buttons[i].GetComponent<Renderer>().material = originalMaterials[i]; // Brown (unlit)
            }
            else
            {
                buttons[i].GetComponent<Renderer>().material = litMaterial; // White (lit)
            }
        }
    }

    private bool puzzleAlreadySolved = false; // Prevent multiple invocations

    private void CheckPuzzleUnlocked()
    {
        bool allLit = true;
        for (int i = 0; i < buttonStates.Length; i++)
        {
            if (buttonStates[i]) // If any button is brown (unlit)
            {
                allLit = false;
                break;
            }
        }

        if (allLit && hasInteracted)
        {
            spikeTrap.unlockSpike = true;

            if (!puzzleAlreadySolved)
            {
                puzzleAlreadySolved = true;
                onPuzzleSolved?.Invoke();  // Notify listeners
                UnityEngine.Debug.Log("Puzzle solved: all buttons are lit!");
            }
        }
        else
        {
            spikeTrap.unlockSpike = false;
        }
    }

    private void RegisterButtonHint()
    {
        string hintMessage = "Every press counts! Consider how each button affects the others when you press them.";
        hintMessageUIManager.RegisterHintMessage("ButtonPuzzle", hintMessage); // Register the hint for the 'SafePuzzle' context
    }

    // New method to check if all buttons have reached the press threshold
    private bool hintDisplayed = false; // Flag to track if the hint has already been shown

    private void CheckPressThreshold()
    {
        bool allPressedEnough = true;

        for (int i = 0; i < pressCount.Length; i++)
        {
            if (pressCount[i] < pressThreshold)
            {
                allPressedEnough = false;
                break;
            }
        }

        if (allPressedEnough && !hintDisplayed) // Check if the hint has already been shown
        {
            hintUIManager.DisplayHintButton();
            hintMessageUIManager.SetCurrentContext("ButtonPuzzle");
            UnityEngine.Debug.Log("All buttons have been pressed the required number of times!");
            hintDisplayed = true; // Set the flag to prevent further display
        }
        else if (!allPressedEnough)
        {
            UnityEngine.Debug.Log("Press more buttons!");
        }
    }
}
