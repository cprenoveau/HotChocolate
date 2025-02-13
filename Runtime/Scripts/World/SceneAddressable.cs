using HotChocolate.Utils;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace HotChocolate.World
{
    public class AddressableSceneData : ISceneData
    {
        public string SceneAddress { get; set; }
        public SceneInstance SceneInstance { get; set; }
        public UnityEngine.SceneManagement.Scene Scene => SceneInstance.Scene;
    }

    public static partial class Scene
    {
        public static void UseAddressables(SceneStack sceneStack)
        {
            sceneStack.loadSceneDefault = LoadSceneFromAddressable;
            sceneStack.unloadSceneDefault = UnloadSceneFromAddressable;
        }

        public static async Task<ISceneData> LoadSceneFromAddressable(string sceneAddress, CancellationToken ct)
        {
            var sceneData = new AddressableSceneData()
            {
                SceneAddress = sceneAddress
            };

            if (!ct.IsCancellationRequested)
            {
                var op = Addressables.LoadSceneAsync(sceneAddress);
                while(!op.IsDone && !ct.IsCancellationRequested)
                {
                    await Task.Yield();
                }

                sceneData.SceneInstance = op.Result;
            }

            return sceneData;
        }

        public static async Task UnloadSceneFromAddressable(ISceneData sceneData, CancellationToken ct)
        {
            if (!ct.IsCancellationRequested && sceneData.Scene.isLoaded && sceneData.Scene.buildIndex != -1)
            {
                var op = Addressables.UnloadSceneAsync((sceneData as AddressableSceneData).SceneInstance);
                while(!op.IsDone && !ct.IsCancellationRequested)
                {
                    await Task.Yield();
                }
            }
        }
    }
}
