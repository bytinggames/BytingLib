namespace BytingLib
{
    public static class EffectParameterExtension
    {
        public static void SetValueObject(this EffectParameter p, object val)
        {
            switch (p.ParameterType)
            {
                case EffectParameterType.Bool:
                    p.SetValue((bool)val);
                    break;
                case EffectParameterType.Int32:
                    if (p.Elements.Count == 0)
                        p.SetValue((int)val);
                    else
                        p.SetValue((int[])val);
                    break;
                case EffectParameterType.Single:
                    if (p.Elements.Count == 0)
                    {
                        if (p.ColumnCount == 1 && p.RowCount == 1)
                            p.SetValue((float)val);
                        else if (p.RowCount == 1)
                        {
                            switch (p.ColumnCount)
                            {
                                case 2:
                                    p.SetValue((Vector2)val);
                                    break;
                                case 3:
                                    p.SetValue((Vector3)val);
                                    break;
                                case 4:
                                    p.SetValue((Vector4)val);
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        else if (val is Matrix m)
                        {
                            p.SetValue(m);
                        }
                        else if (val is Matrix[] matrixArray)
                        {
                            p.SetValue(matrixArray[0]);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        if (p.ColumnCount == 1 && p.RowCount == 1)
                            p.SetValue((float[])val);
                        else if (p.RowCount == 1)
                        {
                            switch (p.ColumnCount)
                            {
                                case 2:
                                    p.SetValue((Vector2[])val);
                                    break;
                                case 3:
                                    p.SetValue((Vector3[])val);
                                    break;
                                case 4:
                                    p.SetValue((Vector4[])val);
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        else
                            p.SetValue((Matrix[])val);
                    }
                    break;
                case EffectParameterType.Texture:
                case EffectParameterType.Texture2D:
                    p.SetValue((Texture2D)val);
                    break;
                case EffectParameterType.Texture3D:
                    p.SetValue((Texture3D)val);
                    break;
                case EffectParameterType.TextureCube:
                    p.SetValue((TextureCube)val);
                    break;
                case EffectParameterType.Texture1D:
                default:
                    throw new NotImplementedException();
                case EffectParameterType.Void:
                    break;
                case EffectParameterType.String:
                    // there is no set string...
                    //effectParameter.SetValue(effectParameter.GetValueString());
                    break;
                    //effectParameter.SetValue(p.GetValueQuaternion()); // I think this is the same as Vector4, so no need to read it separately
            }
        }
    }
}
