using FireSafety.Scenario;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FireSafety.UI
{
    public sealed class ConsequencePanelUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ScenarioManager scenarioManager;
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Button continueButton;

        private void OnEnable()
        {
            if (scenarioManager != null)
            {
                scenarioManager.ConsequenceOpened += HandleConsequenceOpened;
                scenarioManager.ConsequenceClosed += HandleConsequenceClosed;
            }

            continueButton.onClick.AddListener(HandleContinueSelected);

            if (scenarioManager != null && scenarioManager.CurrentConsequence != null)
            {
                ShowConsequence(scenarioManager.CurrentConsequence);
            }
            else
            {
                HidePanel();
            }
        }

        private void OnDisable()
        {
            if (scenarioManager != null)
            {
                scenarioManager.ConsequenceOpened -= HandleConsequenceOpened;
                scenarioManager.ConsequenceClosed -= HandleConsequenceClosed;
            }

            continueButton.onClick.RemoveListener(HandleContinueSelected);
            HidePanel();
        }

        private void HandleConsequenceOpened(ConsequenceData consequence)
        {
            ShowConsequence(consequence);
        }

        private void HandleConsequenceClosed(ConsequenceData consequence)
        {
            HidePanel();
        }

        private void HandleContinueSelected()
        {
            if (scenarioManager != null)
            {
                scenarioManager.ContinueAfterConsequence();
            }
        }

        private void ShowConsequence(ConsequenceData consequence)
        {
            if (consequence == null)
            {
                HidePanel();
                return;
            }

            titleText.text = consequence.Title;
            descriptionText.text = consequence.Description;
            SetPanelVisible(true);
        }

        private void HidePanel()
        {
            SetPanelVisible(false);
        }

        private void SetPanelVisible(bool isVisible)
        {
            panelCanvasGroup.alpha = isVisible ? 1f : 0f;
            panelCanvasGroup.interactable = isVisible;
            panelCanvasGroup.blocksRaycasts = isVisible;
        }
    }
}
