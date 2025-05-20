using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Diagnostics;

public class IdentifyingKeyDetailsGame : MonoBehaviour
{
    [Header("Main UI References")]
    public GameObject scrollViewContent;
    public GameObject startExaminationBG; // <-- New reference
    public Button startClueExaminationButton;
    public Text selectedAnswerText; // <-- New field for displaying selected answer
    public Text correctAnswerText; // <-- New field for displaying the correct answer
    public GameObject scoringUI; // <-- New reference for scoring UI
    public Text scoreRemarkText; // <-- New reference for score remark
    public Text scoreText; // <-- New reference for displaying score
    public Text detailsFoundText; // <-- New Details Found Text reference
    
    [Header("Events")]
    public UnityEvent notifyLearningModule; // <-- New UnityEvent

    [System.Serializable]
    public class KeyDetailEntry
    {
        public Button keyDetailButton;
        public string associatedWord;
        public string buttonAnswer; // "Correct" or "Wrong"
        public string selectedAnswer; // <-- Track what the user chose
        public string question; // <-- Question field
        public GameObject feedbackUI;
        public string wrongFeedback;
        public string correctFeedback;
        public Text feedbackText;
        public Button correctButton;
        public Button wrongButton;
        public Button exitFeedbackUIButton;
        public bool hasBeenAnswered; // <-- Boolean to track if the entry has been answered
        public GameObject highlightObject; // <-- Add this for highlighting when answered
        public string selectedCorrectAnswerFeedback; // New: feedback if selected answer is correct
        public string selectedWrongAnswerFeedback;   // New: feedback if selected answer is wrong
    }

    [Header("Key Detail Entries")]
    public List<KeyDetailEntry> keyDetails = new List<KeyDetailEntry>();

    private void Start()
    {
        if (startClueExaminationButton != null)
            startClueExaminationButton.onClick.AddListener(OnStartClueExamination);

        foreach (var entry in keyDetails)
        {
            entry.feedbackUI.SetActive(false);
            entry.keyDetailButton.onClick.AddListener(() => OnKeyDetailSelected(entry));
            entry.correctButton.onClick.AddListener(() => OnAnswerSelected(entry, "Correct"));
            entry.wrongButton.onClick.AddListener(() => OnAnswerSelected(entry, "Wrong"));
            entry.exitFeedbackUIButton.onClick.AddListener(() => {
                entry.feedbackUI.SetActive(false);
                selectedAnswerText.gameObject.SetActive(false); // Disable the GameObject
                correctAnswerText.gameObject.SetActive(false); // Disable the GameObject
            });
        }
    }

    private void OnStartClueExamination()
    {
        scrollViewContent.SetActive(true);

        // Disable background and button
        if (startExaminationBG != null)
            startExaminationBG.SetActive(false);

        if (startClueExaminationButton != null)
            startClueExaminationButton.gameObject.SetActive(false);
    }

    private void OnKeyDetailSelected(KeyDetailEntry entry)
    {
        entry.feedbackUI.SetActive(true);
        entry.feedbackText.text = entry.question; // Display the question first
        selectedAnswerText.text = ""; // Clear previous selected answer
        correctAnswerText.text = ""; // Clear correct answer
    }

    private void OnAnswerSelected(KeyDetailEntry entry, string selectedAnswer)
    {
        selectedAnswerText.gameObject.SetActive(true);
        correctAnswerText.gameObject.SetActive(true);

        // Set selected answer
        if (selectedAnswer == entry.buttonAnswer)
        {
            selectedAnswerText.text = entry.selectedCorrectAnswerFeedback;
            selectedAnswerText.color = Color.green;
        }
        else
        {
            selectedAnswerText.text = entry.selectedWrongAnswerFeedback;
            selectedAnswerText.color = Color.red;
        }
        // Set correct answer display
        correctAnswerText.text = $"Correct Answer: {entry.buttonAnswer}";

        // Color logic
        if (selectedAnswer != entry.buttonAnswer)
            selectedAnswerText.color = Color.red;
        else
            selectedAnswerText.color = Color.green;

        // Feedback message
        entry.feedbackText.text = selectedAnswer == entry.buttonAnswer ? entry.correctFeedback : entry.wrongFeedback;
        entry.selectedAnswer = selectedAnswer;

        // Disable answer buttons
        entry.correctButton.gameObject.SetActive(false);
        entry.wrongButton.gameObject.SetActive(false);

        // Mark as answered
        entry.hasBeenAnswered = true;

        // Highlight
        if (entry.highlightObject != null)
            entry.highlightObject.SetActive(true);

        // 🔍 LOGGING SECTION
        string result = selectedAnswer == entry.buttonAnswer ? "Correct" : "Wrong";
        int currentScore = 0;
        foreach (var e in keyDetails)
        {
            if (e.hasBeenAnswered && e.selectedAnswer == e.buttonAnswer)
                currentScore++;
        }

        UnityEngine.Debug.Log($"[LOG] Button Selected: {selectedAnswer} | Result: {result} | Correct Answer: {entry.buttonAnswer} | Current Score: {currentScore}/{keyDetails.Count}");

        StopAllCoroutines();
        StartCoroutine(AutoHideFeedback(entry));

        CheckAllAnswered();
    }

    private IEnumerator AutoHideFeedback(KeyDetailEntry entry)
    {
        yield return new WaitForSeconds(10f);
        entry.feedbackUI.SetActive(false);
        selectedAnswerText.text = ""; // Clear previous selected answer
        correctAnswerText.text = "";
        selectedAnswerText.gameObject.SetActive(false);
        correctAnswerText.gameObject.SetActive(false);
    }

    private void CheckAllAnswered()
    {
        // Check if all entries have been answered
        bool allAnswered = true;
        foreach (var entry in keyDetails)
        {
            if (!entry.hasBeenAnswered)
            {
                allAnswered = false;
                break;
            }
        }

        if (allAnswered)
        {
            DisplayScoreUI();
        }
    }

    private void DisplayScoreUI()
    {
        // Calculate score: Correct answers divided by total entries
        int correctAnswers = 0;
        foreach (var entry in keyDetails)
        {
            if (entry.hasBeenAnswered && entry.selectedAnswer == entry.buttonAnswer)
            {
                correctAnswers++;
            }
        }

        int totalEntries = keyDetails.Count;
        float percentage = (float)correctAnswers / totalEntries;

        // Determine score remark
        if (percentage <= 0.02f)
        {
            scoreRemarkText.text = "Try again next time Detective, I win.";
        }
        else if (percentage >= 0.75f)
        {
            scoreRemarkText.text = "Your'e very clever Detective, the code is: 5567.";
        }
        else if (percentage >= 0.50f)
        {
            scoreRemarkText.text = "Try better next time.";
        }
        else
        {
            scoreRemarkText.text = "Keep practicing!";
        }

        // Display scoring UI
        scoringUI.SetActive(true);
        scoreText.text = $"Score: {correctAnswers}/{totalEntries}";

        // Disable scroll view content and enable the background
        scrollViewContent.SetActive(false);
        startExaminationBG.SetActive(true);

        // Invoke the UnityEvent to notify listeners
        notifyLearningModule?.Invoke();
    }

    // NEW METHOD
    private void UpdateDetailsFoundText()
    {
        int foundCount = 0;
        foreach (var entry in keyDetails)
        {
            if (entry.hasBeenAnswered)
                foundCount++;
        }

        if (detailsFoundText != null)
            detailsFoundText.text = $"Details Found: {foundCount}";
    }
}

