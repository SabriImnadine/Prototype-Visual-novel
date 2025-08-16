using UnityEngine;
using UnityEngine.U2D;

[ExecuteInEditMode]
public class TerrainController : MonoBehaviour
{
    [Header("Terrain Settings")]
    [SerializeField] private SpriteShapeController shapeController;
    [SerializeField, Range(3, 500)] private int segmentCount = 100;
    [SerializeField, Range(1f, 50f)] private float horizontalSpacing = 2f;
    [SerializeField, Range(1f, 50f)] private float heightVariation = 2f;
    [SerializeField, Range(0f, 1f)] private float noiseScale = 0.5f;
    [SerializeField, Range(0f, 5f)] private float curveTightness = 0.5f;
    [SerializeField] private float groundDepth = 10f;

    private void OnValidate()
    {
        if (!shapeController) return;

        shapeController.spline.Clear();

        
        for (int pointIndex = 0; pointIndex < segmentCount; pointIndex++)
        {
            float x = transform.position.x + (pointIndex * horizontalSpacing);
            float y = transform.position.y + Mathf.PerlinNoise(pointIndex * noiseScale, 0f) * heightVariation;
            shapeController.spline.InsertPointAt(pointIndex, new Vector3(x, y, 0f));

            if (pointIndex > 0 && pointIndex < segmentCount - 1)
            {
                shapeController.spline.SetTangentMode(pointIndex, ShapeTangentMode.Continuous);
                shapeController.spline.SetLeftTangent(pointIndex, Vector3.left * horizontalSpacing * curveTightness);
                shapeController.spline.SetRightTangent(pointIndex, Vector3.right * horizontalSpacing * curveTightness);
            }
        }

        Vector3 lastPoint = shapeController.spline.GetPosition(segmentCount - 1);
        shapeController.spline.InsertPointAt(segmentCount, new Vector3(lastPoint.x, transform.position.y - groundDepth, 0f));
        shapeController.spline.InsertPointAt(segmentCount + 1, new Vector3(transform.position.x, transform.position.y - groundDepth, 0f));
    }
}
