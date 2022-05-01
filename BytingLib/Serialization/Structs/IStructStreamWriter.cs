using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BytingLib
{
    public interface IStructStreamWriter<T> : IDisposable where T : struct
    {
        void AddState(T state);
    }
}
