# Unity project context

Inspected 2026-10-20. Root: /Users/yushenwu/Fire.

- Confirmed Unity 2022.3.62f3, Built-in rendering (GraphicsSettings has no custom pipeline), legacy Input Manager (activeInputHandler 0).
- uGUI and TMP are actively used. No networking or Unity MCP provider detected in manifest, source, or available tools. Unity is open; UI and local Mono compiler are available.
- No first-party asmdefs, tests, CI, README or AGENTS.md found. Runtime code compiles into Assembly-CSharp; Assets/Editor is editor-only.
- MenuController starts Level. Level is the current first-level scene with FireSafety systems and FinalResultsUI. Level_2 retains legacy kitchen assets; its traversal now uses the same PlayerMovement/PlayerLook/PlayerHeadBob components as Level, with a level-local GameManager. Level_1 was deleted in the pre-existing working tree; its disabled Build Settings entry still existed at inspection.
- FireSafety/Decision owns decision events. PlayerPerformanceTracker records ResponseQuality.Safe/Unsafe/Critical and recovery; PerformanceRankCalculator emits S/A/B/C. FinalResultsUI presents the result. SceneFlowService reloads the active scene. Legacy GameHud separately owns pause/menu/options.
- Namespaces FireSafety.Core/Scenario/Evaluation/UI; private SerializeField references; event subscriptions balanced in OnEnable/OnDisable.
- No region or CBAS-specific data structure found. Level 2 maps ordinary mistakes to Unsafe, serious mistakes to Critical, and reuses PerformanceRank plus FinalResultsUI; this is project-specific behavior evaluation.
- Existing uncommitted edits include Level/Menu scenes, HUD/menu builders, UI/Core scripts, ProjectSettings and new Level_2/music assets. Preserve them.
- Validation: local Mono compile, deterministic branch tests, serialized wiring review; Editor play checks when available. No dependencies should be installed for this feature.

- Level 2 uses the actual enabled Level 1 PlayerMovement/PlayerLook/PlayerHeadBob scripts with identical serialized tuning, camera height and CharacterController settings. Its local GameManager gates Exploration between decision one and Floor_16 arrival (-3.125, 2.5, 5.125), Decision for panels, Paused for ESC and Finished for results. The legacy player.cs stays disabled and unmodified. Replay reloads Level_2, resetting player physics, camera, session and the arrival gate.

- Returning to the kitchen after decision two now arms a separate arrival gate targeting RoomFurniture_03 (Transform 1804536684; world approx 3.477, 0.109183, 5.363). Only arrival opens ReturnFromKitchen options; the serious error is recorded at the initial return choice. Radius 1.5m / vertical tolerance 0.75m; safe-exit route bypasses it.
