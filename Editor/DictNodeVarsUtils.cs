using System;
using System.Reflection;
using System.Collections.Generic;
using Fractural.Utils;

namespace Fractural.DependencyInjection
{
    public static class DictNodeVarsUtils
    {
        /// <summary>
        /// Returns all fixed node vars for a given type, with each node var
        /// given a default value.
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static NodeVarData[] GetFixedDictNodeVarTemplates(Type objectType)
        {
            var fixedDictNodeVars = new List<NodeVarData>();
            foreach (var property in objectType.GetProperties())
            {
                var attribute = property.GetCustomAttribute<DictNodeVarAttribute>();
                if (attribute == null)
                    continue;

                NodeVarOperation operation;
                if (attribute.Operation.HasValue)
                    operation = attribute.Operation.Value;
                else
                {
                    // We scan the property for getters and setters to determien the operation
                    bool hasGetter = property.GetGetMethod() != null;
                    bool hasSetter = property.GetSetMethod() != null;

                    if (hasGetter && hasSetter)
                        operation = NodeVarOperation.GetSet;
                    else if (hasGetter)
                        // If the property has a getter, then it needs someone else to set it's value
                        operation = NodeVarOperation.Set;
                    else
                        // If the property has a setter, then it means other users can get it's value
                        operation = NodeVarOperation.Get;
                }

                fixedDictNodeVars.Add(new NodeVarData()
                {
                    Name = property.Name,
                    ValueType = property.PropertyType,
                    Operation = operation,
                    InitialValue = DefaultValueUtils.GetDefault(property.PropertyType)
                });
            }
            return fixedDictNodeVars.ToArray();
        }

        /// <summary>
        /// Returns all fixed node vars for a given type, with each node var
        /// given a default value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static NodeVarData[] GetFixedDictNodeVars<T>() => GetFixedDictNodeVarTemplates(typeof(T));
    }
}
