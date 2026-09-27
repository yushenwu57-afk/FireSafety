using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanSmokeResponse : MonoBehaviour
{
    private enum FollowUpDecision
    {
        None,
        StoveOff,
        Water
    }

    [SerializeField] private string smokeEffectName = "Particles_Dust_01";
    [SerializeField] private string fireEffectName = "Particles_Fire_02";
    [SerializeField] private float smokeSizeMultiplier = 0.35f;
    [SerializeField] private float waterFireScale = 2f;
    [SerializeField] private float waterFireEmissionMultiplier = 1.5f;
    [SerializeField] private float waterSmokeSizeMultiplier = 2f;
    [SerializeField] private float waterSmokeEmissionMultiplier = 2.5f;
    [SerializeField] private string dangerLevelAfterWater = "High";

    private bool hasReducedSmoke;
    private bool hasPouredWater;
    private FollowUpDecision pendingFollowUp = FollowUpDecision.None;
    private bool isComplete;
    private string finalResultText = "";

    public bool HasPendingFollowUp => pendingFollowUp != FollowUpDecision.None;
    public bool IsComplete => isComplete;
    public string FinalResultText => finalResultText;

    public void BeginInitialChoice(int optionIndex)
    {
        if (isComplete)
        {
            return;
        }

        ProximityActionMenu proximityMenu = GetComponent<ProximityActionMenu>();
        if (proximityMenu != null)
        {
            proximityMenu.Finish();
        }

        if (optionIndex == 0)
        {
            ReduceSmokeSize();
            pendingFollowUp = FollowUpDecision.StoveOff;
        }
        else if (optionIndex == 1)
        {
            PourWaterOnFire();
            pendingFollowUp = FollowUpDecision.Water;
        }
    }

    public string GetFollowUpTitle()
    {
        switch (pendingFollowUp)
        {
            case FollowUpDecision.StoveOff:
                return "Decision 2A";

            case FollowUpDecision.Water:
                return "Decision 2B";

            default:
                return "Available Actions";
        }
    }

    public int GetFollowUpOptionCount()
    {
        return HasPendingFollowUp ? 2 : 0;
    }

    public string GetFollowUpOptionLabel(int index)
    {
        if (pendingFollowUp == FollowUpDecision.StoveOff)
        {
            return index == 0 ? "Use fire blanket" : "Evacuate";
        }

        if (pendingFollowUp == FollowUpDecision.Water)
        {
            return index == 0 ? "Evacuate immediately" : "Keep trying to fight fire";
        }

        return "";
    }

    public string PerformFollowUpOption(int index)
    {
        if (!HasPendingFollowUp || isComplete)
        {
            return "";
        }

        if (pendingFollowUp == FollowUpDecision.StoveOff)
        {
            if (index == 0)
            {
                ExtinguishFire();
                finalResultText = "The fire is out.\nBEST ENDING";
            }
            else
            {
                finalResultText =
                    "SAFE EVACUATION\n\n" +
                    "✓ Heat Source Removed\n" +
                    "You turned off the stove.\n\n" +
                    "✓ Player Safety\n" +
                    "You evacuated before conditions became more dangerous.\n\n" +
                    "△ Fire Not Controlled\n" +
                    "The remaining fire was not extinguished.";
            }
        }
        else if (pendingFollowUp == FollowUpDecision.Water)
        {
            if (index == 0)
            {
                finalResultText = "You evacuate immediately and recover safely.\nSAFE ENDING";
            }
            else
            {
                IntensifyFireAndSmoke();
                finalResultText = "The fire and smoke keep increasing.\nUNSAFE ENDING";
            }
        }

        pendingFollowUp = FollowUpDecision.None;
        isComplete = true;
        return finalResultText;
    }

    public void ReduceSmokeSize()
    {
        if (hasReducedSmoke)
        {
            return;
        }

        bool foundSmoke = false;
        foreach (GameObject smokeEffect in FindSceneEffects(smokeEffectName))
        {
            ScaleEffect(smokeEffect, smokeSizeMultiplier, 1f);
            foundSmoke = true;
        }

        hasReducedSmoke = foundSmoke;
    }

    public void PourWaterOnFire()
    {
        if (hasPouredWater)
        {
            return;
        }

        foreach (GameObject fireEffect in FindSceneEffects(fireEffectName))
        {
            fireEffect.transform.localScale = Vector3.one * waterFireScale;
            ScaleEffect(fireEffect, 1f, waterFireEmissionMultiplier);
        }

        foreach (GameObject smokeEffect in FindSceneEffects(smokeEffectName))
        {
            ScaleEffect(smokeEffect, waterSmokeSizeMultiplier, waterSmokeEmissionMultiplier);
        }

        GameHud.ActiveHud?.SetDangerLevel(dangerLevelAfterWater);
        hasPouredWater = true;
    }

    private void ExtinguishFire()
    {
        foreach (GameObject fireEffect in FindSceneEffects(fireEffectName))
        {
            fireEffect.SetActive(false);
        }

        foreach (GameObject smokeEffect in FindSceneEffects(smokeEffectName))
        {
            smokeEffect.SetActive(false);
        }

        GameHud.ActiveHud?.SetDangerLevel("Safe");
    }

    private void IntensifyFireAndSmoke()
    {
        foreach (GameObject fireEffect in FindSceneEffects(fireEffectName))
        {
            fireEffect.transform.localScale *= waterFireScale;
            ScaleEffect(fireEffect, 1f, waterFireEmissionMultiplier);
        }

        foreach (GameObject smokeEffect in FindSceneEffects(smokeEffectName))
        {
            ScaleEffect(smokeEffect, waterSmokeSizeMultiplier, waterSmokeEmissionMultiplier);
        }

        GameHud.ActiveHud?.SetDangerLevel("Unsafe");
    }

    private IEnumerable<GameObject> FindSceneEffects(string baseName)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject candidate = allObjects[i];
            if (candidate.scene == activeScene && IsEffectNameMatch(candidate.name, baseName))
            {
                yield return candidate;
            }
        }
    }

    private bool IsEffectNameMatch(string candidateName, string baseName)
    {
        return candidateName == baseName || candidateName.StartsWith(baseName + " (");
    }

    private void ScaleEffect(GameObject effect, float sizeMultiplier, float emissionMultiplier)
    {
        ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem.MainModule main = particleSystems[i].main;
            main.startSize = ScaleStartSize(main.startSize, sizeMultiplier);

            ParticleSystem.EmissionModule emission = particleSystems[i].emission;
            emission.rateOverTime = ScaleEmissionRate(emission.rateOverTime, emissionMultiplier);
        }
    }

    private ParticleSystem.MinMaxCurve ScaleStartSize(ParticleSystem.MinMaxCurve startSize, float multiplier)
    {
        switch (startSize.mode)
        {
            case ParticleSystemCurveMode.Constant:
                return new ParticleSystem.MinMaxCurve(startSize.constant * multiplier);

            case ParticleSystemCurveMode.TwoConstants:
                return new ParticleSystem.MinMaxCurve(
                    startSize.constantMin * multiplier,
                    startSize.constantMax * multiplier);

            case ParticleSystemCurveMode.Curve:
            case ParticleSystemCurveMode.TwoCurves:
                startSize.curveMultiplier *= multiplier;
                return startSize;

            default:
                return startSize;
        }
    }

    private ParticleSystem.MinMaxCurve ScaleEmissionRate(ParticleSystem.MinMaxCurve emissionRate, float multiplier)
    {
        return ScaleStartSize(emissionRate, multiplier);
    }
}
