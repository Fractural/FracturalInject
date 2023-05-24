using System;

namespace Fractural
{
    public static partial class HintString
    {
        public const string DependencyPath = nameof(DependencyPath);
        public const string ClassType = nameof(ClassType);

        public enum NodeVarsMode
        {
            /// <summary>
            /// NodeVars are customizable on the Node
            /// </summary>
            Local,
            /// <summary>
            /// Fixed NodeVars will be generated for properties with the NodeVar attribute.
            /// </summary>
            Attributes,
            /// <summary>
            /// NodeVars are customizable on the Node, and fixed NodeVars will be generated for properties with the NodeVar attribute.
            /// </summary>
            LocalAttributes,
        }

        public static string NodeVars(NodeVarsMode mode)
        {
            return $"{nameof(NodeVars)},{mode}";
        }
    }
}
