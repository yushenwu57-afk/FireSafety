using FireSafety.UI;
using FireSafety.Core;
using FireSafety.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FireSafety.Scenario
{
    // Wired only in Level_2. The kitchen and its existing uGUI buttons remain in use.
    public sealed class EvacuationLevelController : MonoBehaviour
    {
        [SerializeField] private GameObject decisionPanel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Button[] optionButtons;
        [SerializeField] private Text levelText;
        [SerializeField] private Text dangerText;
        [SerializeField] private GameObject reticle;
        [SerializeField] private GameObject promptPanel;
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [FormerlySerializedAs("chineseFont")]
        [Tooltip("Optional font asset included in the build. Defaults to Unity's bundled Latin font.")]
        [SerializeField] private Font uiFont;
        [SerializeField] private Transform hazardVisualRoot;

        [Header("Movement to decision two")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private GameManager movementGameManager;
        [SerializeField] private Transform secondDecisionLocation;
        [SerializeField, Min(.1f)] private float arrivalRadius = .9f;
        [SerializeField, Min(0f)] private float arrivalHeightTolerance = .75f;

        [Header("Return to kitchen follow-up")]
        [SerializeField] private Transform kitchenFollowUpLocation;
        [SerializeField, Min(.1f)] private float kitchenArrivalRadius = 1.5f;
        [SerializeField, Min(0f)] private float kitchenArrivalHeightTolerance = .75f;

        [Header("Movement to decision three")]
        [SerializeField] private Transform thirdDecisionLocation;
        [SerializeField, Min(.1f)] private float thirdArrivalRadius = .45f;
        [SerializeField, Min(0f)] private float thirdArrivalHeightTolerance = .75f;

        private readonly EvacuationRouteGate routeGate = new EvacuationRouteGate();
        private readonly EvacuationRouteGate kitchenGate =
            new EvacuationRouteGate(EvacuationStep.ReturnFromKitchen);
        private readonly EvacuationRouteGate thirdGate =
            new EvacuationRouteGate(EvacuationStep.Outside);
        private bool IsWalking => routeGate.IsWaiting || kitchenGate.IsWaiting || thirdGate.IsWaiting;

        public EvacuationSession Session { get; } = new EvacuationSession();
        private FinalResultsUI resultsUI;
        private CanvasGroup panelGroup;
        private bool menuOpen;
        private float acceptInputAfter;

        private void Start()
        {
            if (playerTransform == null || playerMovement == null || movementGameManager == null || secondDecisionLocation == null || kitchenFollowUpLocation == null || thirdDecisionLocation == null)
            {
                Debug.LogError("Level 2 requires a player and all decision arrival locations.", this);
                enabled = false;
                return;
            }
            playerMovement.enabled = false;
            transform.localScale = Vector3.one;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            // OS fonts available in the Editor are unavailable in WebGL. Use a
            // bundled font so glyph metrics and line heights match in builds.
            if (uiFont == null)
                uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            foreach (Text label in GetComponentsInChildren<Text>(true))
            {
                label.font = uiFont;
                label.lineSpacing = 1.15f;
            }

            reticle.SetActive(false);
            // Reuse the existing prompt label as the multiline decision/result body.
            bodyText.transform.SetParent(decisionPanel.transform, false);
            promptPanel.SetActive(false);
            Layout(decisionPanel.GetComponent<RectTransform>(), Vector2.zero, new Vector2(1100, 760), false);
            Layout(titleText.rectTransform, new Vector2(0, -30), new Vector2(1020, 65));
            Layout(bodyText.rectTransform, new Vector2(0, -112), new Vector2(1000, 350));
            titleText.fontSize = 34;
            bodyText.fontSize = 28;
            bodyText.resizeTextForBestFit = false;
            bodyText.alignment = TextAnchor.UpperLeft;
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyText.verticalOverflow = VerticalWrapMode.Truncate;
            bodyText.raycastTarget = false;
            levelText.gameObject.SetActive(false);
            dangerText.text = "Fire spreading - Safe exit clear";
            levelText.fontSize = dangerText.fontSize = 28;
            dangerText.resizeTextForBestFit = true;
            dangerText.resizeTextMinSize = 18;
            dangerText.resizeTextMaxSize = 28;
            panelGroup = decisionPanel.AddComponent<CanvasGroup>();
            resultsUI = gameObject.AddComponent<FinalResultsUI>();
            menuButton.onClick.AddListener(ToggleMenu);
            resumeButton.onClick.AddListener(ToggleMenu);
            restartButton.onClick.AddListener(Replay);
            mainMenuButton.onClick.AddListener(ReturnToMenu);
            SetButtonLabel(resumeButton, "Resume");
            SetButtonLabel(restartButton, "Replay Level");
            SetButtonLabel(mainMenuButton, "Main Menu");
            menuPanel.SetActive(false);
            if (hazardVisualRoot != null)
            {
                foreach (Transform visual in hazardVisualRoot.GetComponentsInChildren<Transform>(true))
                {
                    if (!visual.name.StartsWith("Particles_Fire_02") && visual.name != "Particles_Dust_01") continue;
                    visual.gameObject.SetActive(true);
                    foreach (ParticleSystem particles in visual.GetComponentsInChildren<ParticleSystem>(true))
                        particles.Play(true);
                }
            }
            Render();
        }

        private static void Layout(RectTransform rect, Vector2 position, Vector2 size, bool top = true)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = top ? new Vector2(.5f, 1f) : new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) ToggleMenu();
            if (IsWalking && !menuOpen)
            {
                bool returningToKitchen = kitchenGate.IsWaiting;
                bool walkingToThird = thirdGate.IsWaiting;
                Transform destination = walkingToThird ? thirdDecisionLocation
                    : returningToKitchen ? kitchenFollowUpLocation : secondDecisionLocation;
                EvacuationRouteGate activeGate = walkingToThird ? thirdGate
                    : returningToKitchen ? kitchenGate : routeGate;
                Vector3 destinationPosition = destination.position;
                Vector3 offset = playerTransform.position - destinationPosition;
                if (activeGate.TryArrive(offset.x * offset.x + offset.z * offset.z, offset.y,
                    walkingToThird ? thirdArrivalRadius : returningToKitchen ? kitchenArrivalRadius : arrivalRadius,
                    walkingToThird ? thirdArrivalHeightTolerance : returningToKitchen ? kitchenArrivalHeightTolerance : arrivalHeightTolerance))
                {
                    Render();
                }
            }
            if (Time.unscaledTime >= acceptInputAfter)
            {
                foreach (Button button in optionButtons) button.interactable = !menuOpen && !IsWalking;
            }
        }

        private void Render()
        {
            if (IsWalking)
            {
                decisionPanel.SetActive(false);
                panelGroup.interactable = panelGroup.blocksRaycasts = false;
                dangerText.text = kitchenGate.IsWaiting
                    ? "Returning to the kitchen. Prepare to decide."
                    : "Find the safe route to the exit.";
                ApplyMovementState();
                return;
            }
            ApplyMovementState();
            decisionPanel.SetActive(true);
            panelGroup.alpha = 1;
            panelGroup.interactable = panelGroup.blocksRaycasts = !menuOpen;
            int revision = Session.Revision;
            for (int i = 0; i < optionButtons.Length; i++)
            {
                Button button = optionButtons[i];
                button.onClick.RemoveAllListeners();
                bool visible = i < (Session.HasFinalRank ? 2 : Session.OptionCount);
                button.gameObject.SetActive(visible);
                if (!visible) continue;
                Layout(button.GetComponent<RectTransform>(), new Vector2(0, -510 - 94 * i), new Vector2(1000, 78));
                Text label = button.GetComponentInChildren<Text>(true);
                label.fontSize = 28;
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = 22;
                label.resizeTextMaxSize = 28;
                if (Session.HasFinalRank)
                {
                    SetButtonLabel(button, i == 0 ? "Replay Level" : "Main Menu");
                    if (i == 0) button.onClick.AddListener(Replay);
                    else button.onClick.AddListener(ReturnToMenu);
                }
                else
                {
                    int option = i;
                    SetButtonLabel(button, Session.GetOptionText(i));
                    button.onClick.AddListener(() => Choose(revision, option));
                }
                button.interactable = false;
            }
            // The next screen stays locked through a double click, including pointer and submit input.
            acceptInputAfter = Time.unscaledTime + .35f;
            if (Session.HasFinalRank)
            {
                resultsUI.ShowEvacuationResults(Session, panelGroup, titleText, bodyText);
                dangerText.text = Session.SuccessfullyEvacuated ? "Evacuation successful" : "Evacuation failed";
            }
            else
            {
                titleText.text = Session.Title;
                bodyText.text = Session.Description;
                dangerText.text = Session.Step == EvacuationStep.Outside
                    ? "You have reached a safe place outside" : "Fire spreading - Safe exit clear";
            }
        }

        private void Choose(int revision, int index)
        {
            if (menuOpen || IsWalking || Time.unscaledTime < acceptInputAfter) return;
            if (!Session.TrySelect(revision, index)) return;
            // Both the safe first choice and the "leave now" correction converge here.
            routeGate.Begin(Session.Step);
            kitchenGate.Begin(Session.Step);
            thirdGate.Begin(Session.Step);
            Render();
        }

        public void Replay()
        {
            // Reload restores the same player component state as a fresh level start,
            // including movement velocity, camera pitch, head bob and the route gate.
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameObject.scene.name);
        }

        private void ToggleMenu()
        {
            menuOpen = !menuOpen;
            menuPanel.SetActive(menuOpen);
            menuPanel.transform.SetAsLastSibling();
            panelGroup.interactable = panelGroup.blocksRaycasts = !menuOpen && !IsWalking;
            ApplyMovementState();
        }

        private void ApplyMovementState()
        {
            bool canMove = IsWalking && !menuOpen;
            movementGameManager.ChangeState(menuOpen ? GameState.Paused
                : canMove ? GameState.Exploration
                : Session.HasFinalRank ? GameState.Finished : GameState.Decision);
            playerMovement.enabled = canMove;
            reticle.SetActive(canMove);
            Cursor.lockState = canMove ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !canMove;
        }

        private static void SetButtonLabel(Button button, string label)
        {
            button.GetComponentInChildren<Text>(true).text = label;
        }

        private void ReturnToMenu()
        {
            // Leaving an unfinished round does not finalize it or emit a failure.
            Time.timeScale = 1;
            SceneManager.LoadScene("Menu");
        }

        private void OnDestroy()
        {
            menuButton.onClick.RemoveListener(ToggleMenu);
            resumeButton.onClick.RemoveListener(ToggleMenu);
            restartButton.onClick.RemoveListener(Replay);
            mainMenuButton.onClick.RemoveListener(ReturnToMenu);
        }
    }
}
