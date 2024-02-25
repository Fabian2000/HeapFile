namespace HeapFile
{
    /// <summary>
    /// Represents a pointer to a record in the file.
    /// </summary>
    public struct Fpointer
    {
        // The offset of the pointer.
        private readonly long _offset;
        // The size of the pointer.
        private readonly long _size;
        private long _id;
        
        // If the pointer is freed.
        public bool Freed { get; internal set; } = false;

        public long Id { get => _id; internal set => _id = value; }

        // The list of all pointers.
        internal static List<Fpointer> Pointers { get; set; } = new List<Fpointer>();

        /// <summary>
        /// Creates a new pointer with a specified offset and size.
        /// </summary>
        /// <param name="offset">The offset, the base is 0.</param>
        /// <param name="size">The size must be greater than 1, never less</param>
        internal Fpointer(long offset, long size, long id = 0)
        {
            if (offset < 0)
            {
                // The offset must be greater than 0, because 0 would be an invalid pointer.
                throw new ArgumentException("Found invalid pointer.");
            }

            _offset = offset;

            if (size < 1)
            {
                // The size must be greater than 0, because 0 would be a null pointer which is not supported.
                throw new ArgumentException("Found nullptr: Size must be greater than 0.");
            }

            _size = size;
            _id = id;

            Pointers.Add(this);
        }

        /// <summary>
        /// The offset of the pointer
        /// </summary>
        internal long Offset => _offset;

        /// <summary>
        /// The size of the pointer
        /// </summary>
        internal long Size => _size;
    }
}
