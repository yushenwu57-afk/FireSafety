using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DelayedFireEvent : MonoBehaviour
{
    [SerializeField] private string targetEffectName = "Particles_Fire_02";
    [SerializeField] private string[] additionalTargetEffectNames = { "Particles_Dust_01" };
    [SerializeField] private float delaySeconds = 5f;
    [SerializeField] private string dangerLevelAfterDelay = "Medium";
    [SerializeField] private GameHud gameHud;

    private readonly List<GameObject> targetEffects = new List<GameObject>();
    private Coroutine delayCoroutine;

    private void Awake()
    {
        if (gameHud == null)
        {
            gameHud = GetComponent<GameHud>();
        }

        FindTargetEffects();
        SetEffectsActive(false);
    }

    private void OnEnable()
    {
        delayCoroutine = StartCoroutine(RevealFireAfterDelay());
    }

    private void OnDisable()
    {
        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
            delayCoroutine = null;
        }
    }

    private IEnumerator RevealFireAfterDelay()
    {
        yield return new WaitForSeconds(delaySeconds);

        SetEffectsActive(true);

        GameHud hud = gameHud != null ? gameHud : GameHud.ActiveHud;
        if (hud != null)
        {
            hud.SetDangerLevel(dangerLevelAfterDelay);
        }
    }

    private void FindTargetEffects()
    {
        targetEffects.Clear();
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject candidate = allObjects[i];
            if (candidate.scene == activeScene && IsTargetEffectName(candidate.name))
            {
                targetEffects.Add(candidate);
            }
        }
    }

    private bool IsTargetEffectName(string candidateName)
    {
        if (candidateName == targetEffectName)
        {
            return true;
        }

        if (additionalTargetEffectNames == null)
        {
            return false;
        }

        for (int i = 0; i < additionalTargetEffectNames.Length; i++)
        {
            if (candidateName == additionalTargetEffectNames[i])
            {
                return true;
            }
        }

        return false;
    }

    private void SetEffectsActive(bool active)
    {
        for (int i = 0; i < targetEffects.Count; i++)
        {
            GameObject effect = targetEffects[i];
            if (effect == null)
            {
                continue;
            }

            effect.SetActive(active);
            ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>(true);
            for (int j = 0; j < particleSystems.Length; j++)
            {
                if (active)
                {
                    particleSystems[j].Play(true);
                }
                else
                {
                    particleSystems[j].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }
    }
}
