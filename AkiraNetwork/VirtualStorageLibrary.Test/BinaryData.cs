using System.Collections;
using System.Diagnostics;

namespace AkiraNetwork.VirtualStorageLibrary.Test
{
    public class BinaryData
        : IVirtualDeepCloneable<BinaryData>, IEnumerable<byte>, IList<byte>, ICollection<byte>, IDisposable
    {
        private byte[] _data;

        public BinaryData()
        {
            _data = [];
        }

        public BinaryData(IEnumerable<byte> data)
        {
            _data = data.ToArray();
        }

        public override string ToString()
        {
            const int maxDisplayLength = 4;
            string hex = "[";
            // ループをデータの長さと4の小さい方で回す
            int loopLength = Math.Min(_data.Length, maxDisplayLength);
            for (int i = 0; i < loopLength; i++)
            {
                hex += _data[i].ToString("X2");
                if (i < loopLength - 1)
                {
                    hex += ", ";
                }
            }
            if (_data.Length > maxDisplayLength)
            {
                hex += "...";
            }
            hex += "]";

            return hex;
        }

        public byte[] Data => _data;

        public byte this[int index]
        {
            get
            {
                if (index < 0 || index >= _data.Length)
                {
                    throw new IndexOutOfRangeException($"Index out of range. [{index}]");
                }
                return _data[index];
            }
            set
            {
                if (index < 0 || index >= _data.Length)
                {
                    throw new IndexOutOfRangeException($"Index out of range. [{index}]");
                }
                _data[index] = value;
            }
        }

        public int Count => _data.Length;

        public int Length => _data.Length;

        public bool IsReadOnly => false;

        public void Add(byte item)
        {
            byte[] newData = new byte[_data.Length + 1];
            _data.CopyTo(newData, 0);
            newData[_data.Length] = item;
            _data = newData;
        }

        public void Clear()
        {
            _data = [];
        }

        public bool Contains(byte item) => _data.Contains(item);

        public void CopyTo(byte[] array, int arrayIndex)
        {
            ArgumentNullException.ThrowIfNull(array);

            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException($"Argument out of range. [{arrayIndex}]");
            if (array.Length - arrayIndex < _data.Length)
                throw new ArgumentException("The remaining length of the array is insufficient.");

            _data.CopyTo(array, arrayIndex);
        }

        public IEnumerator<byte> GetEnumerator() => ((IEnumerable<byte>)_data).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int IndexOf(byte item)
        {
            for (int i = 0; i < _data.Length; i++)
            {
                if (_data[i] == item)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, byte item)
        {
            if (index < 0 || index > _data.Length)
            {
                throw new ArgumentOutOfRangeException($"Argument out of range. [{index}]");
            }

            byte[] newData = new byte[_data.Length + 1];
            Array.Copy(_data, 0, newData, 0, index);
            newData[index] = item;
            Array.Copy(_data, index, newData, index + 1, _data.Length - index);
            _data = newData;
        }

        public bool Remove(byte item)
        {
            int index = IndexOf(item);
            if (index < 0) return false;
            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _data.Length)
            {
                throw new ArgumentOutOfRangeException($"Argument out of range. [{index}]");
            }

            byte[] newData = new byte[_data.Length - 1];
            Array.Copy(_data, 0, newData, 0, index);
            Array.Copy(_data, index + 1, newData, index, _data.Length - index - 1);
            _data = newData;
        }

        public BinaryData DeepClone(bool recursive = false)
        {
            byte[] dataClone = new byte[_data.Length];
            _data.CopyTo(dataClone, 0);
            return new BinaryData(dataClone);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            _data = [];

            Debug.WriteLine("BinaryData.Dispose: データを破棄しました。");
        }
    }
}
