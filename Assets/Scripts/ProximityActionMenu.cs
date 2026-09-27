using UnityEngine;

public class ProximityActionMenu : MonoBehaviour
{
    [SerializeField] private ActionContext actionContext;
    [SerializeField] private float triggerDistance = 2f;
    [SerializeField] private float activationDelaySeconds = 5f;

    private Transform playerTransform;
    private bool hasShown;
    private bool isFinished;

    private void Awake()
    {
        if (actionContext == null)
        {
            actionContext = GetComponent<ActionContext>();
        }
    }

    private void Update()
    {
        if (isFinished || actionContext == null || actionContext.OptionCount == 0)
        {
            return;
        }

        if (Time.timeSinceLevelLoad < activationDelaySeconds)
        {
            return;
        }

        if (playerTransform == null)
        {
            player playerComponent = FindObjectOfType<player>();
            if (playerComponent == null)
            {
                return;
            }

            playerTransform = playerComponent.transform;
        }

        float sqrDistance = (playerTransform.position - transform.position).sqrMagnitude;
        bool isInRange = sqrDistance <= triggerDistance * triggerDistance;

        if (!isInRange)
        {
            hasShown = false;
            return;
        }

        if (hasShown || GameHud.IsMenuOpen || GameHud.IsActionPanelOpen)
        {
            return;
        }

        GameHud.ActiveHud?.ShowActionPanel(actionContext);
        hasShown = true;
    }

    public void Finish()
    {
        isFinished = true;
        hasShown = true;
    }
}
