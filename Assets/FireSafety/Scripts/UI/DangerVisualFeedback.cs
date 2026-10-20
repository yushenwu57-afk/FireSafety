using FireSafety.Hazard;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FireSafety.UI
{
    public sealed class DangerVisualFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DangerManager dangerManager;
        [SerializeField] private TMP_Text dangerText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private RectTransform pulseTarget;

        [Header("Text Colours")]
        [SerializeField] private Color lowTextColor = Color.white;
        [SerializeField] private Color mediumTextColor = new Color(1f, 1f, 0f);
        [SerializeField] private Color highTextColor = Color.red;
        [SerializeField] private Color criticalTextColor = new Color(1f, 0.1f, 0.1f);

        [Header("Background Colours")]
        [SerializeField] private Color lowBackgroundColor = new Color(0f, 0f, 0f, 0f);
        [SerializeField] private Color mediumBackgroundColor = new Color(0.35f, 0.25f, 0f, 0.35f);
        [SerializeField] private Color highBackgroundColor = new Color(0.45f, 0.12f, 0f, 0.45f);
        [SerializeField] private Color criticalBackgroundColor = new Color(0.5f, 0f, 0f, 0.55f);

        [Header("Pulse Speeds")]
        [Min(0f)]
        [SerializeField] private float mediumPulseSpeed = 0.8f;
        [Min(0f)]
        [SerializeField] private float highPulseSpeed = 1.4f;
        [Min(0f)]
        [SerializeField] private float criticalPulseSpeed = 2f;

        [Header("Pulse Amounts")]
        [Range(0f, 0.1f)]
        [SerializeField] private float mediumPulseAmount = 0.03f;
        [Range(0f, 0.1f)]
        [SerializeField] private float highPulseAmount = 0.05f;
        [Range(0f, 0.1f)]
        [SerializeField] private float criticalPulseAmount = 0.08f;

        [Header("Transition Punch")]
        [Range(0.05f, 1f)]
        [SerializeField] private float transitionPunchDuration = 0.25f;
        [Range(0f, 0.25f)]
        [SerializeField] private float mediumTransitionPunch = 0.08f;
        [Range(0f, 0.25f)]
        [SerializeField] private float highTransitionPunch = 0.12f;
        [Range(0f, 0.25f)]
        [SerializeField] private float criticalTransitionPunch = 0.16f;

        private RectTransform activePulseTarget;
        private float currentPulseSpeed;
        private float currentPulseAmount;
        private bool isTransitionPunchActive;
        private float transitionPunchElapsed;
        private float transitionPunchAmount;
        private Color32 defaultFaceColor;

        private void Awake()
        {
            if (dangerText != null)
            {
                defaultFaceColor = dangerText.faceColor;
            }
        }

        private void OnEnable()
        {
            activePulseTarget = pulseTarget != null
                ? pulseTarget
                : dangerText != null
                    ? dangerText.rectTransform
                    : null;

            if (dangerManager != null)
            {
                dangerManager.DangerLevelChanged += HandleDangerLevelChanged;
                ApplyVisuals(dangerManager.CurrentLevel);
            }
            else
            {
                ResetAnimationState();
            }
        }

        private void OnDisable()
        {
            if (dangerManager != null)
            {
                dangerManager.DangerLevelChanged -= HandleDangerLevelChanged;
            }

            ResetAnimationState();
        }

        private void Update()
        {
            if (activePulseTarget == null)
            {
                return;
            }

            float normalPulseScale = CalculateNormalPulseScale();
            float finalScale = normalPulseScale;

            if (isTransitionPunchActive)
            {
                transitionPunchElapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(
                    transitionPunchElapsed / transitionPunchDuration);
                float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
                float punchScale = 1f + transitionPunchAmount;
                finalScale = Mathf.Lerp(punchScale, normalPulseScale, easedProgress);

                if (progress >= 1f)
                {
                    isTransitionPunchActive = false;
                    transitionPunchElapsed = 0f;
                    transitionPunchAmount = 0f;
                }
            }

            activePulseTarget.localScale = new Vector3(finalScale, finalScale, 1f);
        }

        private void HandleDangerLevelChanged(
            DangerLevel previousLevel,
            DangerLevel newLevel)
        {
            ApplyVisuals(newLevel);

            if ((int)newLevel > (int)previousLevel)
            {
                StartTransitionPunch(newLevel);
            }
        }

        private void ApplyVisuals(DangerLevel level)
        {
            ResetAnimationState();

            if (dangerText != null)
            {
                // TMP multiplies the vertex colour by the material face colour.
                // faceColor creates a local material instance, preserving other labels.
                dangerText.faceColor = level == DangerLevel.Medium || level == DangerLevel.High
                    ? new Color32(255, 255, 255, defaultFaceColor.a)
                    : defaultFaceColor;
            }

            switch (level)
            {
                case DangerLevel.Medium:
                    ApplyColours(mediumTextColor, mediumBackgroundColor);
                    currentPulseSpeed = mediumPulseSpeed;
                    currentPulseAmount = mediumPulseAmount;
                    break;
                case DangerLevel.High:
                    ApplyColours(highTextColor, highBackgroundColor);
                    currentPulseSpeed = highPulseSpeed;
                    currentPulseAmount = highPulseAmount;
                    break;
                case DangerLevel.Critical:
                    ApplyColours(criticalTextColor, criticalBackgroundColor);
                    currentPulseSpeed = criticalPulseSpeed;
                    currentPulseAmount = criticalPulseAmount;
                    break;
                default:
                    ApplyColours(lowTextColor, lowBackgroundColor);
                    break;
            }
        }

        private float CalculateNormalPulseScale()
        {
            if (currentPulseAmount <= 0f)
            {
                return 1f;
            }

            float pulse = (Mathf.Sin(Time.unscaledTime * currentPulseSpeed *
                Mathf.PI * 2f) + 1f) * 0.5f;
            return 1f + pulse * currentPulseAmount;
        }

        private void StartTransitionPunch(DangerLevel level)
        {
            switch (level)
            {
                case DangerLevel.Medium:
                    transitionPunchAmount = mediumTransitionPunch;
                    break;
                case DangerLevel.High:
                    transitionPunchAmount = highTransitionPunch;
                    break;
                case DangerLevel.Critical:
                    transitionPunchAmount = criticalTransitionPunch;
                    break;
                default:
                    transitionPunchAmount = 0f;
                    break;
            }

            if (transitionPunchDuration <= 0f || transitionPunchAmount <= 0f)
            {
                isTransitionPunchActive = false;
                return;
            }

            transitionPunchElapsed = 0f;
            isTransitionPunchActive = true;

            if (activePulseTarget != null)
            {
                float scale = 1f + transitionPunchAmount;
                activePulseTarget.localScale = new Vector3(scale, scale, 1f);
            }
        }

        private void ApplyColours(Color textColor, Color backgroundColor)
        {
            if (dangerText != null)
            {
                dangerText.color = textColor;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
            }
        }

        private void ResetAnimationState()
        {
            currentPulseSpeed = 0f;
            currentPulseAmount = 0f;
            isTransitionPunchActive = false;
            transitionPunchElapsed = 0f;
            transitionPunchAmount = 0f;

            if (activePulseTarget != null)
            {
                activePulseTarget.localScale = Vector3.one;
            }
        }
    }
}
