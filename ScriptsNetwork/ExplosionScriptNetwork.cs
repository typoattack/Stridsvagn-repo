using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Netcode;

public class ExplosionScriptNetwork : NetworkBehaviour
{
    static string s_ObjectPoolTag = "ObjectPool";
    NetworkObjectPool m_ObjectPool;
    public AudioClip expsound;
    private AudioSource audio;
    bool m_isActive;

    public void Config(bool isActive)
    {
        m_isActive = isActive;
    }

    private void Awake()
    {
        m_ObjectPool = GameObject.FindWithTag(s_ObjectPoolTag).GetComponent<NetworkObjectPool>();
        Assert.IsNotNull(m_ObjectPool, $"{nameof(NetworkObjectPool)} not found in scene. Did you apply the {s_ObjectPoolTag} to the GameObject?");
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        audio = GetComponent<AudioSource>();
        audio.PlayOneShot(expsound, 1.0f);
        StartCoroutine(DespawnExpCoroutine(expsound.length));
    }

    private void Update()
    {
        if (m_isActive)
        {
            audio.PlayOneShot(expsound, 1.0f);
            m_isActive = false;
            StartCoroutine(DespawnExpCoroutine(expsound.length));
        }
    }

    private void DestroyExp()
    {
        Assert.IsTrue(NetworkManager.IsServer);

        if (!NetworkObject.IsSpawned)
        {
            return;
        }

        NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
        networkObject.Despawn(true);
        //m_ObjectPool.ReturnNetworkObject(NetworkObject);
    }

    IEnumerator DespawnExpCoroutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        DestroyExp();
    }
}
