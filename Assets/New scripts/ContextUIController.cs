using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContextUIController : MonoBehaviour
{
    [System.Serializable]
    public class ContextUI
    {
        public string contextName;
        public GameObject contextPanel;
        public Button openButton;
        public Button exitButton;

        [HideInInspector] public bool hasAppeared = false;
    }

    [SerializeField]
    private List<ContextUI> contextUIs = new List<ContextUI>();

    private void Update()
    {
        foreach (var context in contextUIs)
        {
            if (!context.hasAppeared && context.openButton != null && context.openButton.gameObject.activeInHierarchy)
            {
                context.hasAppeared = true;
            }
        }
    }

    private void Awake()
    {
        foreach (var context in contextUIs)
        {
            SetupContextUI(context);
        }
    }

    // Call this to add a new Context UI dynamically (optional use)
    public void AddContextUI(ContextUI newContext)
    {
        contextUIs.Add(newContext);
        SetupContextUI(newContext);
    }

    // Setup open and close button listeners
    private void SetupContextUI(ContextUI context)
    {
        if (context.openButton != null)
        {
            context.openButton.onClick.AddListener(() => ShowContextUI(context));
        }

        if (context.exitButton != null)
        {
            context.exitButton.onClick.AddListener(() => HideContextUI(context));
        }
    }

    private void ShowContextUI(ContextUI context)
    {
        context.contextPanel?.SetActive(true);
    }

    private void HideContextUI(ContextUI context)
    {
        context.contextPanel?.SetActive(false);
    }
}

