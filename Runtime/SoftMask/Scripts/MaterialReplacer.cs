using System;
using System.Collections.Generic;
using System.Reflection;
#if !NET_STANDARD_2_0
using System.Reflection.Emit;
#endif
using UnityEngine;

namespace SoftMasking {
    /// <summary>
    /// Mark an implementation of the IMaterialReplacer interface with this attribute to
    /// register it in the global material replacer chain. The global replacers will be
    /// used automatically by all SoftMasks.
    ///
    /// Globally registered replacers are called in order of ascending of their `order`
    /// value. The traversal is stopped on the first IMaterialReplacer which returns a
    /// non-null value and this returned value is then used as a replacement.
    ///
    /// Implementation of IMaterialReplacer marked by this attribute should have a
    /// default constructor.
    /// </summary>
    /// <seealso cref="IMaterialReplacer"/>
    /// <seealso cref="MaterialReplacer.globalReplacers"/>
    [AttributeUsage(AttributeTargets.Class)]
    public class GlobalMaterialReplacerAttribute : Attribute { }

    /// <summary>
    /// Used by SoftMask to automatically replace materials which don't support
    /// Soft Mask by those that do.
    /// </summary>
    /// <seealso cref="GlobalMaterialReplacerAttribute"/>
    public interface IMaterialReplacer {
        /// <summary>
        /// Determines the mutual order in which IMaterialReplacers will be called.
        /// The lesser the return value, the earlier it will be called, that is,
        /// replacers are sorted by ascending of the `order` value.
        /// The order of default implementation is 0. If you want your function to be
        /// called before, return a value lesser than 0.
        /// </summary>
        int order { get; }

        /// <summary>
        /// Should return null if this replacer can't replace the given material.
        /// </summary>
        Material Replace(Material material);
    }

    public static class MaterialReplacer {
        static List<IMaterialReplacer> _globalReplacers;

        /// <summary>
        /// Returns the collection of all globally registered replacers.
        /// </summary>
        /// <seealso cref="GlobalMaterialReplacerAttribute"/>
        public static List<IMaterialReplacer> globalReplacers {
            get {
                if (_globalReplacers == null)
                    _globalReplacers = CollectGlobalReplacers();
                return _globalReplacers;
            }
        }

        static List<IMaterialReplacer> CollectGlobalReplacers() {
            List<IMaterialReplacer> materialReplacers = new List<IMaterialReplacer>();
            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach(var type in assembly.GetTypesSafe())
                {
                    if (IsMaterialReplacerType(type))
                    {
                        IMaterialReplacer materialReplacer = TryCreateInstance(type);
                        if (materialReplacer != null)
                            materialReplacers.Add(materialReplacer);
                    }
                }
            }
            return materialReplacers;
        }

        static bool IsMaterialReplacerType(Type t) {
        #if NET_STANDARD_2_0
            var isTypeBuilder = false;
        #else
            var isTypeBuilder = t is TypeBuilder;
        #endif
            return !isTypeBuilder
                && !t.IsAbstract
                && t.IsDefined(typeof(GlobalMaterialReplacerAttribute), false)
                && typeof(IMaterialReplacer).IsAssignableFrom(t);
        }

        static IMaterialReplacer TryCreateInstance(Type t) {
            try {
                return (IMaterialReplacer)Activator.CreateInstance(t);
            } catch (Exception ex) {
                Debug.LogErrorFormat("Could not create instance of {0}: {1}", t.Name, ex);
                return null;
            }
        }

        static IEnumerable<Type> GetTypesSafe(this Assembly asm) {
            try {
                return asm.GetTypes();
            } catch (ReflectionTypeLoadException e) {
                List<Type> types = new List<Type>();
                foreach(var t in e.Types)
                {
                    if (t != null)
                        types.Add(t);
                }
                return types;
            }
        }
    }

    public class MaterialReplacerChain : IMaterialReplacer {
        readonly List<IMaterialReplacer> _replacers;

        public MaterialReplacerChain(List<IMaterialReplacer> replacers, IMaterialReplacer yetAnother) {
            _replacers = replacers;
            _replacers.Add(yetAnother);
            Initialize();
        }

        public int order { get; private set; }

        public Material Replace(Material material) {
            for (int i = 0; i < _replacers.Count; ++i) {
                var result = _replacers[i].Replace(material);
                if (result != null)
                    return result;
            }
            return null;
        }

        void Initialize() {
            _replacers.Sort((a, b) => a.order.CompareTo(b.order));
            order = _replacers[0].order;
        }
    }
}
