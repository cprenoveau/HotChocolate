using HotChocolate.Utils;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HotChocolate.UI
{
    public static partial class Menu
    {
        public static void UseAddressables(MenuStack menuStack)
        {
            menuStack.createInstanceDefault = CreateInstanceFromAddressable;
            menuStack.destroyInstanceDefault = DestroyAddressableInstance;
        }

        public static async Task<IMenu> CreateInstanceFromAddressable(object assetKey, string menuId, Transform parent, int siblingIndex, MonoBehaviour opHolder, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return null;
            }

            var op = Addressables.InstantiateAsync(assetKey, opHolder.transform.parent);
            await opHolder.StartCoroutineAsync(op, ct).ConfigureAwait(true);

            var instance = op.Result;
            instance.transform.SetParent(parent);

            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;

            if (siblingIndex != -1)
                instance.transform.SetSiblingIndex(siblingIndex);

            instance.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            instance.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

            instance.GetComponent<IMenu>().Id = menuId;
            return instance.GetComponent<IMenu>();
        }

        public static Task DestroyAddressableInstance(IMenu instance, MonoBehaviour opHolder, CancellationToken ct)
        {
            if (!ct.IsCancellationRequested)
            {
                Addressables.ReleaseInstance((instance as MonoBehaviour).gameObject);
            }

            return Task.FromResult<bool>(true);
        }
    }
}
