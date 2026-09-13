using System;
using UnityEngine;
using UnityEngine.Events;

public class ActionContext : MonoBehaviour
{
    [SerializeField] private ContextActionOption[] options = Array.Empty<ContextActionOption>();

    public int OptionCount => options == null ? 0 : options.Length;

    public ContextActionOption GetOption(int index)
    {
        if (options == null || index < 0 || index >= options.Length)
        {
            return null;
        }

        return options[index];
    }

    public string PerformOption(int index)
    {
        ContextActionOption option = GetOption(index);
        if (option == null)
        {
            return "";
        }

        option.Invoke();
        return option.ResultText;
    }
}

[Serializable]
public class ContextActionOption
{
    [SerializeField] private string label = "Action";
    [TextArea(2, 3)]
    [SerializeField] private string resultText = "";
    [SerializeField] private UnityEvent onSelected;

    public string Label => label;
    public string ResultText => resultText;

    public void Invoke()
    {
        onSelected?.Invoke();
    }
}
