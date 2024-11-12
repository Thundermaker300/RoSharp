using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp
{
    public class SessionVerify
    {
        public const string GENERIC_MESSAGE =
            "This API member ({0}) requires a logged-in session to be attached. Did you call AttachSession or a constructor with a session that has been logged in via session.LoginAsync()?";

        public const string GENERIC_REFRESH_MESSAGE =
            "This API member ({0}) requires a logged-in session to be attached. Please attach a logged-in session via AttachSession() and run RefreshAsync() to update the API cache, or add the session when first accessing this class.";
        public static bool Verify(Session? session)
        {
            if (session is null)
                return false;
            if (!session.LoggedIn)
                return false;

            return true;
        }

        public static void ThrowIfNecessary(Session? session, string apiMemberName)
        {
            if (!Verify(session))
                Throw(apiMemberName);
        }

        public static void Throw(string apiMemberName)
            => throw new ArgumentNullException("session", string.Format(GENERIC_REFRESH_MESSAGE, apiMemberName));
        public static void ThrowRefresh(string apiMemberName)
            => throw new ArgumentNullException("session", string.Format(GENERIC_REFRESH_MESSAGE, apiMemberName));
    }
}
