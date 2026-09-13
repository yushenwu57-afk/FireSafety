using UnityEngine;
using UnityEngine.UI;

public class ReticleGraphic : Graphic
{
    [SerializeField] private float radius = 14f;
    [SerializeField] private float thickness = 3f;
    [SerializeField] private int segments = 48;

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();

        int safeSegments = Mathf.Max(12, segments);
        float outerRadius = Mathf.Max(radius, thickness);
        float innerRadius = Mathf.Max(0f, outerRadius - thickness);

        for (int i = 0; i < safeSegments; i++)
        {
            float angle = (Mathf.PI * 2f * i) / safeSegments;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            vertexHelper.AddVert(direction * outerRadius, color, Vector2.zero);
            vertexHelper.AddVert(direction * innerRadius, color, Vector2.zero);
        }

        for (int i = 0; i < safeSegments; i++)
        {
            int next = (i + 1) % safeSegments;
            int outerCurrent = i * 2;
            int innerCurrent = outerCurrent + 1;
            int outerNext = next * 2;
            int innerNext = outerNext + 1;

            vertexHelper.AddTriangle(outerCurrent, outerNext, innerCurrent);
            vertexHelper.AddTriangle(innerCurrent, outerNext, innerNext);
        }
    }
}
