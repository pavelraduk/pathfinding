using System.Collections.Generic;

namespace Assets.PathfindingAlgorithm
{
    public class Heap<T>
    {
        private struct Element
        {
            public T Value;
            public int Handle;
        };

        public static uint InvalidHandler => uint.MaxValue;

        public bool Empty => m_store.Count == 0;

        public int Count => m_store.Count;

        public T Top => m_store[0].Value;

        public T this[int i] => m_store[i].Value;

        public Heap(IComparer<T> comparer = null)
        {
            m_comparer = comparer ?? Comparer<T>.Default;
        }

        public int Insert(T value)
        {
            // define handler
            int handle;
            if (m_freeHandlers.Count == 0)
            {
                handle = m_indexByHandler.Count;
                m_indexByHandler.Add(m_store.Count);
            }
            else
            {
                handle = m_freeHandlers[m_freeHandlers.Count - 1];
                m_freeHandlers.RemoveAt(m_freeHandlers.Count - 1);
                m_indexByHandler[handle] = m_store.Count;
            }

            Element element;
            element.Value = value;
            element.Handle = handle;

            m_store.Add(element);
            SieveUp(handle);

            return handle;
        }

        public void Remove(int handler)
        {
            int lastHandler = m_store[m_store.Count - 1].Handle;

            if (lastHandler != handler)
            {
                Swap(handler, lastHandler);
            }

            m_store.RemoveAt(m_store.Count - 1);
            m_freeHandlers.Add(handler);

            if (lastHandler != handler)
            {
                SieveUp(lastHandler);
                SieveDown(lastHandler);
            }
        }

        public T ExtractMin()
        {
            T result = m_store[0].Value;

            int firstHandler = m_store[0].Handle;
            int lastHandler = m_store[m_store.Count - 1].Handle;

            if (firstHandler != lastHandler)
            {
                Swap(firstHandler, lastHandler);
            }

            m_store.RemoveAt(m_store.Count - 1);
            m_freeHandlers.Add(firstHandler);

            if (firstHandler != lastHandler)
            {
                SieveDown(lastHandler);
            }
            return result;
        }

        public void SieveUp(int handler)
        {
            int elementIndex = m_indexByHandler[handler];
            if (elementIndex == 0)
            {
                return;
            }

            int parentIndex = (elementIndex - 1) / 2;
            while (elementIndex > 0 && Less(elementIndex, parentIndex))
            {
                Swap(handler, m_store[parentIndex].Handle);

                elementIndex = parentIndex;
                parentIndex = (elementIndex - 1) / 2;
            }
        }

        public void SieveDown(int handler)
        {
            int parentIndex = m_indexByHandler[handler];
            int outIndexToSwap = TryGetChildToSwap(parentIndex);

            while (outIndexToSwap >= 0)
            {
                Swap(handler, m_store[outIndexToSwap].Handle);

                parentIndex = outIndexToSwap;
                outIndexToSwap = TryGetChildToSwap(parentIndex);
            }
        }

        public void Clear()
        {
            m_store.Clear();
            m_indexByHandler.Clear();
            m_freeHandlers.Clear();
        }

        private int TryGetChildToSwap(int parentIndex)
        {
            int firstChildIndex = 2 * parentIndex + 1;
            int secondChildIndex = 2 * parentIndex + 2;

            if (firstChildIndex >= m_store.Count && secondChildIndex >= m_store.Count)
            {
                return -1;
            }

            // Если второго потомка нет, то сравниваем родителя с первым потомком
            if (secondChildIndex >= m_store.Count)
            {
                return Less(firstChildIndex, parentIndex) ? firstChildIndex : -1;
            }

            // Если первого потомка нет, то сравниваем родителя со вторым потомком
            if (firstChildIndex >= m_store.Count)
            {
                return Less(secondChildIndex, parentIndex) ? secondChildIndex : -1;
            }

            // Есть оба потомка
            if (Less(firstChildIndex, secondChildIndex))
            {
                return Less(firstChildIndex, parentIndex) ? firstChildIndex : -1;
            }
            else
            {
                return Less(secondChildIndex, parentIndex) ? secondChildIndex : -1;
            }
        }

        private void Swap(int firstHandle, int secondHandle)
        {
            int firstIndex = m_indexByHandler[firstHandle];
            int secondIndex = m_indexByHandler[secondHandle];

            Element temp = m_store[firstIndex];
            m_store[firstIndex] = m_store[secondIndex];
            m_store[secondIndex] = temp;

            m_indexByHandler[firstHandle] = secondIndex;
            m_indexByHandler[secondHandle] = firstIndex;
        }

        bool Less(int firstIndex, int secondIndex)
        {
            return m_comparer.Compare(m_store[firstIndex].Value, m_store[secondIndex].Value) < 0;
        }

        private readonly IComparer<T> m_comparer;
        private readonly List<Element> m_store = new List<Element>();
        private readonly List<int> m_indexByHandler = new List<int>();
        private readonly List<int> m_freeHandlers = new List<int>();
    }
}
