using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BytingLib
{
    public class OnDispose : IDisposable
    {
        private readonly Action onDisposeAction;

        public OnDispose(Action onDisposeAction)
        {
            this.onDisposeAction = onDisposeAction ?? throw new ArgumentNullException(nameof(onDisposeAction));
        }

        public void Dispose()
        {
            onDisposeAction.Invoke();
        }
    }
}
