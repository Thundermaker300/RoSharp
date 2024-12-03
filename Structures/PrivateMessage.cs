using RoSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Structures
{
    public struct PrivateMessage
    {
        public ulong Id { get; init; }
        public Id<User> Recipient { get; init; }
        public Id<User> Sender { get; init; }
        public string Subject { get; init; }
        public string Text { get; init; }
        public DateTime Created { get; init; }
        public bool IsSystemMessage { get; init; }
        public bool IsRead { get; init; }
    }
}
