using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 지정 머티리얼로 화면 전체에 풀스크린 이펙트를 적용하되, '게임 카메라'에서만 동작한다.
/// 씬뷰/프리뷰 카메라는 제외 → 에디터 씬 화면에는 이펙트가 안 보인다.
/// (빌트인 Full Screen Pass Renderer Feature 대신 이걸 쓰면 됨. Unity 6 URP RenderGraph 기준)
/// </summary>
public class GameOnlyFullscreenFeature : ScriptableRendererFeature
{
    [SerializeField] Material material;
    [SerializeField] RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingPostProcessing;

    class BlitPass : ScriptableRenderPass
    {
        readonly Material _material;
        public BlitPass(Material m) { _material = m; }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_material == null) return;

            var cameraData = frameData.Get<UniversalCameraData>();
            if (cameraData.cameraType != CameraType.Game) return; // 씬뷰/프리뷰 제외

            var resourceData = frameData.Get<UniversalResourceData>();
            TextureHandle source = resourceData.activeColorTexture;

            TextureDesc destDesc = renderGraph.GetTextureDesc(source);
            destDesc.name = "FullscreenFX_Temp";
            destDesc.clearBuffer = false;
            TextureHandle dest = renderGraph.CreateTexture(destDesc);

            var blit = new RenderGraphUtils.BlitMaterialParameters(source, dest, _material, 0);
            renderGraph.AddBlitPass(blit, "GameOnly Fullscreen FX");

            resourceData.cameraColor = dest; // 이후 패스가 이걸 화면색으로 사용
        }
    }

    BlitPass _pass;

    public override void Create()
    {
        _pass = new BlitPass(material) { renderPassEvent = injectionPoint };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (material == null) return;
        renderer.EnqueuePass(_pass);
    }
}
