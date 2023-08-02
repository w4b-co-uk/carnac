using System.Collections;
using System.Collections.Generic;

namespace Carnac.Logic.Internal {
    internal class FixedQueue<T>: IEnumerable<T> {
        private readonly int fixedSize;
        private readonly Queue<T> queue;

        public FixedQueue(int fixedSize) {
            this.fixedSize = fixedSize;
            queue = new Queue<T>();
        }

        public void Enqueue(T item) {
            queue.Enqueue(item);
            if (queue.Count > fixedSize) {
                _ = queue.Dequeue();
            }
        }

        public void Clear() {
            queue.Clear();
        }

        public IEnumerator<T> GetEnumerator() {
            return queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}