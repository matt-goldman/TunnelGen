using System;

namespace TunnelGen.Attributes
{

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class SetTunnelUrlAttribute : Attribute
    {
        public string TunnelName { get; }

        public SetTunnelUrlAttribute(string tunnelName = null)
        {
            TunnelName = tunnelName;
        }
    }
}