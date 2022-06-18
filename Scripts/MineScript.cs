using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineScript : MonoBehaviour
{
    CircleCollider2D m_collider;

    // Start is called before the first frame update
    void Start()
    {
        m_collider = GetComponent<CircleCollider2D>();
        m_collider.enabled = false;
        StartCoroutine(SetColliderActive());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator SetColliderActive()
    {
        yield return new WaitForSeconds(2);
        m_collider.enabled = true;
        this.transform.GetChild(1).gameObject.SetActive(true);
    }
}
