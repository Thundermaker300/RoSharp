﻿namespace RoSharp.API.Pooling
{
    internal static class GroupPool
    {
        internal static Dictionary<ulong, Group> Pool = new();

        internal static void Add(Group group) => Pool.Add(group.Id, group);
        internal static Group? Get(ulong id, Session? session = null) => Pool.GetValueOrDefault(id)?.AttachSessionAndReturn(session);
    }
}