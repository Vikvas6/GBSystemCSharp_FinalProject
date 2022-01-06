using UnityEngine;
using UnityEngine.Rendering;


public class CustomPipelineRender : RenderPipeline
{
    private CameraRenderer _cameraRenderer = new CameraRenderer();
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        CameraRender(context, cameras);
    }

    private void CameraRender(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            _cameraRenderer.Render(context, camera);
        }
    }
}
