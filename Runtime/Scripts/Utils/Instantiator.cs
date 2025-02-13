using System;

namespace HotChocolate.Utils
{
    public class Instantiator
    {
        public string displayName;
        public Type type;
        public delegate object InstantiateDelegate();
        public InstantiateDelegate Instantiate;
    }
}
