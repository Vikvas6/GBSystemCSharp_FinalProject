using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;


partial class CameraRenderer
{
    private ScriptableRenderContext _context;
    private Camera _camera;
    private bool _dynamicBatching;
    private bool _instancing;
    private readonly CommandBuffer _commandBuffer = new CommandBuffer {name = bufferName};
    private const string bufferName = "Camera Render";
    private CullingResults _cullingResult;
    private static readonly List<ShaderTagId> drawingShaderTagIds =
        new List<ShaderTagId>
        {
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Always"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM"),
        };
    
    const int maxVisibleLights = 4;
	
    static int visibleLightColorsId = Shader.PropertyToID("_VisibleLightColors");
    static int visibleLightDirectionsOrPositionsId = Shader.PropertyToID("_VisibleLightDirectionsOrPositions");
    static int visibleLightAttenuationsId = Shader.PropertyToID("_VisibleLightAttenuations");
    static int visibleLightSpotDirectionsId = Shader.PropertyToID("_VisibleLightSpotDirections");
	
    Vector4[] visibleLightColors = new Vector4[maxVisibleLights];
    Vector4[] visibleLightDirectionsOrPositions = new Vector4[maxVisibleLights];
    Vector4[] visibleLightAttenuations = new Vector4[maxVisibleLights];
    Vector4[] visibleLightSpotDirections = new Vector4[maxVisibleLights];
    
    public void Render(ScriptableRenderContext context, Camera camera, bool dynamicBatching, bool instancing)
    {
        _camera = camera;
        _context = context;
        _dynamicBatching = dynamicBatching;
        _instancing = instancing;

        PrepareBuffer();
        PrepareForSceneWindow();
        if (!Cull(out var parameters))
        {
            return;
        }
        Settings(parameters);
        DrawVisible();
        
        DrawUnsupportedShaders();
        DrawGizmos();
        DrawUI();
        Submit();
    }

    private void DrawVisible()
    {
        var drawingSettings = CreateDrawingSettings(drawingShaderTagIds, SortingCriteria.CommonOpaque, out var sortingSettings);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        _context.DrawRenderers(_cullingResult, ref drawingSettings, ref filteringSettings);

        _context.DrawSkybox(_camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        _context.DrawRenderers(_cullingResult, ref drawingSettings, ref filteringSettings);

        // sortingSettings.criteria = SortingCriteria.None;
        // var drawSettings = new DrawingSettings(new ShaderTagId("ForwardBase"), sortingSettings);
        // filteringSettings.renderQueueRange = RenderQueueRange.all;
        // _context.DrawRenderers(_cullingResult, ref drawSettings, ref filteringSettings);
    }

    private void Settings(ScriptableCullingParameters parameters)
    {
        //parameters.cullingOptions = CullingOptions.ShadowCasters;
        _cullingResult = _context.Cull(ref parameters);
        _context.SetupCameraProperties(_camera);
        CameraClearFlags flags = _camera.clearFlags;
        _commandBuffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags == CameraClearFlags.Color,
            flags == CameraClearFlags.Color ? _camera.backgroundColor.linear : Color.clear);

        if (_cullingResult.visibleLights.Length > 0)
        {
            ConfigureLights();
        }

        _commandBuffer.BeginSample(SampleName);
        _commandBuffer.SetGlobalVectorArray(
            visibleLightColorsId, visibleLightColors
        );
        _commandBuffer.SetGlobalVectorArray(
            visibleLightDirectionsOrPositionsId, visibleLightDirectionsOrPositions
        );
        _commandBuffer.SetGlobalVectorArray(
            visibleLightAttenuationsId, visibleLightAttenuations
        );
        _commandBuffer.SetGlobalVectorArray(
            visibleLightSpotDirectionsId, visibleLightSpotDirections
        );
        ExecuteCommandBuffer();
    }

    private void ConfigureLights()
    {
        int i = 0;
        for (; i < _cullingResult.visibleLights.Length; i++) {
            if (i == maxVisibleLights) {
                break;
            }
            VisibleLight light = _cullingResult.visibleLights[i];
            visibleLightColors[i] = light.finalColor;
            Vector4 attenuation = Vector4.zero;
            attenuation.w = 1f;
            if (light.lightType == LightType.Directional)
            {
                Vector4 v = light.localToWorldMatrix.GetColumn(2);
                v.x = -v.x;
                v.y = -v.y;
                v.z = -v.z;
                visibleLightDirectionsOrPositions[i] = v;
            }
            else
            {
                visibleLightDirectionsOrPositions[i] = light.localToWorldMatrix.GetColumn(3);
                attenuation.x = 1f / Mathf.Max(light.range * light.range, 0.00001f);

                if (light.lightType == LightType.Spot) {
                    Vector4 v = light.localToWorldMatrix.GetColumn(2);
                    v.x = -v.x;
                    v.y = -v.y;
                    v.z = -v.z;
                    visibleLightSpotDirections[i] = v;
                    float outerRad = Mathf.Deg2Rad * 0.5f * light.spotAngle;
                    float outerCos = Mathf.Cos(outerRad);
                    float outerTan = Mathf.Tan(outerRad);
                    float innerCos = Mathf.Cos(Mathf.Atan(46f / 64f * outerTan));
                    float angleRange = Mathf.Max(innerCos - outerCos, 0.001f);
                    attenuation.z = 1f / angleRange;
                    attenuation.w = -outerCos * attenuation.z;
                }
            }
            
            visibleLightAttenuations[i] = attenuation;
        }
        for (; i < maxVisibleLights; i++) {
            visibleLightColors[i] = Color.clear;
        }
    }

    private void Submit()
    {
        _commandBuffer.EndSample(SampleName);
        ExecuteCommandBuffer();
        _context.Submit();
    }
    private void ExecuteCommandBuffer()
    {
        _context.ExecuteCommandBuffer(_commandBuffer);
        _commandBuffer.Clear();
    }
    
    private bool Cull(out ScriptableCullingParameters parameters)
    {
        return _camera.TryGetCullingParameters(out parameters);
    }
    
    private DrawingSettings CreateDrawingSettings(List<ShaderTagId> shaderTags, SortingCriteria
        sortingCriteria, out SortingSettings sortingSettings)
    {
        sortingSettings = new SortingSettings(_camera)
        {
            criteria = sortingCriteria,
        };
        var drawingSettings = new DrawingSettings(shaderTags[0], sortingSettings);

        for (var i = 1; i < shaderTags.Count; i++)
        {
            drawingSettings.SetShaderPassName(i, shaderTags[i]);
        }

        drawingSettings.enableDynamicBatching = _dynamicBatching;
        drawingSettings.enableInstancing = _instancing;
        return drawingSettings;
    }
    
    private void DrawGizmos()
    {
        if (!Handles.ShouldRenderGizmos())
        {
            return;
        }
        _context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
        _context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
    }

    private void DrawUI()
    {
        _context.DrawUIOverlay(_camera);
    }
}
