using Godot;

namespace Fractural
{
    public static partial class HintString
    {
        public const string DependencyPath = nameof(DependencyPath);
        public const string ClassType = nameof(ClassType);

        public enum DictNodeVarsMode
        {
            /// <summary>
            /// DictNodeVars are customizable on the Node
            /// </summary>
            Local,
            /// <summary>
            /// Fixed DictNodeVars will be generated for properties with the DictNodeVar attribute.
            /// </summary>
            Attributes,
            /// <summary>
            /// DictNodeVars are customizable on the Node, and fixed DictNodeVars will be generated for properties with the DictNodeVar attribute.
            /// </summary>
            LocalAttributes,
        }

        public static string DictNodeVars(DictNodeVarsMode mode = DictNodeVarsMode.LocalAttributes)
        {
            return $"{nameof(DictNodeVars)},{mode}";
        }

        public static void AddDictNodeVarsProp(this PropertyListBuilder builder, string name, DictNodeVarsMode mode = DictNodeVarsMode.LocalAttributes)
        {
            builder.AddItem(
                name: name,
                type: Variant.Type.Dictionary,
                hintString: DictNodeVars(mode)
            );
        }
    }
}
