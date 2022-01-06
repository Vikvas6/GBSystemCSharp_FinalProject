using UnityEngine;
using UnityEngine.Rendering;


public class CustomPipelineRender : RenderPipeline
{
    private CameraRenderer _cameraRenderer = new CameraRenderer();
    private bool _dynamicBatching;
    private bool _instancing;

    public CustomPipelineRender(bool dynamicBatching, bool instancing)
    {
        GraphicsSettings.lightsUseLinearIntensity = true;
        _dynamicBatching = dynamicBatching;
        _instancing = instancing;
    }
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        CameraRender(context, cameras);
    }

    private void CameraRender(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            _cameraRenderer.Render(context, camera, _dynamicBatching, _instancing);
        }
    }
}
