using System;

namespace EditorTools
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AutoAssignAttribute : Attribute
    {
        public enum AutoAssignMode
        {
            Same,
            Child,
            Parent
        }

        public AutoAssignMode Mode { get; private set; }

        public AutoAssignAttribute()
        {
            Mode = AutoAssignMode.Same;
        }

        public AutoAssignAttribute(AutoAssignMode mode)
        {
            Mode = mode;
        }
    }
}