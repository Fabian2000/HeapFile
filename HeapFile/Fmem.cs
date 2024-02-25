namespace HeapFile
{
    /// <summary>
    /// Represents a kind of heap as a file.
    /// </summary>
    public class Fmem : IDisposable
    {
        private readonly FileStream _fileStream;
        private readonly string _headerPath;

        /// <summary>
        /// The official recommended file extension for HeapFile without a starting dot.
        /// </summary>
        public const string FILE_EXTENSION = "hpf";

        /// <summary>
        /// Automatically creates a new file if it doesn't exist and keeps the file open.
        /// </summary>
        /// <param name="path">The path to the hpf file.</param>
        public Fmem(string path)
        {
            _fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            // check if a header exists. If yes, read the freed pointers.
            _headerPath = Path.ChangeExtension(path, ".hpf.header");
            if (File.Exists(_headerPath))
            {
                using (FileStream headerStream = new FileStream(_headerPath, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader reader = new BinaryReader(headerStream))
                    {
                        int count = reader.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            long offset = reader.ReadInt64();
                            long size = reader.ReadInt64();
                            bool freed = reader.ReadBoolean();
                            long id = reader.ReadInt64();
                            //Fpointer.Pointers.Add(new Fpointer(offset, size, id) { Freed = freed });
                            new Fpointer(offset, size, id) { Freed = freed };
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The length of the file.
        /// </summary>
        public long Length => _fileStream.Length;

        /// <summary>
        /// Allocates memory with a specified size and returns the pointer.
        /// </summary>
        /// <param name="size"></param>
        /// <returns>The "pointer".</returns>
        private Fpointer AllocOld(int size, long id)
        {
            long offset = _fileStream.Seek(0, SeekOrigin.End);
            _fileStream.Seek(0, SeekOrigin.End);

            _fileStream.Write(new byte[size], 0, size);

            return new Fpointer(offset, size, id);
        }

        public void GetAllOldPointers(out Fpointer[] pointers)
        {
            pointers = Fpointer.Pointers.ToArray();
            //pointers = Fpointer.Pointers.OrderBy(p => p.Id).ToArray();
        }

        /// <summary>
        /// Allocates memory with a specified size and returns the pointer.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="id">For reallocation (mapping) after program restart</param>
        /// <returns>The "pointer".</returns>
        public Fpointer Alloc(int size, long id)
        {
            for (int i = 0; i < Fpointer.Pointers.Count; i++)
            {
                if (!Fpointer.Pointers[i].Freed)
                {
                    continue;
                }

                if (Fpointer.Pointers[i].Size >= size)
                {
                    Fpointer pointer = Fpointer.Pointers[i];
                    Fpointer.Pointers.RemoveAt(i);
                    if (pointer.Size > size)
                    {
                        Fpointer.Pointers.Add(new Fpointer(pointer.Offset + size, pointer.Size - size));
                    }
                    return new Fpointer(pointer.Offset, size, id);
                }
            }

            return AllocOld(size, id);
        }

        /// <summary>
        /// Allocates memory with the size of the given type and returns the pointer.
        /// </summary>
        /// <typeparam name="T">The type which is used for the size.</typeparam>
        /// <returns>The "pointer".</returns>
        /// <exception cref="ArgumentException">Exception, if the type is not supported.</exception>
        public Fpointer Alloc<T>(long id) where T : struct => typeof(T) switch
        {
            _ when typeof(T) == typeof(int) => Alloc(sizeof(int), id),
            _ when typeof(T) == typeof(long) => Alloc(sizeof(long), id),
            _ when typeof(T) == typeof(short) => Alloc(sizeof(short), id),
            _ when typeof(T) == typeof(byte) => Alloc(sizeof(byte), id),
            _ when typeof(T) == typeof(char) => Alloc(sizeof(char), id),
            _ when typeof(T) == typeof(double) => Alloc(sizeof(double), id),
            _ when typeof(T) == typeof(float) => Alloc(sizeof(float), id),
            _ when typeof(T) == typeof(decimal) => Alloc(sizeof(decimal), id),
            _ when typeof(T) == typeof(bool) => Alloc(sizeof(bool), id),
            _ when typeof(T) == typeof(sbyte) => Alloc(sizeof(sbyte), id),
            _ when typeof(T) == typeof(uint) => Alloc(sizeof(uint), id),
            _ when typeof(T) == typeof(ulong) => Alloc(sizeof(ulong), id),
            _ when typeof(T) == typeof(ushort) => Alloc(sizeof(ushort), id),
            _ => throw new ArgumentException("Type not supported"),
        };

        /// <summary>
        /// Allocates memory with the size of the given type multiplied by the array size and returns the pointer.
        /// </summary>
        /// <typeparam name="T">The type which is used for the size.</typeparam>
        /// <param name="arraySize">The size of the array.</param>
        /// <returns>The "pointer"</returns>
        /// <exception cref="ArgumentException">Exception, if the type is not supported.</exception>
        public Fpointer Alloc<T>(int arraySize, long id) where T : struct
        {
            return typeof(T) switch
            {
                _ when typeof(T) == typeof(int) => Alloc(sizeof(int) * arraySize, id),
                _ when typeof(T) == typeof(long) => Alloc(sizeof(long) * arraySize, id),
                _ when typeof(T) == typeof(short) => Alloc(sizeof(short) * arraySize, id),
                _ when typeof(T) == typeof(byte) => Alloc(sizeof(byte) * arraySize, id),
                _ when typeof(T) == typeof(char) => Alloc(sizeof(char) * arraySize, id),
                _ when typeof(T) == typeof(double) => Alloc(sizeof(double) * arraySize, id),
                _ when typeof(T) == typeof(float) => Alloc(sizeof(float) * arraySize, id),
                _ when typeof(T) == typeof(decimal) => Alloc(sizeof(decimal) * arraySize, id),
                _ when typeof(T) == typeof(bool) => Alloc(sizeof(bool) * arraySize, id),
                _ when typeof(T) == typeof(sbyte) => Alloc(sizeof(sbyte) * arraySize, id),
                _ when typeof(T) == typeof(uint) => Alloc(sizeof(uint) * arraySize, id),
                _ when typeof(T) == typeof(ulong) => Alloc(sizeof(ulong) * arraySize, id),
                _ when typeof(T) == typeof(ushort) => Alloc(sizeof(ushort) * arraySize, id),
                _ => throw new ArgumentException("Type not supported"),
            };
        }

        /// <summary>
        /// Writes the value to the pointer.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="pointer">The pointer.</param>
        /// <returns>Returns the value at the pointer.</returns>
        public T Read<T>(Fpointer pointer) where T : struct
        {
            _fileStream.Seek(pointer.Offset, SeekOrigin.Begin);
            byte[] buffer = new byte[SizeOf<T>()];
            _fileStream.Read(buffer, 0, buffer.Length);
            return ConvertTo<T>(buffer);
        }

        private T ConvertTo<T>(byte[] buffer) where T : struct => typeof(T) switch
        {
            _ when typeof(T) == typeof(int) => (T)Convert.ChangeType(BitConverter.ToInt32(buffer, 0), typeof(T)),
            _ when typeof(T) == typeof(long) => (T)Convert.ChangeType(BitConverter.ToInt64(buffer, 0), typeof(T)),
            _ when typeof(T) == typeof(short) => (T)Convert.ChangeType(BitConverter.ToInt16(buffer, 0), typeof(T)),
            _ when typeof(T) == typeof(byte) => (T)Convert.ChangeType(buffer[0], typeof(T)),
            _ when typeof(T) == typeof(char) => (T)Convert.ChangeType(BitConverter.ToChar(buffer, 0), typeof(T)),
            _ when typeof(T) == typeof(double) => (T)Convert.ChangeType(BitConverter.ToDouble(buffer, 0), typeof(T)),
            _ when typeof(T) == typeof(float) => (T)Convert.ChangeType(BitConverter.ToSingle(buffer, 0), typeof(T)),
            _ when typeof(T) == typeof(bool) => (T)Convert.ChangeType(BitConverter.ToBoolean(buffer, 0), typeof(T)),
            _ when typeof(T) == typeof(sbyte) => (T)Convert.ChangeType((sbyte)buffer[0], typeof(T)),
            _ when typeof(T) == typeof(uint) => (T)Convert.ChangeType(BitConverter.ToUInt32(buffer, 0), typeof(T)),
            _ when typeof(T) == typeof(ulong) => (T)Convert.ChangeType(BitConverter.ToUInt64(buffer, 0), typeof(T)),
            _ when typeof(T) == typeof(ushort) => (T)Convert.ChangeType(BitConverter.ToUInt16(buffer, 0), typeof(T)),
            _ => throw new ArgumentException("Type not supported"),
        };  

        /// <summary>
        /// Reads an array from the pointer.
        /// </summary>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="pointer">The pointer.</param>
        /// <param name="size">The size of the array.</param>
        /// <returns>Returns the array at the pointer.</returns>
        public T[] ReadArray<T>(Fpointer pointer, int size) where T : struct
        {
            T[] arr = new T[size];
            for (int i = 0; i < size; i++)
            {
                arr[i] = Read<T>(PointerOffset(pointer, i * SizeOf<T>()));
            }
            return arr;
        }

        /// <summary>
        /// Writes the value to the pointer.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="pointer">The pointer.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentException">Exception, if the type is not supported.</exception>
        public void Write<T>(Fpointer pointer, T value) where T : struct
        {
            byte[] buffer = typeof(T) switch
            {
                _ when typeof(T) == typeof(int) => BitConverter.GetBytes((int)(object)value!),
                _ when typeof(T) == typeof(long) => BitConverter.GetBytes((long)(object)value!),
                _ when typeof(T) == typeof(short) => BitConverter.GetBytes((short)(object)value!),
                _ when typeof(T) == typeof(byte) => [(byte)(object)value!],
                _ when typeof(T) == typeof(char) => BitConverter.GetBytes((char)(object)value!),
                _ when typeof(T) == typeof(double) => BitConverter.GetBytes((double)(object)value!),
                _ when typeof(T) == typeof(float) => BitConverter.GetBytes((float)(object)value!),
                _ when typeof(T) == typeof(bool) => BitConverter.GetBytes((bool)(object)value!),
                _ when typeof(T) == typeof(sbyte) => [(byte)((sbyte)(object)value!)],
                _ when typeof(T) == typeof(uint) => BitConverter.GetBytes((uint)(object)value!),
                _ when typeof(T) == typeof(ulong) => BitConverter.GetBytes((ulong)(object)value!),
                _ when typeof(T) == typeof(ushort) => BitConverter.GetBytes((ushort)(object)value!),
                _ => throw new ArgumentException("Type not supported"),
            };

            if (buffer.Length > pointer.Size)
            {
                throw new ArgumentException("Buffer size does not match expected size.");
            }

            _fileStream.Seek(pointer.Offset, SeekOrigin.Begin);
            _fileStream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes an array to the pointer.
        /// </summary>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="pointer">The pointer.</param>
        /// <param name="arr">An array of the type T.</param>
        public void WriteArray<T>(Fpointer pointer, T[] arr) where T : struct
        {
            int size = SizeOf<T>();
            for (int i = 0; i < arr.Length; i++)
            {
                T value = arr[i];
                Write<T>(PointerOffset(pointer, i * size), value);
            }
        }

        private int SizeOf<T>() where T : struct => typeof(T) switch
        {
            _ when typeof(T) == typeof(int) => sizeof(int),
            _ when typeof(T) == typeof(long) => sizeof(long),
            _ when typeof(T) == typeof(short) => sizeof(short),
            _ when typeof(T) == typeof(byte) => sizeof(byte),
            _ when typeof(T) == typeof(char) => sizeof(char),
            _ when typeof(T) == typeof(double) => sizeof(double),
            _ when typeof(T) == typeof(float) => sizeof(float),
            _ when typeof(T) == typeof(decimal) => sizeof(decimal),
            _ when typeof(T) == typeof(bool) => sizeof(bool),
            _ when typeof(T) == typeof(sbyte) => sizeof(sbyte),
            _ when typeof(T) == typeof(uint) => sizeof(uint),
            _ when typeof(T) == typeof(ulong) => sizeof(ulong),
            _ when typeof(T) == typeof(ushort) => sizeof(ushort),
            _ => throw new ArgumentException("Type not supported"),
        };

        public Fpointer PointerOffset(Fpointer pointer, long offset)
        {
            if ((pointer.Offset + offset) < 0)
            {
                throw new ArgumentException("Offset is too small.");
            }
            else if ((pointer.Offset + offset) > _fileStream.Length)
            {
                throw new ArgumentException("Offset is too large.");
            }
            else if ((pointer.Size - offset) < 1)
            {
                throw new ArgumentException("Size is too small.");
            }
            else if ((pointer.Size - offset) > _fileStream.Length)
            {
                throw new ArgumentException("Size is too large.");
            }
            else if ((pointer.Offset + offset + pointer.Size - offset) > _fileStream.Length)
            {
                throw new ArgumentException("Size is too large.");
            }
            else if ((pointer.Offset + offset) > (pointer.Offset + offset + pointer.Size - offset))
            {
                throw new ArgumentException("Size is too small.");
            }

            return new Fpointer(pointer.Offset + offset, pointer.Size - offset);
        }

        /// <summary>
        /// Frees the memory of the pointer.
        /// </summary>
        /// <param name="pointer"></param>
        public void Free(Fpointer pointer)
        {
            pointer.Freed = true;
        }

        public void Flush()
        {
            _fileStream.Flush();
        }
        
        /// <summary>
        /// Shrinks the file to the possible minimum size.
        /// </summary>
        public void Shrink()
        {
            if (Fpointer.Pointers.Count == 0)
            {
                return;
            }

            Fpointer lastPointer = Fpointer.Pointers[0];
            for (int i = 0; i < Fpointer.Pointers.Count; i++)
            {
                if (!Fpointer.Pointers[i].Freed)
                {
                    continue;
                }

                if (Fpointer.Pointers[i].Offset > lastPointer.Offset)
                {
                    lastPointer = Fpointer.Pointers[i];
                }
            }

            if (!lastPointer.Freed)
            {
                return;
            }

            if (lastPointer.Offset + lastPointer.Size == _fileStream.Length)
            {
                _fileStream.SetLength(lastPointer.Offset);
                Fpointer.Pointers.Remove(lastPointer);
                Shrink();
            }
        }

        /// <summary>
        /// Close the file and release all resources.
        /// </summary>
        public void Close()
        {
            using (FileStream headerStream = new FileStream(_headerPath, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter writer = new BinaryWriter(headerStream))
                {
                    writer.Write(Fpointer.Pointers.Count);
                    foreach (Fpointer pointer in Fpointer.Pointers)
                    {
                        writer.Write(pointer.Offset);
                        writer.Write(pointer.Size);
                        writer.Write(pointer.Freed);
                        writer.Write(pointer.Id);
                    }
                }
            }

            _fileStream?.Close();
            _fileStream?.Dispose();
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~Fmem()
        {
            Close();
        }

        /// <summary>
        /// Dispose the file and release all resources.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Close();
        }
    }
}
