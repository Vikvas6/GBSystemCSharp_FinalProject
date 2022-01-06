using System.Collections.Generic;
using Characters;
using Mechanics;
using UnityEngine;
using UnityEngine.Networking;


namespace Main
{
    public class SolarSystemNetworkManager : NetworkManager
    {
        public string playerName;
        [SerializeField] private ViewDistanceLimiter distanceLimiter;

        public List<string> _playerNames = new List<string>();

        private List<PlanetOrbit> _planets;
        private List<GameObject> _players = new List<GameObject>();

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            NetworkServer.AddPlayerForConnection(conn, InstantiatePlayer(), playerControllerId);
        }

        public GameObject InstantiatePlayer()
        {
            var spawnTransform = GetStartPosition();
            var player = Instantiate(playerPrefab, spawnTransform.position, spawnTransform.rotation);
            player.GetComponent<ShipController>().PlayerName = playerName;
            _players.Add(player);
            return player;
        }

        public void RecreateClient()
        {
            playerName = "%UPD%" + playerName;
            StopClient();
            StartClient();
        }

        private void Start()
        {
            _planets = new List<PlanetOrbit>(FindObjectsOfType<PlanetOrbit>());
            distanceLimiter.StartDistanceChecking(this);
        }

        public List<GameObject> GetPlayers()
        {
            return _players;
        }

        public List<PlanetOrbit> GetPlanets()
        {
            return _planets;
        }
    }
}