using System;
using System.Collections.Generic;
using System.Text;

namespace ThestralServer
{
    class IdAssigner
    {
        public Queue<uint> nextIds = new Queue<uint>();
        public uint highestFreeId = 0;

        public uint GetFreeID()
        {
            if(nextIds.Count > 0)
            {
                return nextIds.Dequeue();
            }
            else
            {
                return highestFreeId++;
            }
        }

        public void UnassignID(uint id)
        {
            nextIds.Enqueue(id);
        }
    }
}
