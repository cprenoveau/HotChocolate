using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace HotChocolate.FTUE
{
    public class FtueSequence : FtueStep
    {
#if UNITY_2020_1_OR_NEWER
        [SerializeReference]
#endif
        public List<IFtueStep> steps = new List<IFtueStep>();

        protected override async Task<IFtueResult> _Execute(IFtueResult result, object data, IFtueListener listener, CancellationToken ct)
        {
            foreach (var step in steps)
            {
                if (result.Failed)
                    return result;

                result = await step.Execute(result, data, listener, ct).ConfigureAwait(true);
            }

            return result;
        }

        protected override JToken _ToJson()
        {
            var jArray = new JArray();
            foreach(var step in steps)
            {
                jArray.Add(step.ToJson());
            }

            return new JObject()
            {
                { "steps", jArray }
            };
        }

        protected override void _FromJson(JToken jToken)
        {
            steps.Clear();

            var jArray = jToken["steps"].ToObject<JArray>();
            foreach(var step in jArray)
            {
                steps.Add(InstantiateFromJson(step));
            }
        }
    }
}
