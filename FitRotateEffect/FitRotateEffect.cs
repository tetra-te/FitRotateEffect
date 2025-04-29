using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;

namespace FitRotateEffect
{
    [VideoEffect("フィット回転", ["描画"], ["fit rotate"], IsAviUtlSupported = false)]
    class FitRotateEffect : VideoEffectBase
    {
        public override string Label => $"フィット回転 {Rotate.GetValue(0, 1, 30):F1}°";

        [Display(GroupName = "フィット回転", Name = "回転角", Description = "回転させる角度（右回り）")]
        [AnimationSlider("F1", "°", -360, 360)]
        public Animation Rotate { get; } = new Animation(0, -36000, 36000);

        public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription)
        {
            return [];
        }

        public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        {
            return new FitRotateEffectProcessor(devices, this);
        }

        protected override IEnumerable<IAnimatable> GetAnimatables() => [Rotate];
    }
}