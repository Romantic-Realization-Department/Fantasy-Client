using System;
using TMPro;
using UnityEngine;

public sealed class EquipmentCategoryLabelUI : MonoBehaviour
{
    [Serializable]
    private struct CareerCategoryLabels
    {
        public Career career;
        public string[] labels;
    }

    [SerializeField]
    private TMP_Text[] categoryTexts;

    [SerializeField]
    private CareerCategoryLabels[] careerLabels =
    {
        new() { career = Career.Warrior, labels = new[] { "\uB2E8\uAC80", "\uC7A5\uAC80" } },
        new() { career = Career.Archer, labels = new[] { "\uB2E8\uAD81", "\uC7A5\uAD81" } },
        new()
        {
            career = Career.Wizard,
            labels = new[] { "\uC9C0\uD321\uC774", "\uB9C8\uBC95\uC11C" },
        },
    };

    private GameManager gameManager;

    private void OnEnable()
    {
        BindGameManager();
        RefreshLabels();
    }

    private void Start()
    {
        if (gameManager != null)
            return;

        BindGameManager();
        RefreshLabels();
    }

    private void OnDisable()
    {
        UnbindGameManager();
    }

    private void BindGameManager()
    {
        GameManager foundGameManager = GameManager.InstanceOrNull;
        if (foundGameManager == gameManager)
            return;

        UnbindGameManager();
        gameManager = foundGameManager;

        if (gameManager != null)
            gameManager.OnSelectedJobChanged += HandleSelectedJobChanged;
    }

    private void UnbindGameManager()
    {
        if (gameManager == null)
            return;

        gameManager.OnSelectedJobChanged -= HandleSelectedJobChanged;
        gameManager = null;
    }

    private void RefreshLabels()
    {
        if (gameManager == null)
            return;

        ApplyLabels(gameManager.SelectedJob);
    }

    private void HandleSelectedJobChanged(Career job)
    {
        ApplyLabels(job);
    }

    private void ApplyLabels(Career career)
    {
        string[] labels = GetLabels(career);
        if (labels == null)
            return;

        int count = Mathf.Min(categoryTexts.Length, labels.Length);
        for (int i = 0; i < count; i++)
        {
            if (categoryTexts[i] != null)
                categoryTexts[i].text = labels[i];
        }
    }

    private string[] GetLabels(Career career)
    {
        for (int i = 0; i < careerLabels.Length; i++)
        {
            if (careerLabels[i].career == career)
                return careerLabels[i].labels;
        }

        return null;
    }
}
