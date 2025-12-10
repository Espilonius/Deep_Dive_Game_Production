using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DayPartInfo))]
public class DayPartInfoEditor : Editor
{
    private Texture2D gradientPreviewTex;

    private const int PreviewWidth = 200;
    private const int PreviewHeight = 20;

    private void OnDisable()
    {
        if (gradientPreviewTex != null)
        {
            Object.DestroyImmediate(gradientPreviewTex);
            gradientPreviewTex = null;
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Gradient Preview", EditorStyles.boldLabel);

        var dp = (DayPartInfo)target;
        if (dp.DayPartGradient == null)
        {
            EditorGUILayout.HelpBox("No gradient assigned.", MessageType.Info);
            return;
        }

        // Zorg dat we een texture hebben
        if (gradientPreviewTex == null ||
            gradientPreviewTex.width != PreviewWidth ||
            gradientPreviewTex.height != PreviewHeight)
        {
            gradientPreviewTex = new Texture2D(PreviewWidth, PreviewHeight, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };
        }

        // Texture vullen op basis van gradient
        for (int x = 0; x < PreviewWidth; x++)
        {
            float t = x / (float)(PreviewWidth - 1);
            Color c = dp.DayPartGradient.Evaluate(t);
            for (int y = 0; y < PreviewHeight; y++)
            {
                gradientPreviewTex.SetPixel(x, y, c);
            }
        }
        gradientPreviewTex.Apply();

        Rect r = GUILayoutUtility.GetRect(PreviewWidth, PreviewHeight);
        EditorGUI.DrawPreviewTexture(r, gradientPreviewTex);

        EditorGUILayout.HelpBox(
            "This preview shows the light color progression over this day part (0 → 1).",
            MessageType.None
        );
    }
}
