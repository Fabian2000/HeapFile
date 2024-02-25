namespace HeapFile
{
    /// <summary>
    /// Contains extension methods for the <see cref="Fmem"/> class.
    /// </summary>
    public static class FmemExtensions
    {
        /// <summary>
        /// Allocates a string in the file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Fpointer AllocString(this Fmem file, ref string value, long id)
        {
            int length = value.Length + 1;
            Fpointer pointer = file.Alloc<char>(length, id);
            return pointer;
        }

        /// <summary>
        /// Writes a string to the file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="pointer"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void WriteString(this Fmem file, Fpointer pointer, string value)
        {
            value = value + "\0";
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (pointer.Size < value.Length)
            {
                throw new ArgumentException("The pointer size is too small.");
            }

            char[] buffer = value.ToCharArray();
            file.WriteArray<char>(pointer, buffer);
        }

        /// <summary>
        /// Reads a string from the file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public static string ReadString(this Fmem file, Fpointer pointer)
        {
            int length = 0;
            for (long i = pointer.Offset; i < pointer.Offset + pointer.Size; i++)
            {
                char c = file.Read<char>(file.PointerOffset(pointer, sizeof(char) * (i - pointer.Offset)));
                if (c == '\0')
                {
                    break;
                }

                length++;
            }

            char[] buffer = file.ReadArray<char>(pointer, length);
            return new string(buffer);
        }

        /// <summary>
        /// Allocates a DateTime in the file.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Fpointer AllocDateTime(this Fmem file, long id)
        {
            Fpointer pointer = file.Alloc<long>(id);
            return pointer;
        }

        /// <summary>
        /// Writes DateTime to the file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="pointer"></param>
        /// <param name="value"></param>
        public static void WriteDateTime(this Fmem file, Fpointer pointer, DateTime value)
        {
            byte[] buffer = BitConverter.GetBytes(value.Ticks);
            file.WriteArray<byte>(pointer, buffer);
        }

        /// <summary>
        /// Reads DateTime from the file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public static DateTime ReadDateTime(this Fmem file, Fpointer pointer)
        {
            byte[] buffer = file.ReadArray<byte>(pointer, sizeof(long));
            long ticks = BitConverter.ToInt64(buffer, 0);
            return new DateTime(ticks);
        }
    }
}
