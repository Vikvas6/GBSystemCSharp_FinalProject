using System.Threading;
using System.Threading.Tasks;
using Main;
using UnityEngine;


public class ViewDistanceLimiter : MonoBehaviour
{
    private SolarSystemNetworkManager _manager;
    private CancellationTokenSource _cancellationTokenSource;

    public void StartDistanceChecking(SolarSystemNetworkManager manager)
    {
        _manager = manager;
        _cancellationTokenSource = new CancellationTokenSource();
        var _cancellationToken = _cancellationTokenSource.Token;
        Task checkTask = new Task(() => CheckDistance(_cancellationToken));
        checkTask.Start();
    }

    private async Task CheckDistance(CancellationToken cancellationToken)
    {
        Debug.Log("Works");
        while (true)
        {
            foreach (var planet in _manager.GetPlanets())
            {
                bool havePlayerNear = false;
                foreach (var player in _manager.GetPlayers())
                {
                    if (Vector3.SqrMagnitude(player.transform.position - planet.transform.position) <
                        planet.GetViewDistanceSqr())
                    {
                        havePlayerNear = true;
                    }
                }

                planet.ProcessPlayerNear(havePlayerNear);
            }
            Debug.Log("Works2");

            await Task.Delay(1000, cancellationToken);
        }
    }

    private void OnDestroy()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}
