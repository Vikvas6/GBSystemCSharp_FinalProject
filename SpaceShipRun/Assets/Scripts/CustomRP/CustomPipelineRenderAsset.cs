using UnityEngine;
using UnityEngine.Rendering;


[CreateAssetMenu(menuName = "Rendering/CustomRenderPipelineAsset")]
public class CustomPipelineRenderAsset : RenderPipelineAsset
{
    [SerializeField] bool _dynamicBatching;
    [SerializeField] bool _instancing;
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomPipelineRender(_dynamicBatching, _instancing);
    }
}
