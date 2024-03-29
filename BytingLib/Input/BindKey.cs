﻿using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class BindKey
    {
        public List<Keys> Keys { get; set; }
        public List<MouseButton> MouseButtons { get; set; }
        public List<Buttons> GamePadButtons { get; set; }
        public List<Keys[]> KeyCombos { get; set; } = new();

        public BindKey(List<Keys>? keys, List<MouseButton>? mouseButtons, List<Buttons>? gamePadButtons)
        {
            Keys = keys ?? new();
            MouseButtons = mouseButtons ?? new();
            GamePadButtons = gamePadButtons ?? new();
        }

        public static BindKey FromKeys(params Keys[] keys)
        {
            return new BindKey(keys.ToList(), null, null);
        }
        public static BindKey FromMouseButtons(params MouseButton[] mouseButtons)
        {
            return new BindKey(null, mouseButtons.ToList(), null);
        }
        public static BindKey FromGamePad(params Buttons[] buttons)
        {
            return new BindKey(null, null, buttons.ToList());
        }

        public IKey GetKey(AllInput input)
        {
            IKey? key = null;
            if (Keys.Count > 0)
            {
                AddKey(input.Keys.GetKeyAnyFromIList(Keys));
            }
            if (MouseButtons.Count > 0)
            {
                AddKey(input.Mouse.GetKeyAnyFromIList(MouseButtons));
            }
            if (GamePadButtons.Count > 0)
            {
                AddKey(input.GamePad.GetKeyAnyFromIList(GamePadButtons));
            }
            if (KeyCombos.Count > 0)
            {
                foreach (var combo in KeyCombos)
                {
                    if (combo.Length == 0)
                    {
                        continue;
                    }

                    // check if all but the last key are held down
                    int i;
                    for (i = 0; i < combo.Length - 1; i++)
                    {
                        if (!input.Keys.GetState().IsKeyDown(combo[i]))
                        {
                            break;
                        }
                    }

                    if (i == combo.Length - 1)
                    {
                        AddKey(input.Keys.GetKey(combo.Last()));
                    }
                }
            }

            if (key == null)
            {
                return new Key();
            }
            return key;

            void AddKey(IKey newKey)
            {
                if (key == null)
                {
                    key = newKey;
                }
                else
                {
                    key = key.Or(newKey);
                }
            }
        }
    }
}