using UnityEngine;
using CommonCore.State;

namespace CommonCore.World
{

    public class PlayerSpawnIntent : Intent
    {
        public string SpawnPoint { get; private set; } //TID of spawnpoint, use point if null
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }

        public PlayerSpawnIntent(string spawnPoint)
        {
            SpawnPoint = spawnPoint;
        }

        public PlayerSpawnIntent(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }
}