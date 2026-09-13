using UnityEngine;

public class HighlightableObject : MonoBehaviour
{
    [SerializeField] private Color highlightColor = new Color(1f, 0.82f, 0.22f, 1f);
    [SerializeField, Range(0f, 1f)] private float highlightBlend = 0.45f;

    private Renderer[] renderers;
    private Material[][] rendererMaterials;
    private Color[][] originalColors;
    private bool isHighlighted;

    private void Awake()
    {
        CacheMaterials();
    }

    public void SetHighlighted(bool highlighted)
    {
        if (isHighlighted == highlighted)
        {
            return;
        }

        isHighlighted = highlighted;

        if (rendererMaterials == null)
        {
            CacheMaterials();
        }

        for (int rendererIndex = 0; rendererIndex < rendererMaterials.Length; rendererIndex++)
        {
            Material[] materials = rendererMaterials[rendererIndex];
            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                Material material = materials[materialIndex];
                if (material == null)
                {
                    continue;
                }

                string colorProperty = GetColorProperty(material);
                if (string.IsNullOrEmpty(colorProperty))
                {
                    continue;
                }

                Color color = highlighted
                    ? Color.Lerp(originalColors[rendererIndex][materialIndex], highlightColor, highlightBlend)
                    : originalColors[rendererIndex][materialIndex];

                material.SetColor(colorProperty, color);
            }
        }
    }

    private void OnDisable()
    {
        SetHighlighted(false);
    }

    private void CacheMaterials()
    {
        renderers = GetComponentsInChildren<Renderer>();
        rendererMaterials = new Material[renderers.Length][];
        originalColors = new Color[renderers.Length][];

        for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
        {
            Material[] materials = renderers[rendererIndex].materials;
            rendererMaterials[rendererIndex] = materials;
            originalColors[rendererIndex] = new Color[materials.Length];

            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                Material material = materials[materialIndex];
                string colorProperty = material == null ? "" : GetColorProperty(material);
                originalColors[rendererIndex][materialIndex] = string.IsNullOrEmpty(colorProperty)
                    ? Color.white
                    : material.GetColor(colorProperty);
            }
        }
    }

    private string GetColorProperty(Material material)
    {
        if (material.HasProperty("_BaseColor"))
        {
            return "_BaseColor";
        }

        return material.HasProperty("_Color") ? "_Color" : "";
    }
}
