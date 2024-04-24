using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public enum SurfaceType
{
    Opaque,
    TransparentBlend,
    TransparentCutout
}

public enum FaceRenderingMode
{
    FrontOnly, 
    NoCulling,
    DoubleSided    
}

public class CustomLitInspector : ShaderGUI
{
    private static readonly int SourceBlend = Shader.PropertyToID("_SourceBlend");
    private static readonly int DestBlend = Shader.PropertyToID("_DestBlend");
    private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");
    private static readonly int Type = Shader.PropertyToID("_SurfaceType");
    private static readonly int RenderingMode = Shader.PropertyToID("_FaceRenderingMode");
    private static readonly int Cull = Shader.PropertyToID("_Cull");

    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
    {
        base.AssignNewShaderToMaterial(material, oldShader, newShader);

        if (newShader.name == "Custom/CustomLit")
        {
            UpdateSurfaceType(material);
        }
    }
    
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        var material = materialEditor.target as Material;
        var surfaceProperty = BaseShaderGUI.FindProperty("_SurfaceType", properties, true);
        var faceProperty = BaseShaderGUI.FindProperty("_FaceRenderingMode", properties, true);
        
        EditorGUI.BeginChangeCheck();
        surfaceProperty.floatValue = (int)(SurfaceType)EditorGUILayout.EnumPopup("Surface type", (SurfaceType)surfaceProperty.floatValue);
        faceProperty.floatValue = (int)(FaceRenderingMode)EditorGUILayout.EnumPopup("Face rendering mode", (FaceRenderingMode)faceProperty.floatValue);
        
        if (EditorGUI.EndChangeCheck())
        {
            UpdateSurfaceType(material);
        }

        base.OnGUI(materialEditor, properties);
    }

    private void UpdateSurfaceType(Material material)
    {
        SurfaceType surface = (SurfaceType)material.GetFloat(Type);

        switch (surface)
        {
            case SurfaceType.Opaque:
                material.renderQueue = (int)RenderQueue.Geometry;
                material.SetOverrideTag("RenderType", "Opaque");
                material.SetInt(SourceBlend, (int)BlendMode.One);
                material.SetInt(DestBlend, (int)BlendMode.Zero);
                material.SetInt(ZWrite, 1);
                break;
            case SurfaceType.TransparentBlend:
                material.renderQueue = (int)RenderQueue.Transparent;
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt(SourceBlend, (int)BlendMode.SrcAlpha);
                material.SetInt(DestBlend, (int)BlendMode.OneMinusSrcAlpha);
                material.SetInt(ZWrite, 0);
                break;
            case SurfaceType.TransparentCutout:
                material.renderQueue = (int)RenderQueue.AlphaTest;
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetInt(SourceBlend, (int)BlendMode.One);
                material.SetInt(DestBlend, (int)BlendMode.Zero);
                material.SetInt(ZWrite, 1);
                break;
        }
        
        material.SetShaderPassEnabled("ShadowCaster", surface != SurfaceType.TransparentBlend);

        if (surface == SurfaceType.TransparentCutout) material.EnableKeyword("_ALPHA_CUTOUT");
        else material.DisableKeyword("_ALPHA_CUTOUT");

        var faceRenderingMode = (FaceRenderingMode)material.GetFloat(RenderingMode);
        if(faceRenderingMode == FaceRenderingMode.FrontOnly) material.SetInt(Cull, (int)CullMode.Back);
        else material.SetInt(Cull, (int)CullMode.Off);
        
        if(faceRenderingMode == FaceRenderingMode.DoubleSided) material.EnableKeyword("_DOUBLE_SIDED_NORMALS");
        else material.DisableKeyword("_DOUBLE_SIDED_NORMALS");
    }
}

