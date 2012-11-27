using System;

using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace Awful.Helpers
{
    public struct VirtualTextItem
    {
        public int Index { get; set; }
        public string Text { get; set; }
    }

    public class VirtualTextReader : IList<string>
    {
        private readonly string wholeText;
        private readonly int blockCount;
        private readonly int stepSize;
        private readonly int blockSize;
        private int currentCount;

        public event EventHandler SizeChanged;

        public VirtualTextReader(string text, int blockSize, int stepSize)
        {
            this.wholeText = text;
            this.blockSize = blockSize;
            this.blockCount = text.Length / blockSize;
            this.stepSize = stepSize;
            this.currentCount = stepSize;

            if (text.Length % blockSize > 0)
                this.blockCount = this.blockCount + 1;
        }

        public int IndexOf(string item)
        {
            throw new NotImplementedException();
        }

        public string this[int index]
        {
            get
            {
                int startIndex = index * blockSize;
                int blockEnd = startIndex + blockSize;
                var min = Math.Min(blockEnd, wholeText.Length);
                int length = 0;

                if (min == blockEnd)
                    length = blockSize;
                else
                    length = wholeText.Length - startIndex;

                var sub = wholeText.Substring(startIndex, length);

                return sub;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int Count
        {
            get { return blockCount; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        #region Unused

        public bool Remove(string item)
        {
            throw new NotImplementedException();
        }
        public IEnumerator<string> GetEnumerator()
        {
            throw new NotImplementedException();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        public void Insert(int index, string item)
        {
            throw new NotImplementedException();
        }
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
        public void CopyTo(string[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
        public bool Contains(string item)
        {
            return true;
        }
        public void Add(string item)
        {
            throw new NotImplementedException();
        }
        public void Clear()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
