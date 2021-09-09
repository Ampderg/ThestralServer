using System;
using System.Collections.Generic;
using System.Text;

namespace ThestralServer
{
    static class EntityLibrary
    {
        public struct EntityLibraryEntry
        {
            public string entityName;
            public double maxMoveSpeed;

            public EntityLibraryEntry(string entityName, double maxMoveSpeed)
            {
                this.entityName = entityName;
                this.maxMoveSpeed = maxMoveSpeed;
            }
        }

        public static Dictionary<uint, EntityLibraryEntry> entries;

        public static void Populate()
        {
            entries = new Dictionary<uint, EntityLibraryEntry>();

            entries[0] = new EntityLibraryEntry("Player Character", 10);
        }
    }
}
