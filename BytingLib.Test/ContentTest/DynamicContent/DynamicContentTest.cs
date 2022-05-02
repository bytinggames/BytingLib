﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using BytingLib;
using System.Collections.Generic;

namespace BytingLib.Test.ContentTest
{
    [TestClass]
    public class DynamicContentTest
    {
        [TestMethod]
        public void TestDynamicTexturesDisposalResizeAndDelete()
        {
            string enemyPng = @"Content\Textures\Enemy.png";
            File.Delete(enemyPng);

            CreateGame(out ContentCollector collector, out DynamicContent dynamicContent);

            List<Texture2D> loadedTextures = new();

            using (var playerTex = collector.Use<Texture2D>("Textures/Player"))
            using (var enemyTex = collector.Use<Texture2D>("Textures/Enemy"))
            {
                Color[] playerColors = playerTex.Value.ToColor();
                Color[] enemyColors = enemyTex.Value.ToColor();
                Texture2D enemyTexRaw = enemyTex.Value;
                loadedTextures.Add(enemyTex.Value);
                loadedTextures.Add(playerTex.Value);

                Assert.IsFalse(enemyTexRaw.IsDisposed);
                Assert.That.AreNotEqualItems(playerColors, enemyColors);

                // save player tex as Enemy.png, so that Enemy looks like player
                using (var fs = File.Create(enemyPng))
                    playerTex.Value.SaveAsPng(fs, playerTex.Value.Width, playerTex.Value.Height);
                dynamicContent.UpdateChanges();

                Color[] enemyColorsTemp = enemyTex.Value.ToColor();

                Assert.IsTrue(enemyTexRaw.IsDisposed);
                Assert.That.AreEqualItems(playerColors, enemyColorsTemp);
                enemyTexRaw = enemyTex.Value;
                loadedTextures.Add(enemyTex.Value);
                Assert.IsFalse(enemyTexRaw.IsDisposed);

                // delete Enemy.png, so that Enemy looks like enemy again
                File.Delete(enemyPng);
                dynamicContent.UpdateChanges();
                Assert.IsTrue(enemyTexRaw.IsDisposed);
                Color[] enemyColorsNew = enemyTex.Value.ToColor();
                Assert.That.AreEqualItems(enemyColors, enemyColorsNew);
                loadedTextures.Add(enemyTex.Value);
            }

            for (int i = 0; i < loadedTextures.Count; i++)
            {
                Assert.IsTrue(loadedTextures[i].IsDisposed, $"loadedTextures[{i}] was not disposed");
            }
        }

        private static void CreateGame(out ContentCollector collector, out DynamicContent dynamicContent)
        {
            Game game = new Game();
            new GraphicsDeviceManager(game); // must be called to initialize GraphicsDevice
            game.RunOneFrame(); // must be called to initialize the game

            ContentManagerRaw rawContent = new ContentManagerRaw(game.Services, "Content");
            collector = new ContentCollector(rawContent);
            dynamicContent = new DynamicContent(game.Services, collector, "Content");
        }

        [TestMethod]
        public void TestDynamicFontsDisposalResizeAndDelete()
        {
            string font2File = @"Content\Fonts\Font2.spritefont";
            string font1ResourceFile = @"ContentTest\DynamicContent\Resources\Font1.spritefont";
            File.Delete(font2File);

            CreateGame(out ContentCollector collector, out DynamicContent dynamicContent);

            List<SpriteFont> loadedFonts = new();

            using (var font1 = collector.Use<SpriteFont>("Fonts/Font1"))
            using (var font2 = collector.Use<SpriteFont>("Fonts/Font2"))
            {
                int font1TexHeight = font1.Value.Texture.Height;
                int font2TexHeight = font2.Value.Texture.Height;
                SpriteFont font2Raw = font2.Value;
                loadedFonts.Add(font1.Value);
                loadedFonts.Add(font2.Value);

                Assert.IsFalse(font2Raw.Texture.IsDisposed);
                Assert.AreNotEqual(font1TexHeight, font2TexHeight);

                // save player tex as Enemy.png, so that Enemy looks like player
                File.Copy(font1ResourceFile, font2File);
                dynamicContent.UpdateChanges();

                int font2TexHeightTemp = font2.Value.Texture.Height;

                Assert.IsTrue(font2Raw.Texture.IsDisposed);
                Assert.AreEqual(font1TexHeight, font2TexHeightTemp);
                font2Raw = font2.Value;
                loadedFonts.Add(font2.Value);
                Assert.IsFalse(font2Raw.Texture.IsDisposed);

                // delete Enemy.png, so that Enemy looks like enemy again
                File.Delete(font2File);
                dynamicContent.UpdateChanges();
                Assert.IsTrue(font2Raw.Texture.IsDisposed);
                int font2TexHeightNew = font2.Value.Texture.Height;
                Assert.AreEqual(font2TexHeight, font2TexHeightNew);
                loadedFonts.Add(font2.Value);
            }

            for (int i = 0; i < loadedFonts.Count; i++)
            {
                Assert.IsTrue(loadedFonts[i].Texture.IsDisposed, $"loadedFonts[{i}].Texture was not disposed");
            }
        }

    }
}
