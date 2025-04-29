using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Player.Video.Effects;

namespace FitRotateEffect
{
    class FitRotateEffectProcessor(IGraphicsDevicesAndContext devices, FitRotateEffect item) : VideoEffectProcessorBase(devices)
    {
        AffineTransform2D? transformEffect;
        Crop? cropEffect;

        bool isFirst = true;
        Matrix3x2 transformMatrix;
        Vector4 cropRect;
        
        public override DrawDescription Update(EffectDescription effectDescription)
        {
            if (transformEffect is null || cropEffect is null)
                return effectDescription.DrawDescription;
            
            var frame = effectDescription.ItemPosition.Frame;
            var length = effectDescription.ItemDuration.Frame;
            var fps = effectDescription.FPS;
            
            var rotate = item.Rotate.GetValue(frame, length, fps) * Math.PI / 180;

            var inputBounds = devices.DeviceContext.GetImageLocalBounds(input);
            var width = inputBounds.Right - inputBounds.Left;
            var height = inputBounds.Bottom - inputBounds.Top;

            Matrix3x2 transformMatrix;
            Vector4 cropRect;
            
            if (width != 0 && height != 0)
            {
                var cos = Math.Abs(Math.Cos(rotate));
                var sin = Math.Abs(Math.Sin(rotate));

                var rotatedWidth = width * cos + height * sin;
                var rotatedHeight = width * sin + height * cos;

                var scale = Math.Max(rotatedWidth / width, rotatedHeight / height);

                var imageX = (inputBounds.Left + inputBounds.Right) / 2;
                var imageY = (inputBounds.Top + inputBounds.Bottom) / 2;

                var imagePosition = new Vector2(imageX, imageY);

                var translationMatrix1 = Matrix3x2.CreateTranslation(-imagePosition);
                var translationMatrix2 = Matrix3x2.CreateTranslation(imagePosition);
                var rotationMatrix = Matrix3x2.CreateRotation((float)rotate);
                var scaleMatrix = Matrix3x2.CreateScale((float)scale);

                transformMatrix = translationMatrix1 * rotationMatrix * scaleMatrix * translationMatrix2;

                cropRect = item.FixSize ?
                    new Vector4(inputBounds.Left, inputBounds.Top, inputBounds.Right, inputBounds.Bottom) :
                    new Vector4(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue);
            }
            else
            {
                transformMatrix = Matrix3x2.Identity;
                cropRect = new Vector4(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue);
            }
            
            if (isFirst || this.transformMatrix != transformMatrix)
            {
                transformEffect.TransformMatrix = transformMatrix;
            }
            if (isFirst || this.cropRect != cropRect)
            {
                cropEffect.Rectangle = cropRect;
            }

            this.transformMatrix = transformMatrix;
            this.cropRect = cropRect;
            isFirst = false;

            return effectDescription.DrawDescription;
        }

        protected override void setInput(ID2D1Image? input)
        {
            transformEffect?.SetInput(0, input, true);
        }

        protected override ID2D1Image? CreateEffect(IGraphicsDevicesAndContext devices)
        {
            transformEffect = new AffineTransform2D(devices.DeviceContext);
            disposer.Collect(transformEffect);
            cropEffect = new Crop(devices.DeviceContext);
            disposer.Collect(cropEffect);
            using (var image = transformEffect.Output)
            {
                cropEffect.SetInput(0, image, true);
            }
            var output = cropEffect.Output;
            disposer.Collect(output);
            return output;
        }

        protected override void ClearEffectChain()
        {
            transformEffect?.SetInput(0, null, true);
            cropEffect?.SetInput(0, null, true);
        }
    }
}