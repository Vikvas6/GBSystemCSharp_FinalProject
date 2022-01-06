using Network;
using UnityEngine;

namespace Mechanics
{
    public class PlanetOrbit : NetworkMovableObject
    {
        protected override float speed => smoothTime;

        [SerializeField] private Transform aroundPoint;
        [SerializeField] private float smoothTime = .3f;
        [SerializeField] private float circleInSecond = 1f;

        [SerializeField] private float offsetSin = 1;
        [SerializeField] private float offsetCos = 1;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float viewDistance = 1;

        private float dist;
        private float currentAng;
        private Vector3 currentPositionSmoothVelocity;
        private float currentRotationAngle;

        private const float circleRadians = Mathf.PI * 2;

        private bool havePlayerNear = false;

        private void Start()
        {
            if (isServer)
            {
                dist = (transform.position - aroundPoint.position).magnitude;
            }
            Initiate(UpdatePhase.FixedUpdate);
        }

        protected override void HasAuthorityMovement()
        {
            if (!isServer)
            {
                return;
            }

            Vector3 p = aroundPoint.position;
            p.x += Mathf.Sin(currentAng) * dist * offsetSin;
            p.z += Mathf.Cos(currentAng) * dist * offsetCos;
            transform.position = p;
            currentRotationAngle += Time.deltaTime * rotationSpeed;
            currentRotationAngle = Mathf.Clamp(currentRotationAngle, 0, 361);
            if (currentRotationAngle >= 360)
            {
                currentRotationAngle = 0;
            }
            transform.rotation = Quaternion.AngleAxis(currentRotationAngle, transform.up);
            currentAng += circleRadians * circleInSecond * Time.deltaTime;

            SendToServer();
        }

        protected override void SendToServer()
        {
            serverPosition = transform.position;
            serverEuler = transform.eulerAngles;
        }

        protected override void FromServerUpdate()
        {
            if (!isClient)
            {
                return;
            }
            transform.position = Vector3.SmoothDamp(transform.position,
                serverPosition, ref currentPositionSmoothVelocity, speed);
            transform.rotation = Quaternion.Euler(serverEuler);
        }

        public PlanetData PrepareData()
        {
            PlanetData result = new PlanetData();
            result.name = gameObject.name;
            result.position = transform.position;
            result.scale = transform.localScale.x;
            result.offsetCos = offsetCos;
            result.offsetSin = offsetSin;
            result.rotationSpeed = rotationSpeed;
            result.circleInSecond = circleInSecond;
            result.viewDistance = viewDistance;
            result.planetOrbit = this;
            return result;
        }

        public void UpdateFromData(PlanetData planetData)
        {
            gameObject.name = planetData.name;
            transform.position = planetData.position;
            transform.localScale = Vector3.one * planetData.scale;
            offsetCos = planetData.offsetCos;
            offsetSin = planetData.offsetSin;
            rotationSpeed = planetData.rotationSpeed;
            circleInSecond = planetData.circleInSecond;
            viewDistance = planetData.viewDistance;
        }

        public void CopyFrom(PlanetOrbit other)
        {
            transform.position = other.transform.position;
            transform.localScale = other.transform.localScale;
            offsetCos = other.offsetCos;
            offsetSin = other.offsetSin;
            rotationSpeed = other.rotationSpeed;
            circleInSecond = other.circleInSecond;
            viewDistance = other.viewDistance;
        }

        public void SetAroundPoint(Transform aroundPoint)
        {
            this.aroundPoint = aroundPoint;
        }

        public float GetViewDistanceSqr()
        {
            return viewDistance * viewDistance;
        }

        public void ProcessPlayerNear(bool havePlayerNear)
        {
            if (havePlayerNear == this.havePlayerNear)
            {
                return;
            }

            this.havePlayerNear = havePlayerNear;
            if (havePlayerNear)
            {
                gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
            else
            {
                gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }
}
