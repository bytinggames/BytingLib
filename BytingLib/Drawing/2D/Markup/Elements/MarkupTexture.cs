using BytingLib.Creation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BytingLib.Markup
{
    [MarkupShortcut("tex")]
    public class MarkupTexture : MarkupBlock, IDisposable
    {
        public Rectangle BoundingRectangle { get; }

        public Ref<Texture2D> Texture { get; }

        public Color Color { get; set; } = Color.White;
        public Rectangle? SourceRectangle { get; set; } = null;

        public Vector2 ScaleXY { get; set; } = Vector2.One;

        public SpriteEffects Effects { get; set; } = SpriteEffects.None;

        public float Scale
        {
            set => ScaleXY = new Vector2(value);
        }

        public MarkupTexture(IContentCollector content, string texName)
        {
            Texture = content.Use<Texture2D>("Textures/" + texName);
        }

        public void Flip()
        {
            Effects = Effects ^ (SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically);
        }
        public void FlipX()
        {
            Effects = Effects ^ SpriteEffects.FlipHorizontally;
        }
        public void FlipY()
        {
            Effects = Effects ^ SpriteEffects.FlipVertically;
        }
        public void FlipNone()
        {
            Effects = SpriteEffects.None;
        }

        protected override Vector2 GetSizeChildUnscaled(MarkupSettings settings)
        {
            return GetSizeChildUnscaledInternal(settings) * ScaleXY * settings.TextureScale;
        }

        private Vector2 GetSizeChildUnscaledInternal(MarkupSettings settings)
        {
            if (SourceRectangle == null)
                return Texture.Value.GetSize();
            else
                return SourceRectangle.Value.Size.ToVector2();
        }

        protected override void DrawChild(MarkupSettings settings)
        {
            SpriteEffects flip = settings.Effects ^ Effects;
            //flip = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
            //switch (Effects)
            //{
            //    case SpriteEffects.FlipHorizontally:
            //        switch (flip)
            //        {
            //            case SpriteEffects.None:
            //                flip = SpriteEffects.FlipHorizontally;
            //                break;
            //            case SpriteEffects.FlipHorizontally:
            //                flip = SpriteEffects.None;
            //                break;
            //            case SpriteEffects.FlipVertically:
            //                throw new NotImplementedException();
            //        }
            //        break;
            //    case SpriteEffects.FlipVertically:
            //        switch (flip)
            //        {
            //            case SpriteEffects.None:
            //                flip = SpriteEffects.FlipVertically;
            //                break;
            //            case SpriteEffects.FlipHorizontally:
            //                throw new NotImplementedException();
            //            case SpriteEffects.FlipVertically:
            //                flip = SpriteEffects.None;
            //                break;
            //        }
            //        break;
            //}
            Texture.Value.Draw(settings.SpriteBatch, settings.Anchor, ColorExtension.MultiplyColors(settings.TextureColor, Color), SourceRectangle, settings.Scale * ScaleXY * settings.TextureScale, settings.Rotation, flip);
        }

        public override string ToString()
        {
            return "tex: " + Texture.Value.Name;
        }

        public override void Dispose()
        {
            Texture.Dispose();

            base.Dispose();
        }
    }
}
