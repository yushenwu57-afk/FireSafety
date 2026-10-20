using FireSafety.Hazard;
using FireSafety.UI;
using UnityEngine;

public sealed class ScenarioMusic : MonoBehaviour
{
    private static AudioSource sharedMusicSource;

    [Header("Music")]
    [SerializeField] private AudioClip before;
    [SerializeField] private AudioClip happen;
    [SerializeField] private AudioClip results;
    [SerializeField, Range(0f, 1f)] private float volume = 0.6f;

    [Header("Scene References")]
    [SerializeField] private DangerManager dangerManager;
    [SerializeField] private GameHud legacyHud;
    [SerializeField] private FinalResultsUI finalResults;

    private AudioSource musicSource;
    private bool showingResults;

    private void Awake()
    {
        // Scene components own their event subscriptions; only playback survives loads.
        if (sharedMusicSource == null)
        {
            GameObject playbackObject = new GameObject("Scenario Music Playback");
            sharedMusicSource = playbackObject.AddComponent<AudioSource>();
            sharedMusicSource.playOnAwake = false;
            sharedMusicSource.spatialBlend = 0f;
            sharedMusicSource.loop = true;
            DontDestroyOnLoad(playbackObject);
        }

        musicSource = sharedMusicSource;
        musicSource.volume = volume;
    }

    private void OnEnable()
    {
        if (dangerManager != null)
        {
            dangerManager.DangerLevelChanged += HandleDangerChanged;
        }

        if (legacyHud != null)
        {
            legacyHud.DangerLevelChanged += HandleLegacyDangerChanged;
        }

        if (finalResults != null)
        {
            finalResults.ResultsShown += HandleResultsShown;
        }
    }

    private void Start()
    {
        if (showingResults || (finalResults != null && finalResults.IsVisible))
        {
            HandleResultsShown();
        }
        else if ((dangerManager != null && dangerManager.CurrentLevel >= DangerLevel.Medium)
            || (legacyHud != null && IsLegacyHazard(legacyHud.CurrentDangerLevel)))
        {
            Play(happen);
        }
        else
        {
            Play(before);
        }
    }

    private void OnDisable()
    {
        if (dangerManager != null)
        {
            dangerManager.DangerLevelChanged -= HandleDangerChanged;
        }

        if (legacyHud != null)
        {
            legacyHud.DangerLevelChanged -= HandleLegacyDangerChanged;
        }

        if (finalResults != null)
        {
            finalResults.ResultsShown -= HandleResultsShown;
        }
    }

    private void HandleDangerChanged(DangerLevel previousLevel, DangerLevel newLevel)
    {
        if (!showingResults && newLevel == DangerLevel.Medium)
        {
            Play(happen);
        }
    }

    private void HandleLegacyDangerChanged(string newLevel)
    {
        if (!showingResults && string.Equals(newLevel, "Medium", System.StringComparison.OrdinalIgnoreCase))
        {
            Play(happen);
        }
    }

    private void HandleResultsShown()
    {
        if (showingResults)
        {
            return;
        }

        showingResults = true;
        Play(results, false);
    }

    private static bool IsLegacyHazard(string level)
    {
        return string.Equals(level, "Medium", System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(level, "High", System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(level, "Critical", System.StringComparison.OrdinalIgnoreCase);
    }

    private void Play(AudioClip clip, bool loop = true)
    {
        if (clip == null || (musicSource.clip == clip && musicSource.isPlaying))
        {
            return;
        }

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }
}
