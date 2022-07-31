using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PseudoExtensibleEnum
{
    public class PseudoExtensibleEnumContext
    {
        private Dictionary<Type, List<Type>> PseudoExtensionMap = new Dictionary<Type, List<Type>>();

        /// <summary>
        /// Gets the enum types that pseudo-extend a base enum type
        /// </summary>
        public Type[] GetPseudoExtensionsToEnum(Type baseType)
        {
            if(PseudoExtensionMap.TryGetValue(baseType, out List<Type> list))
            {
                return list.ToArray();
            }

            return new Type[] { };
        }

        /// <summary>
        /// Loads a collection of types into this context
        /// </summary>
        public void LoadTypes(IEnumerable<Type> types)
        {
            LoadTypesInternal(types);
        }

        /// <summary>
        /// Loads types from an assembly into this context
        /// </summary>
        public void LoadTypes(Assembly assembly)
        {
            LoadTypesInternal(assembly.GetTypes());
        }

        /// <summary>
        /// Loads types from a collection of assemblies into this context
        /// </summary>
        public void LoadTypes(IEnumerable<Assembly> assemblies)
        {
            LoadTypesInternal(assemblies.SelectMany(a => a.GetTypes()));
        }

        internal PseudoExtensibleEnumContext()
        {

        }

        private void LoadTypesInternal(IEnumerable<Type> types)
        {
            types = types.Where(t => t.IsEnum);

            //we'll trade some memory for speed (hopefully) and force these to materialize
            var extensibleTypes = types.Where(t => t.IsDefined(typeof(PseudoExtensibleAttribute))).ToArray();
            var extensionTypes = types.Where(t => t.IsDefined(typeof(PseudoExtendAttribute))).ToArray();

            foreach(var baseType in extensibleTypes)
            {
                var typeExtendedTypes = extensionTypes.Where(t => t.GetCustomAttribute<PseudoExtendAttribute>().BaseType == baseType);

                if(PseudoExtensionMap.ContainsKey(baseType))
                {
                    PseudoExtensionMap[baseType].AddRange(typeExtendedTypes);
                }
                else
                {
                    PseudoExtensionMap.Add(baseType, typeExtendedTypes.ToList());
                }
            }
        }

    }
}
