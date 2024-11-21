using RoSharp.Utility;

namespace RoSharp.Extensions
{
    /// <summary>
    /// A set of internal extension methods for <see cref="Session"/> objects.
    /// </summary>
    public static class SessionExtensions
    {
        internal static Session? Global(this Session? session)
        {
            if (!SessionVerify.Verify(session) && GlobalSession.Assigned != null)
                return GlobalSession.Assigned;

            return session;
        }

        internal static Session Global(this Session? session, string mustReturnSomethingString)
        {
            Session? testSession = Global(session);
            SessionVerify.ThrowIfNecessary(testSession, mustReturnSomethingString);
#pragma warning disable CS8603 // Possible null reference return.
            return testSession;
#pragma warning restore CS8603
        }
    }
}
