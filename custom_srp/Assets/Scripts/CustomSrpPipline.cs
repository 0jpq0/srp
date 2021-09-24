using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomSrpPipline : RenderPipeline
{
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
            Render(context, camera);
    }

    void Render(ScriptableRenderContext context, Camera camera)
    {
        if (!camera.TryGetCullingParameters(out var cullingParameters))
            return;

        var cull = context.Cull(ref cullingParameters);

        context.SetupCameraProperties(camera);

        var buffer = new CommandBuffer { name = camera.name };

        var clearFlags = camera.clearFlags;

        buffer.ClearRenderTarget(
            (clearFlags & CameraClearFlags.Depth) != 0,
            (clearFlags & CameraClearFlags.Color) != 0,
            camera.backgroundColor);

        context.ExecuteCommandBuffer(buffer);
        buffer.Release();

        var sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };
        var drawSettings = new DrawingSettings(new ShaderTagId("SRPDefaultUnlit"), sortingSettings);

        var filterSettings = new FilteringSettings(RenderQueueRange.opaque);
        context.DrawRenderers(cull, ref drawSettings, ref filterSettings);

        context.DrawSkybox(camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawSettings.sortingSettings = sortingSettings;
        filterSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(cull, ref drawSettings, ref filterSettings);

        context.Submit();
    }
}
