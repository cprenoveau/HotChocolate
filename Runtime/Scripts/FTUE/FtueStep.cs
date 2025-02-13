using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HotChocolate.FTUE
{
    public interface IFtueResult
    {
        bool Failed { get; }
    }

    public interface IFtueStep
    {
        string Info { get; }

        Task<IFtueResult> Execute(IFtueResult currentResult, object data, IFtueListener listener, CancellationToken ct);
        JToken ToJson();
        void FromJson(JToken jToken);
    }

    public abstract class FtueStep : IFtueStep
    {
        public virtual string Info => $"{GetType().Name}";

        public async Task<IFtueResult> Execute(IFtueResult currentResult, object data, IFtueListener listener, CancellationToken ct)
        {
            return await _Execute(currentResult, data, listener, ct).ConfigureAwait(true);
        }

        public JToken ToJson()
        {
            var jObject = new JObject()
            {
                { "type", GetType().FullName },
                { "assembly", GetType().Assembly.FullName },
                { "data", _ToJson() }
            };

            return jObject;
        }

        public static IFtueStep InstantiateFromJson(JToken jToken)
        {
            var typeName = jToken["type"].ToString();
            var assemblyName = jToken["assembly"].ToString();

            var assembly = Assembly.Load(assemblyName);
            var instance = (IFtueStep)assembly.CreateInstance(typeName);

            instance.FromJson(jToken);
            return instance;
        }

        public void FromJson(JToken jToken)
        {
            _FromJson(jToken["data"]);
        }

        protected abstract Task<IFtueResult> _Execute(IFtueResult currentResult, object data, IFtueListener listener, CancellationToken ct);
        protected virtual JToken _ToJson() { throw new NotImplementedException(); }
        protected virtual void _FromJson(JToken jToken) { throw new NotImplementedException(); }
    }
}
