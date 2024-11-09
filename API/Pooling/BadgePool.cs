﻿using RoSharp.API.Assets;

namespace RoSharp.API.Pooling
{
    internal static class BadgePool
    {
        internal static Dictionary<ulong, Badge> Pool = new();

        internal static void Add(Badge badge) => Pool.Add(badge.Id, badge);
        internal static Badge? Get(ulong id, Session? session = null) => Pool.GetValueOrDefault(id)?.AttachSessionAndReturn(session);
    }
}