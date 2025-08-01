namespace RoSharp.API.Assets
{
    /// <summary>
    /// A struct that contains information about the inner details of a model.
    /// </summary>
    public struct ModelDetails
    {
        /// <summary>
        /// Gets the AssetId of the model that this struct is referring to.
        /// </summary>
        public ulong AssetId { get; internal set; }

        /// <summary>
        /// Gets the total amount of calculated triangles within the model.
        /// </summary>
        public int Triangles { get; internal set; }

        /// <summary>
        /// Gets the total amount of calculated vertices within the model.
        /// </summary>
        public int Vertices { get; internal set; }

        /// <summary>
        /// Gets the total amount of scripts within the model.
        /// </summary>
        public int ScriptCount { get; internal set; }

        /// <summary>
        /// Gets the total amount of mesh parts within the model.
        /// </summary>
        public int MeshPartCount { get; internal set; }

        /// <summary>
        /// Gets the total amount of animations within the model.
        /// </summary>
        public int AnimationCount { get; internal set; }

        /// <summary>
        /// Gets the total amount of decals within the model.
        /// </summary>
        public int DecalCount { get; internal set; }

        /// <summary>
        /// Gets the total amount of audios within the model.
        /// </summary>
        public int AudioCount { get; internal set; }

        /// <summary>
        /// Gets the total amount of tools within the model.
        /// </summary>
        public int ToolCount { get; internal set; }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"ModelDetails [{AssetId}] || Triangles: {Triangles} | Vertices: {Vertices} | Scripts: {ScriptCount}| MeshParts: {MeshPartCount} | Animations: {AnimationCount} | Decals: {DecalCount} | Audios: {AudioCount} | Tools: {ToolCount}  ";
        }
    }
}
