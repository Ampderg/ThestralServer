using System;
using System.Collections.Generic;
using System.Text;

namespace ThestralServer
{
    //Handles all of the server-side tracking of entities. Does not communicate with clients
    class Entity
    {
        private uint clientAuthorityId;
        internal uint EntityTypeId;
        internal int PixelX { get; private set; }
        internal int PixelY { get; private set; }
        internal int VelX { get; private set; }
        internal int VelY { get; private set; }
        internal double OverrideMaxMoveSpeed { get; set; }
        internal int HalfPixelWidth
        {
            get
            {
                return EntityLibrary.entries[EntityTypeId].halfPixelWidthCol;
            }
        }
        internal int HalfPixelHeight
        {
            get
            {
                return EntityLibrary.entries[EntityTypeId].halfPixelHeightCol;
            }
        }

        internal bool DoesClientHaveAuthority(uint clientId)
        {
            return clientAuthorityId == clientId;
        }

        internal void SetAuthority(uint clientId)
        {
            clientAuthorityId = clientId;
        }

        internal int[] MoveEntity(int targetX, int targetY)
        {
            return MoveEntity(targetX, targetY, OverrideMaxMoveSpeed);
        }
        internal int[] MoveEntity(int targetX, int targetY, double maxDistance)
        {
            int prevX = PixelX;
            int prevY = PixelY;
            double deltaX = targetX - PixelX;
            double deltaY = targetY - PixelY;
            double magnitude = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
            int[] pos = new int[4] { targetX, targetY, 0, 0 };
            if(maxDistance != -1 && magnitude > maxDistance)
            {
                double normalX = deltaX / magnitude;
                double normalY = deltaY / magnitude;
                pos[0] = PixelX + (int)Math.Round(normalX * maxDistance);
                pos[1] = PixelY + (int)Math.Round(normalY * maxDistance);
            }
            pos[2] = VelX = (100 * (pos[0] - prevX)) / Program.PixelsPerUnit;
            pos[3] = VelY = (100 * (pos[1] - prevY)) / Program.PixelsPerUnit;

            uint roomId = 0;

            if(!(RoomCollision.TryCollide(roomId, pos[0] + HalfPixelWidth, pos[1] + HalfPixelHeight)
                || RoomCollision.TryCollide(roomId, pos[0] - HalfPixelWidth, pos[1] + HalfPixelHeight)
                || RoomCollision.TryCollide(roomId, pos[0] + HalfPixelWidth, pos[1] - HalfPixelHeight)
                || RoomCollision.TryCollide(roomId, pos[0] - HalfPixelWidth, pos[1] - HalfPixelHeight)))
            {
                PixelX = pos[0];
                PixelY = pos[1];
                return pos;
            }
            return new int[4] { PixelX, PixelY, 0, 0 };
        }

        internal int[] GetVelocity()
        {
            return new int[2] { VelX, VelY };
        }
    }
}
