using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ThestralServer
{
    class RoomCollision
    {
        #region Static
        public static bool TryCollide(uint roomId, int pixelX, int pixelY)
        {
            return TryCollide(roomId, new int[2] { pixelX, pixelY });
        }
        public static bool TryCollide(uint roomId, int[] pos)
        {
            if(rooms.ContainsKey(roomId))
            {
                return rooms[roomId].TryCollide(pos);
            }
            else
            {
                Program.Log($"A collision attempt was made for room id {roomId}, but no collider information existed for it!", LogType.Warning);
                return false;
            }
        }

        private static Dictionary<uint, RoomCollision> rooms;
        private const string DIRECTORY = "RoomColliderData";

        public static void Initialize()
        {
            rooms = new Dictionary<uint, RoomCollision>();
            string[] paths = Directory.GetFiles(DIRECTORY, "*.collision");
            foreach(string s in paths)
            {
                uint id;
                RoomCollision c = ReadFromFile(s, out id);
                if (!rooms.ContainsKey(id))
                    rooms[id] = c;
                else
                    Program.Log($"Encountered a duplicate collision data room id for path {s}, room id {id}, skipping!", LogType.Warning);
            }
        }

        private static RoomCollision ReadFromFile(string path, out uint roomId)
        {
            RoomCollision c = new RoomCollision();
            List<Collider> cols = new List<Collider>();
            using (StreamReader sr = new StreamReader(path))
            {
                roomId = uint.Parse(sr.ReadLine());
                while(!sr.EndOfStream)
                {
                    string[] tokens = sr.ReadLine().Split('|');
                    Collider col = ColliderFromOffsetSize(int.Parse(tokens[1]), int.Parse(tokens[2]),
                        int.Parse(tokens[3]), int.Parse(tokens[4]));
                    col.objectName = tokens[0];
                    cols.Add(col);
                }
                c.roomColliders = cols.ToArray();
            }
            return c;
        }

        private static Collider ColliderFromOffsetSize(int offsetX, int offsetY, int sizeX, int sizeY)
        {
            Collider c = new Collider();
            bool oddSizeX = sizeX % 2 != 0;
            c.pixelLeft = offsetX - sizeX / 2;
            c.pixelRight = offsetX + sizeX / 2 + (oddSizeX ? 1 : 0);
            bool oddSizeY = sizeY % 2 != 0;
            c.pixelBottom = offsetY - sizeY / 2;
            c.pixelTop = offsetY + sizeY / 2 + (oddSizeY ? 1 : 0);
            return c;
        }
        #endregion

        private struct Collider
        {
            public string objectName;
            public int pixelLeft;
            public int pixelRight;
            public int pixelTop;
            public int pixelBottom;
        }

        private Collider[] roomColliders;

        public bool TryCollide(int[] pos)
        {
            for(int i = 0; i < roomColliders.Length; i++)
            {
                if(TryCollide(roomColliders[i], pos))
                {
                    return true;
                }
            }
            return false;
        }

        private bool TryCollide(Collider c, int[] pos)
        {
            if(pos[0] > c.pixelLeft && pos[0] < c.pixelRight
                && pos[1] > c.pixelBottom && pos[1] < c.pixelTop)
            {
                return true;
            }
            return false;
        }

        
    }
}
