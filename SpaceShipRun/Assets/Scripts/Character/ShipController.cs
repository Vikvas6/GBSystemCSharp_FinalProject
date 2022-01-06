using Main;
using Mechanics;
using Network;
using UI;
using UnityEngine;
using UnityEngine.Networking;

namespace Characters
{
    public class ShipController : NetworkMovableObject
    {
        public string PlayerName
        {
            get => playerName;
            set => playerName = value;
        }

        protected override float speed => shipSpeed;

        [SerializeField] private Transform cameraAttach;
        private CameraOrbit cameraOrbit;
        private PlayerLabel playerLabel;
        private float shipSpeed;
        private Rigidbody rb;

        [SyncVar] private string playerName;

        private void OnGUI()
        {
            if (cameraOrbit == null)
            {
                return;
            }
            cameraOrbit.ShowPlayerLabels(playerLabel);
        }

        public override void OnStartAuthority()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                return;
            }

            playerName = ((SolarSystemNetworkManager) NetworkManager.singleton).playerName;
            gameObject.name = playerName;
            CmdSetClientName(playerName);
            cameraOrbit = FindObjectOfType<CameraOrbit>();
            cameraOrbit.Initiate(cameraAttach == null ? transform : cameraAttach);
            playerLabel = GetComponentInChildren<PlayerLabel>();
            base.OnStartAuthority();
        }

        protected override void HasAuthorityMovement()
        {
            var spaceShipSettings = SettingsContainer.Instance?.SpaceShipSettings;
            if (spaceShipSettings == null)
            {
                return;
            }

            var isFaster = Input.GetKey(KeyCode.LeftShift);
            var speed = spaceShipSettings.ShipSpeed;
            var faster = isFaster ? spaceShipSettings.Faster : 1.0f;

            shipSpeed = Mathf.Lerp(shipSpeed, speed * faster,
                SettingsContainer.Instance.SpaceShipSettings.Acceleration);

            var currentFov = isFaster
                ? SettingsContainer.Instance.SpaceShipSettings.FasterFov
                : SettingsContainer.Instance.SpaceShipSettings.NormalFov;
            cameraOrbit.SetFov(currentFov, SettingsContainer.Instance.SpaceShipSettings.ChangeFovSpeed);

            var velocity = cameraOrbit.transform.TransformDirection(Vector3.forward) * shipSpeed;
            rb.velocity = velocity * Time.deltaTime;

            if (!Input.GetKey(KeyCode.C))
            {
                var targetRotation = Quaternion.LookRotation(
                    Quaternion.AngleAxis(cameraOrbit.LookAngle, -transform.right) *
                    velocity);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
            }
        }

        protected override void FromServerUpdate() { }
        protected override void SendToServer() { }

        [ClientCallback]
        private void LateUpdate()
        {
            cameraOrbit?.CameraMovement();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            gameObject.name = playerName;
        }

        [Command]
        public void CmdSetClientName(string playerName)
        {
            Debug.LogError($"CmdSetClientName {playerName}");
            var networkManager = ((SolarSystemNetworkManager) NetworkManager.singleton);
            if (networkManager._playerNames.Contains(playerName))
            {
                RpcShowErrorMessage($"Name {playerName} is not unique, please, choose different name.");
            }

            playerName = playerName.Replace("%UPD%", "");
            if (!networkManager._playerNames.Contains(playerName))
            {
                networkManager._playerNames.Add(playerName);
            }

            this.playerName = playerName;
            gameObject.name = this.playerName;
            RpcSetClientName(this.playerName);
        }

        [ClientRpc]
        private void RpcSetClientName(string playerName)
        {
            if (hasAuthority)
            {
                Debug.LogError($"RpcSetClientName {playerName}");
                gameObject.name = playerName;
            }
        }

        [ClientRpc]
        private void RpcShowErrorMessage(string msg)
        {
            if (hasAuthority)
            {
                Debug.LogError(msg);
                NetworkManager.singleton.StopHost();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasAuthority)
            {
                Debug.LogError("Collision!");
                ((SolarSystemNetworkManager) NetworkManager.singleton).RecreateClient();
            }
        }
    }
}
