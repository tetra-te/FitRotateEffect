using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace FitRotateEffect
{
    class FitRotateEffectProcessor : IVideoEffectProcessor
    {
        readonly IGraphicsDevicesAndContext devices;

        readonly FitRotateEffect item;  

        ID2D1Image? input;

        public ID2D1Image Output => input ?? throw new NullReferenceException(nameof(input) + " is null");

        public FitRotateEffectProcessor(IGraphicsDevicesAndContext devices, FitRotateEffect item)
        {
            this.devices = devices;
            this.item = item;
        }

        public DrawDescription Update(EffectDescription effectDescription)
        {
            var drawDesc = effectDescription.DrawDescription;

            var frame = effectDescription.ItemPosition.Frame;
            var length = effectDescription.ItemDuration.Frame;
            var fps = effectDescription.FPS;

            var rotate = item.Rotate.GetValue(frame, length, fps);
            var rotateRad = rotate * Math.PI / 180;

            var inputBounds = devices.DeviceContext.GetImageLocalBounds(input);
            var width = inputBounds.Right - inputBounds.Left;
            var height = inputBounds.Bottom - inputBounds.Top;

            if (width == 0 || height == 0)
                return drawDesc;

            var cos = Math.Abs(Math.Cos(rotateRad));
            var sin = Math.Abs(Math.Sin(rotateRad));

            var rotatedWidth = width * cos + height * sin;
            var rotatedHeight = width * sin + height * cos;

            var scale = Math.Max(rotatedWidth / width, rotatedHeight / height);

            return
                drawDesc with
                {
                    Rotation = new(
                        drawDesc.Rotation.X,
                        drawDesc.Rotation.Y,
                        drawDesc.Rotation.Z + (float)rotate),
                    Zoom = drawDesc.Zoom * (float)scale
                };
        }

        public void SetInput(ID2D1Image? input)
        {
            this.input = input;
        }

        public void ClearInput()
        {
            input = null;
        }

        public void Dispose()
        {
        }     
    }
}