using Microsoft.Xna.Framework.Graphics;
using System;

namespace BytingLib
{
    public static class EffectExtension
    {
        public static void CopyParametersTo(this Effect effect, Effect effectTarget)
        {
            var parametersSource = effect.Parameters;
            var parametersTarget = effectTarget.Parameters;
            foreach (var p in parametersSource)
            {
                var p2 = parametersTarget[p.Name];
                if (p2 == null) // removed parameter
                    continue;
                switch (p.ParameterType)
                {
                    case EffectParameterType.Bool:
                        p2.SetValue(p.GetValueBoolean());
                        break;
                    case EffectParameterType.Int32:
                        if (p.Elements.Count == 0)
                            p2.SetValue(p.GetValueInt32());
                        else
                            p2.SetValue(p.GetValueInt32Array());
                        break;
                    case EffectParameterType.Single:
                        if (p.Elements.Count == 0)
                        {
                            if (p.ColumnCount == 1 && p.RowCount == 1)
                                p2.SetValue(p.GetValueSingle());
                            else if (p.RowCount == 1)
                            {
                                switch (p.ColumnCount)
                                {
                                    case 2:
                                        p2.SetValue(p.GetValueVector2());
                                        break;
                                    case 3:
                                        p2.SetValue(p.GetValueVector3());
                                        break;
                                    case 4:
                                        p2.SetValue(p.GetValueVector4());
                                        break;
                                    default:
                                        throw new NotImplementedException();
                                }
                            }
                            else
                                p2.SetValue(p.GetValueMatrix());
                        }
                        else
                        {
                            if (p.ColumnCount == 1 && p.RowCount == 1)
                                p2.SetValue(p.GetValueSingleArray());
                            else if (p.RowCount == 1)
                            {
                                switch (p.ColumnCount)
                                {
                                    case 2:
                                        p2.SetValue(p.GetValueVector2Array());
                                        break;
                                    case 3:
                                        p2.SetValue(p.GetValueVector3Array());
                                        break;
                                    case 4:
                                        p2.SetValue(p.GetValueVector4Array());
                                        break;
                                    default:
                                        throw new NotImplementedException();
                                }
                            }
                            else
                                p2.SetValue(p.GetValueMatrixArray(p.Elements.Count));
                        }
                        break;
                    case EffectParameterType.Texture:
                    case EffectParameterType.Texture2D:
                        p2.SetValue(p.GetValueTexture2D());
                        break;
                    case EffectParameterType.Texture3D:
                        p2.SetValue(p.GetValueTexture3D());
                        break;
                    case EffectParameterType.TextureCube:
                        p2.SetValue(p.GetValueTextureCube());
                        break;
                    case EffectParameterType.Texture1D:
                    default:
                        throw new NotImplementedException();
                    case EffectParameterType.Void:
                        break;
                    case EffectParameterType.String:
                        // there is no set string...
                        //p2.SetValue(p.GetValueString());
                        break;
                        //p2.SetValue(p.GetValueQuaternion()); // I think this is the same as Vector4, so no need to read it separately
                }
            }
        }

    }
}
