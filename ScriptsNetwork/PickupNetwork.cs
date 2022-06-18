using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Assertions;

public class PickupNetwork : NetworkBehaviour
{
    static string s_ObjectPoolTag = "ObjectPool";

    public static int numPickups = 0;

    NetworkObjectPool m_ObjectPool;

    public NetworkVariable<Buff.BuffType> buffType = new NetworkVariable<Buff.BuffType>();

    void Awake()
    {
        m_ObjectPool = GameObject.FindWithTag(s_ObjectPoolTag).GetComponent<NetworkObjectPool>();
        Assert.IsNotNull(m_ObjectPool, $"{nameof(NetworkObjectPool)} not found in scene. Did you apply the {s_ObjectPoolTag} to the GameObject?");
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            OnStartClient();
        }

        if (IsServer)
        {
            OnStartServer();
        }
    }

    void OnStartClient()
    {
        //float dir = 170.0f;
        //transform.rotation = Quaternion.Euler(0, 180, dir);
        //GetComponent<Rigidbody2D>().angularVelocity = dir;

        //Color color = Buff.bufColors[(int)buffType.Value];
        //GetComponent<Renderer>().material.color = color;
        this.transform.GetChild((int)buffType.Value).gameObject.SetActive(true);

        if (!IsServer)
        {
            numPickups += 1;
        }
    }

    void OnStartServer()
    {
        numPickups += 1;
    }
    /*
    void OnGUI()
    {
        GUI.color = Buff.bufColors[(int)buffType.Value];
        Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);
        GUI.Label(new Rect(pos.x - 20, Screen.height - pos.y - 30, 100, 30), buffType.Value.ToString());
    }
    */
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer)
        {
            return;
        }

        var otherTankControl = other.gameObject.GetComponent<TankControllerNetwork>();
        if (otherTankControl != null)
        {
            otherTankControl.AddBuff(buffType.Value);
            DestroyPowerUp();
        }
    }

    void DestroyPowerUp()
    {
        AudioSource.PlayClipAtPoint(GetComponent<AudioSource>().clip, transform.position);
        numPickups -= 1;

        NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
        networkObject.Despawn();
        //NetworkObject.Despawn(true);
        //m_ObjectPool.ReturnNetworkObject(NetworkObject);
    }
}
