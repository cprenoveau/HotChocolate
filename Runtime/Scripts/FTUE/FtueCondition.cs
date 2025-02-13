using Newtonsoft.Json.Linq;
using System;
using System.Reflection;

namespace HotChocolate.FTUE
{
    public interface IFtueCondition
    {
        event Action<IFtueCondition> OnBecameTrue;
        event Action<IFtueCondition> OnBecameFalse;

        void Init(object data, IFtueListener listener);
        bool IsTrue();
        void Refresh();
        void Dispose();

        JToken ToJson();
        void FromJson(JToken jToken);
    }

    [Serializable]
    public abstract class FtueCondition : IFtueCondition
    {
        public bool invert;
        public bool alwaysTrigger;

        public event Action<IFtueCondition> OnBecameTrue;
        public event Action<IFtueCondition> OnBecameFalse;

        protected object data;
        protected IFtueListener listener;

        private bool _init;
        private bool? _wasTrue;

        public void Init(object data, IFtueListener listener)
        {
            this.data = data;
            this.listener = listener;

            _Init();
            _init = true;
        }

        public bool IsTrue()
        {
            return _IsTrue() ^ invert;
        }

        public void Refresh()
        {
            bool isTrue = IsTrue();

            if(alwaysTrigger)
            {
                if(isTrue) OnBecameTrue?.Invoke(this);
            }
            else if (!_wasTrue.HasValue || isTrue != _wasTrue.Value)
            {
                _wasTrue = isTrue;

                if (isTrue) OnBecameTrue?.Invoke(this);
                else OnBecameFalse?.Invoke(this);
            }
        }

        public void Dispose()
        {
            if(_init) _Dispose();
        }

        public JToken ToJson()
        {
            var jObject = new JObject()
            {
                { "type", GetType().FullName },
                { "assembly", GetType().Assembly.FullName },
                { "invert", invert },
                { "alwaysTrigger", alwaysTrigger },
                { "data", _ToJson() }
            };

            return jObject;
        }

        public static IFtueCondition InstantiateFromJson(JToken jToken)
        {
            var typeName = jToken["type"].ToString();
            var assemblyName = jToken["assembly"].ToString();

            var assembly = Assembly.Load(assemblyName);
            var instance = (IFtueCondition)assembly.CreateInstance(typeName);

            instance.FromJson(jToken);
            return instance;
        }

        public void FromJson(JToken jToken)
        {
            invert = jToken["invert"].ToObject<bool>();
            alwaysTrigger = jToken["alwaysTrigger"].ToObject<bool>();

            _FromJson(jToken["data"]);
        }

        protected abstract void _Init();
        protected abstract bool _IsTrue();
        protected abstract void _Dispose();

        protected virtual JToken _ToJson() { throw new NotImplementedException(); }
        protected virtual void _FromJson(JToken jToken) { throw new NotImplementedException(); }
    }
}
