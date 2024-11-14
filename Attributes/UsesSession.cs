namespace RoSharp
{
    /// <summary>
    /// Indicates a member that requires Roblox authentication to use. No longer used.
    /// </summary>
    [Obsolete("Not used.")]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    public class UsesSession : Attribute
    {
    }
}
