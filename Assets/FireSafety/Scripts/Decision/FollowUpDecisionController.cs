using FireSafety.Scenario;
using UnityEngine;

namespace FireSafety.Decision
{
    public sealed class FollowUpDecisionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ScenarioManager scenarioManager;
        [SerializeField] private DecisionManager decisionManager;

        [Header("Follow-Up Decisions")]
        [SerializeField] private DecisionDefinition stabilisingDecision =
            new DecisionDefinition();
        [SerializeField] private DecisionDefinition escalatingDecision =
            new DecisionDefinition();
        [SerializeField] private DecisionDefinition recoveryDecision =
            new DecisionDefinition();

        private bool hasOpenedStabilisingDecision;
        private bool hasOpenedEscalatingDecision;
        private bool hasOpenedRecoveryDecision;

        private void OnEnable()
        {
            if (scenarioManager != null)
            {
                scenarioManager.ConsequenceContinued += HandleConsequenceContinued;
            }
        }

        private void OnDisable()
        {
            if (scenarioManager != null)
            {
                scenarioManager.ConsequenceContinued -= HandleConsequenceContinued;
            }
        }

        private void HandleConsequenceContinued(ScenarioPhase currentPhase)
        {
            if (decisionManager == null)
            {
                return;
            }

            switch (currentPhase)
            {
                case ScenarioPhase.Stabilising:
                    OpenStabilisingDecision();
                    break;
                case ScenarioPhase.Escalating:
                    OpenEscalatingDecision();
                    break;
                case ScenarioPhase.Recovering:
                    OpenRecoveryDecision();
                    break;
            }
        }

        private void OpenStabilisingDecision()
        {
            if (hasOpenedStabilisingDecision || stabilisingDecision == null)
            {
                return;
            }

            DecisionData decision = stabilisingDecision.CreateDecisionData();
            decisionManager.OpenDecision(decision);

            if (decisionManager.CurrentDecision == decision)
            {
                hasOpenedStabilisingDecision = true;
            }
        }

        private void OpenEscalatingDecision()
        {
            if (hasOpenedEscalatingDecision || escalatingDecision == null)
            {
                return;
            }

            DecisionData decision = escalatingDecision.CreateDecisionData();
            decisionManager.OpenDecision(decision);

            if (decisionManager.CurrentDecision == decision)
            {
                hasOpenedEscalatingDecision = true;
            }
        }

        private void OpenRecoveryDecision()
        {
            if (hasOpenedRecoveryDecision || recoveryDecision == null)
            {
                return;
            }

            DecisionData decision = recoveryDecision.CreateDecisionData();
            decisionManager.OpenDecision(decision);

            if (decisionManager.CurrentDecision == decision)
            {
                hasOpenedRecoveryDecision = true;
            }
        }
    }
}
