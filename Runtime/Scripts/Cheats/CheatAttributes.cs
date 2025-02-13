using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HotChocolate.Cheats
{
    public interface ICustomWidget
    {
        void Init(string name, object data);

        public delegate void ValueChanged(object data);
        event ValueChanged OnValueChanged;
    }

    public interface ICheatPredicate
    {
        public bool IsValid();
    }
    
    public abstract class CheatAttribute : Attribute 
    {
        public string DeclarationOrder { get; protected set; }
        public KeyCode[] ShortcutKeys { get; set; }
        public Type Predicate { get; set; }
        private ICheatPredicate _predicateInstance;

        protected CheatAttribute(string file, int line) => DeclarationOrder = $"{file.ToLower()}|{line.ToString().PadLeft(6, '0')}";
        
        public bool IsValid()
        {
            if (Predicate == null)
                return true;

            if (!typeof(ICheatPredicate).IsAssignableFrom(Predicate))
            {
                Debug.LogWarning($"Invalid cheat predicate type {Predicate.Name}");
                return true;
            }

            _predicateInstance ??= (ICheatPredicate)Activator.CreateInstance(Predicate);
            return _predicateInstance.IsValid();
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CheatProperty : CheatAttribute
    {
        public CheatProperty([CallerFilePath] string file = "", [CallerLineNumber] int line = 0) : base(file, line) { }

        public string StringArrayProperty { get; set; } = null;
        public float MinValue { get; set; } = -1;
        public float MaxValue { get; set; } = -1;
        public string CustomWidgetAddress { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CheatMethod : CheatAttribute
    {
        public CheatMethod([CallerFilePath]string file = "", [CallerLineNumber]int line = 0) : base(file, line) { }
    }
}