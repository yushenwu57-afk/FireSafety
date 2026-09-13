using UnityEngine;

public class ObservableContext : MonoBehaviour
{
    [TextArea(2, 4)]
    [SerializeField] private string promptText = "Faint smoke rises around the object. The surface looks hot, and the surrounding air shimmers slightly.";

    public string PromptText => promptText;
}
