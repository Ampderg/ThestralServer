using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ThestralServer
{
    class PriorityQueue<T> : IEnumerable<T>
    {

#pragma warning disable CS0693 // Type parameter has the same name as the type parameter from outer type
        private struct PriorityItem<T>

        {
            public T item;
            public double priority;

            public PriorityItem(T item, double priority)
            {
                this.item = item;
                this.priority = priority;
            }
        }
#pragma warning restore CS0693 // Type parameter has the same name as the type parameter from outer type

        private PriorityItem<T>[] elements;

        public PriorityQueue(int startingLength)
        {
            elements = new PriorityItem<T>[startingLength];
        }

        public PriorityQueue() : this(10)
        {

        }

        public void Enqueue(T obj, double priority)
        {
            PriorityItem<T> item = new PriorityItem<T>(obj, priority);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return elements.GetEnumerator();
        }

        internal bool HasNext()
        {
            return false;
        }

        internal T Dequeue()
        {
            throw new NotImplementedException();
        }
    }
}
