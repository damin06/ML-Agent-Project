using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cone : MonoBehaviour
{
    private Transform m_owner;

    public void Init(Transform _owner, Material _material)
    {
        m_owner = _owner;
        
        GetComponent<MeshRenderer>().material = _material;
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if(collision.transform != m_owner.transform)
    //    {

    //    }
    //}
}
