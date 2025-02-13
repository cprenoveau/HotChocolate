using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HotChocolate.World
{
    public interface ISceneData
    {
        string SceneAddress { get; }
        UnityEngine.SceneManagement.Scene Scene { get; }
    }

    public class DefaultSceneData : ISceneData
    {
        public string SceneAddress { get; set; }
        public UnityEngine.SceneManagement.Scene Scene => SceneManager.GetSceneByName(SceneNameFromPath(SceneAddress));

        public static string SceneNameFromPath(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
                return null;

            var tokens = scenePath.Split('/');
            if (tokens.Length > 0)
            {
                var filename = tokens.Last();
                return filename.Split('.')[0];
            }

            return null;
        }
    }

    public static partial class Scene
    {
        public delegate Task<ISceneData> Load(string sceneAddress, CancellationToken ct);
        public delegate Task Unload(ISceneData instance, CancellationToken ct);

        public static async Task<ISceneData> LoadSceneDefault(string sceneAddress, CancellationToken ct)
        {
            if (!ct.IsCancellationRequested)
            {
                var op = SceneManager.LoadSceneAsync(sceneAddress);
                while(!op.isDone && !ct.IsCancellationRequested)
                {
                    await Task.Yield();
                }
            }

            var sceneData = new DefaultSceneData
            {
                SceneAddress = sceneAddress
            };

            return sceneData;
        }

        public static async Task UnloadSceneDefault(ISceneData sceneData, CancellationToken ct)
        {
            if (!ct.IsCancellationRequested && sceneData.Scene.isLoaded && sceneData.Scene.buildIndex != -1)
            {
                var op = SceneManager.UnloadSceneAsync(sceneData.SceneAddress);
                while(!op.isDone && !ct.IsCancellationRequested)
                {
                    await Task.Yield();
                }
            }
        }
    }
}
