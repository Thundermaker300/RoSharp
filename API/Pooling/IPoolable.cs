namespace RoSharp.API.Pooling
{
    internal interface IPoolable
    {
        public ulong Id { get; }

        public IPoolable AttachSessionAndReturn(Session? session);
    }
}
