using UnityEngine;

[ExecuteAlways]
public class SnowDeformer : MonoBehaviour
{
    public RenderTexture deformationTexture;
    public Shader drawShader;
    public float brushSize = 0.1f;
    public float brushStrength = 0.5f;

    private Material _drawMat;
    private static readonly int Coords = Shader.PropertyToID("_Coords");
    private static readonly int Strength = Shader.PropertyToID("_Strength");

    private void Start()
    {
        _drawMat = new Material(drawShader);
    }

    private void LateUpdate()
    {
        var pos = transform.position;
        var uv = new Vector2(pos.x, pos.z) * 0.5f + Vector2.one * 0.5f;

        _drawMat.SetVector(Coords, new Vector4(uv.x, uv.y, brushSize, 0));
        _drawMat.SetFloat(Strength, brushStrength);

        var temp = RenderTexture.GetTemporary(deformationTexture.width, deformationTexture.height);
        Graphics.Blit(deformationTexture, temp);
        Graphics.Blit(temp, deformationTexture, _drawMat);
        RenderTexture.ReleaseTemporary(temp);
    }
}