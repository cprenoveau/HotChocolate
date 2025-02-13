using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HotChocolate.Utils
{
    public static class TaskCoroutineExtensions
    {
        public static IEnumerator AsIEnumerator(this Task task)
        {
            return AsIEnumerator(task, Debug.Log);
        }

        public static IEnumerator AsIEnumerator(this Task task, Action<object> exceptionHandler)
        {
            async Task GoAsync()
            {
                try
                {
                    await task;
                }
                catch (Exception e)
                {
                    exceptionHandler?.Invoke(e);
                }
            }
            return GoAsync()._AsIEnumerator();
        }

        private static IEnumerator _AsIEnumerator(this Task task)
        {
            yield return new WaitUntil(() => task.IsCompleted || task.IsFaulted || task.IsCanceled);
        }

        public static async Task StartCoroutineAsync(this MonoBehaviour monoBehaviour, IEnumerator coroutine, CancellationToken ct)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
            var coroutineToWait = monoBehaviour.StartCoroutine(Coroutine(coroutine, () => taskCompletionSource.TrySetResult(true)));

            void cancelRegistration()
            {
                taskCompletionSource.TrySetCanceled();
                monoBehaviour.StopCoroutine(coroutineToWait);
            }

            using (ct.Register(cancelRegistration))
            {
                await taskCompletionSource.Task.ConfigureAwait(true);
            }
        }

        public static async Task StartCoroutineAsync(this MonoBehaviour monoBehaviour, AsyncOperation op, CancellationToken ct)
        {
            await StartCoroutineAsync(monoBehaviour, Coroutine(op), ct).ConfigureAwait(true);
        }

        private static IEnumerator Coroutine(IEnumerator coroutine, Action p)
        {
            yield return coroutine;
            p?.Invoke();
        }

        private static IEnumerator Coroutine(AsyncOperation op)
        {
            yield return op;
        }

        public static bool IsCompletedSuccessfully(this Task task)
        {
            return task.IsCompleted && task.Status == TaskStatus.RanToCompletion;
        }
    }
}
