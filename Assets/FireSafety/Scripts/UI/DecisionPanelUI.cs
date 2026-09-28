using FireSafety.Decision;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FireSafety.UI
{
    public sealed class DecisionPanelUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DecisionManager decisionManager;
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Button optionAButton;
        [SerializeField] private Button optionBButton;
        [SerializeField] private Button optionCButton;
        [SerializeField] private TMP_Text optionALabel;
        [SerializeField] private TMP_Text optionBLabel;
        [SerializeField] private TMP_Text optionCLabel;

        private void OnEnable()
        {
            if (decisionManager != null)
            {
                decisionManager.DecisionOpened += HandleDecisionOpened;
                decisionManager.DecisionClosed += HandleDecisionClosed;
            }

            optionAButton.onClick.AddListener(HandleOptionASelected);
            optionBButton.onClick.AddListener(HandleOptionBSelected);
            optionCButton.onClick.AddListener(HandleOptionCSelected);

            if (decisionManager != null && decisionManager.CurrentDecision != null)
            {
                ShowDecision(decisionManager.CurrentDecision);
            }
            else
            {
                HidePanel();
            }
        }

        private void OnDisable()
        {
            if (decisionManager != null)
            {
                decisionManager.DecisionOpened -= HandleDecisionOpened;
                decisionManager.DecisionClosed -= HandleDecisionClosed;
            }

            optionAButton.onClick.RemoveListener(HandleOptionASelected);
            optionBButton.onClick.RemoveListener(HandleOptionBSelected);
            optionCButton.onClick.RemoveListener(HandleOptionCSelected);

            HidePanel();
        }

        private void HandleDecisionOpened(DecisionData decision)
        {
            ShowDecision(decision);
        }

        private void HandleDecisionClosed(DecisionData decision)
        {
            HidePanel();
        }

        private void HandleOptionASelected()
        {
            decisionManager.SelectOption(0);
        }

        private void HandleOptionBSelected()
        {
            decisionManager.SelectOption(1);
        }

        private void HandleOptionCSelected()
        {
            decisionManager.SelectOption(2);
        }

        private void ShowDecision(DecisionData decision)
        {
            if (decision == null)
            {
                HidePanel();
                return;
            }

            titleText.text = decision.Title;
            descriptionText.text = decision.Description;
            optionALabel.text = decision.OptionAText;
            optionBLabel.text = decision.OptionBText;
            optionCLabel.text = decision.OptionCText;

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
