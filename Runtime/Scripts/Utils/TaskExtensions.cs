using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace HotChocolate.Utils
{
    public static class TaskExtensions
    {
        public static async void FireAndForgetTask(this Task task, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            try
            {
                await task.ConfigureAwait(true);
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                    Debug.LogWarning("Fire and forget task was cancelled");
                else
                    Debug.LogError("Exception occured in FireAndForgetTask " + e.Message + " " + e.StackTrace);
            }
        }
    }
}
