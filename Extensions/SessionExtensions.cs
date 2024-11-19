using RoSharp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Extensions
{
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
            return testSession;
        }
    }
}
