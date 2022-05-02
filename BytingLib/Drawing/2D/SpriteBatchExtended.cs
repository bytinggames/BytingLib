using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BytingLib
{

    public class SpriteBatchExtended : SpriteBatch
    {
        public Texture2D PixelTexture { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public SpriteBatchExtended(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            Initialize();
        }

        public SpriteBatchExtended(GraphicsDevice graphicsDevice, int capacity) : base(graphicsDevice, capacity)
        {
            Initialize();
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        private void Initialize()
        {
            PixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            PixelTexture.SetData<Color>(new Color[] { Color.White });
        }

        public void DrawRectangle(Rect rect, Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawPolygon(Polygon polygon, Color color)
        {
            //TODO is it possible to draw a polygon with SpriteBatch?
        }

        // TODO: probably do something like:
        //public void DrawPolygon(Polygon polygon, DrawArguments args)
        //{

        //}

        public void DrawCircle(Circle circle, Color color)
        {
            throw new NotImplementedException();
        }
    }
}
