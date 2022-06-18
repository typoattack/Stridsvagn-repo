using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShieldControllerNetwork : NetworkBehaviour
{
    public bool isRandom;
    public int isActive;
    public float shieldUptime;
    public float shieldDowntime;
    private WaitForSeconds shieldActive;
    private WaitForSeconds shieldInactive;

    // Start is called before the first frame update
    void Awake()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        shieldActive = new WaitForSeconds(shieldUptime);
        shieldInactive = new WaitForSeconds(shieldDowntime);
        if (isRandom) isActive = Random.Range(0, 2);
        if (isActive == 1)
        {
            StartCoroutine(cycleShield());
        }
        else this.transform.GetChild(3).gameObject.SetActive(false);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!Spawner.Singleton) return;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // --- ClientRPCs ---

    [ClientRpc]
    public void ActivateShieldClientRpc()
    {
        this.transform.GetChild(3).gameObject.SetActive(true);
    }

    [ClientRpc]
    public void DeactivateShieldClientRpc()
    {
        this.transform.GetChild(3).gameObject.SetActive(false);
    }

    IEnumerator cycleShield()
    {
        while (true)
        {
            this.transform.GetChild(3).gameObject.SetActive(true);
            ActivateShieldClientRpc();
            yield return shieldActive;
            this.transform.GetChild(3).gameObject.SetActive(false);
            DeactivateShieldClientRpc();
            yield return shieldInactive;
        }
    }
}
