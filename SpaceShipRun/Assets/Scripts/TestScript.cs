using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TestScript : NetworkBehaviour
{
    [SyncVar]
    private int _value;

    private int _oldValue;

    public int value
    {
        get => _value;
        set
        {
            _value = value;
           
        }
    }

    [Command]
    public void CmdCommandOnServer()
    {
        Debug.LogError("Execute on server");
    }

    [ClientRpc]
    public void RpcCommandOnClient()
    {
        Debug.LogError("Execute on client");
    }

    public void Update()
    {
        if (_value != _oldValue)
        {
            _oldValue = _value;
            Debug.LogError(_value);


            if (isLocalPlayer)
            {
                CmdCommandOnServer();
            }
            else
            {
                RpcCommandOnClient();
            }

        }
    }
}
