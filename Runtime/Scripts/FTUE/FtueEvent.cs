using HotChocolate.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HotChocolate.FTUE
{
    public interface IFtueEvent
    {
        event Action<IFtueEvent> OnTriggered;

        string Id { get; }

        void Init(object data, IFtueListener listener);
        bool ConditionIsTrue();
        Task<IFtueResult> Execute(IFtueResult result, object data, IFtueListener listener, CancellationToken ct);
        void Dispose();

        JToken ToJson();
        void FromJson(JToken jToken);

        IEnumerable<Instantiator> StepsInstantiators();
    }

    public interface IFtueConditionHolder
    {
        IEnumerable<Instantiator> ConditionsInstantiators();
    }
    
    public abstract class FtueEvent<TData, TResult> : ScriptableObject, IFtueEvent, IFtueConditionHolder where TData : class where TResult : class, IFtueResult
    {
        public event Action<IFtueEvent> OnTriggered;

#if UNITY_2020_1_OR_NEWER
        [SerializeReference]
#endif
        public FtueCompositeCondition condition;
#if UNITY_2020_1_OR_NEWER
        [SerializeReference]
#endif
        public FtueSequence sequence;

        public string Id => name;

        public static TData Data(object data) => data as TData;
        public static TResult Result(object result) => result as TResult;

        public abstract IEnumerable<Instantiator> ConditionsInstantiators();
        public abstract IEnumerable<Instantiator> StepsInstantiators();

        public virtual JToken MetaToJson() { return new JObject(); }
        public virtual void MetaFromJson(JToken jToken) { }

        private void OnEnable()
        {
            if (condition == null)
                condition = new FtueCompositeCondition();

            if (sequence == null)
                sequence = new FtueSequence();
        }

        public void Init(TData data, IFtueListener listener)
        {
            if (condition != null)
            {
                condition.Init(data, listener);
                condition.OnBecameTrue -= OnConditionBecameTrue;
                condition.OnBecameTrue += OnConditionBecameTrue;
            }
        }

        public void Init(object data, IFtueListener listener)
        {
            Init(data as TData, listener);
        }

        public bool ConditionIsTrue()
        {
            if (condition != null)
                return condition.IsTrue();

            return false;
        }

        private void OnConditionBecameTrue(IFtueCondition c)
        {
            if(ConditionIsTrue())
                OnTriggered?.Invoke(this);
        }

        public async Task<IFtueResult> Execute(IFtueResult result, object data, IFtueListener listener, CancellationToken ct)
        {
            if (sequence != null)
            {
                result = await sequence.Execute(result, data, listener, ct).ConfigureAwait(true);
            }

            return result;
        }

        public void Dispose()
        {
            if(condition != null)
            {
                condition.Dispose();
                condition.OnBecameTrue -= OnConditionBecameTrue;
            }
        }

        public JToken ToJson()
        {
            var data = new JObject()
            {
                { "condition", condition.ToJson() },
                { "sequence", sequence.ToJson() },
                { "meta", MetaToJson() }
            };

            return new JObject()
            {
                { "type", GetType().FullName },
                { "assembly", GetType().Assembly.FullName },
                { "data", data }
            };
        }

        public void FromJson(JToken jToken)
        {
            if (condition == null)
                condition = new FtueCompositeCondition();

            if (sequence == null)
                sequence = new FtueSequence();

            var data = jToken["data"];

            condition.FromJson(data["condition"]);
            sequence.FromJson(data["sequence"]);
            MetaFromJson(data["meta"]);
        }
    }
}
