using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HotChocolate.FTUE
{
    public enum FtueConditionUnion
    {
        And,
        Or
    }

    [Serializable]
    public class FtueCompositeCondition : FtueCondition
    {
        public FtueConditionUnion union;

#if UNITY_2020_1_OR_NEWER
        [SerializeReference]
#endif
        public List<IFtueCondition> conditions = new List<IFtueCondition>();

        private void OnConditionChanged(IFtueCondition c)
        {
            Refresh();
        }

        protected override void _Init()
        {
            foreach (var condition in conditions)
            {
                condition.Init(data, listener);
                condition.OnBecameTrue -= OnConditionChanged;
                condition.OnBecameTrue += OnConditionChanged;
                condition.OnBecameFalse -= OnConditionChanged;
                condition.OnBecameFalse += OnConditionChanged;
            }
        }

        protected override void _Dispose()
        {
            foreach (var condition in conditions)
            {
                condition.Dispose();
            }
        }

        protected override bool _IsTrue()
        {
            switch (union)
            {
                case FtueConditionUnion.And: return AllConditionsAreTrue();
                case FtueConditionUnion.Or: return AtLeastOneConditionIsTrue();
                default: return false;
            }
        }

        protected override JToken _ToJson()
        {
            var jArray = new JArray();
            foreach (var condition in conditions)
            {
                jArray.Add(condition.ToJson());
            }

            return new JObject()
            {
                { "union", union.ToString() },
                { "conditions", jArray }
            };
        }

        protected override void _FromJson(JToken jToken)
        {
            conditions.Clear();

            var jArray = jToken["conditions"].ToObject<JArray>();
            foreach (var condition in jArray)
            {
                conditions.Add(InstantiateFromJson(condition));
            }

            union = (FtueConditionUnion)Enum.Parse(typeof(FtueConditionUnion), jToken["union"].ToString(), true);
        }

        public bool AllConditionsAreTrue()
        {
            foreach (var condition in conditions)
            {
                if (!condition.IsTrue())
                    return false;
            }

            return true;
        }

        public bool AtLeastOneConditionIsTrue()
        {
            foreach (var condition in conditions)
            {
                if (condition.IsTrue())
                    return true;
            }

            return false;
        }
    }
}
