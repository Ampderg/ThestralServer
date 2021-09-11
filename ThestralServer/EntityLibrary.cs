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
            public int halfPixelWidthCol;
            public int halfPixelHeightCol;

            public EntityLibraryEntry(string entityName, double maxMoveSpeed, int halfPixelWidthCol = 0, int halfPixelHeightCol = 0)
            {
                this.entityName = entityName;
                this.maxMoveSpeed = maxMoveSpeed;
                this.halfPixelWidthCol = halfPixelWidthCol;
                this.halfPixelHeightCol = halfPixelHeightCol;
            }
        }

        public static Dictionary<uint, EntityLibraryEntry> entries;

        public static void Populate()
        {
            entries = new Dictionary<uint, EntityLibraryEntry>();

            entries[0] = new EntityLibraryEntry("Player Character", 10.2, 7, 1);
        }
    }
}
