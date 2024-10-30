using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp
{
    public class SessionErrors
    {
        public static bool Verify(Session? session)
        {
            if (session is null)
                throw new InvalidOperationException("This API member requires a logged-in session to be attached. Did you call AttachSession or a constructor with a session that has been logged in?");
            if (!session.LoggedIn)
                throw new InvalidOperationException("This API member requires a session that is logged in. Did you call Session.LoginAsync?");

            return true;
        }
    }
}
