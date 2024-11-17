using RoSharp.API.Pooling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Interfaces
{
    /// <summary>
    /// Interface for classes that have an Id parameter.
    /// </summary>
    /// <typeparam name="T">The type of the class.</typeparam>
    public interface IIdApi<T>
    {
        /// <summary>
        /// Gets the ID of the instance.
        /// </summary>
        public ulong Id { get; }

        /// <summary>
        /// Gets the instance from the provided Id.
        /// </summary>
        /// <param name="id">The Id.</param>
        /// <param name="session">Session. Optional.</param>
        /// <returns>A task containing the <typeparamref name="T"/>.</returns>
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        public static Task<T> FromId(ulong id, Session? session) => throw new NotImplementedException();

        /// <summary>
        /// Attaches a session to this instance and returns it.
        /// </summary>
        /// <param name="session">The session to attach.</param>
        /// <returns>The same instance.</returns>
        public T AttachSessionAndReturn(Session? session);
    }
}
