using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp
{
    public static class GlobalSession
    {
        private static Session? assigned;

        public static Session? Assigned => assigned;

        public static void AssignSession(Session session)
        {
            if (!SessionVerify.Verify(session))
                throw new ArgumentException("GlobalSession.AssignSession requires a non-null authenticated session.");

            assigned = session;
        }

        internal static Session? Global(this Session? session, string? mustReturnSomethingString = null)
        {
            if (!SessionVerify.Verify(session))
            {
                if (assigned != null)
                    return assigned;
                else if (mustReturnSomethingString != null)
                    SessionVerify.Throw(mustReturnSomethingString);
            }

            return session;
        }

        public static void Unassign() => assigned = null;
    }
}
