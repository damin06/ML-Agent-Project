using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleArea : MonoBehaviour
{
    public Vector3 _minPos;
    public Vector3 _maxPos;
     
    public static Vector3 GetSpawnPos(Vector3 _min, Vector3 _max)
    {
        Vector3 _newPos = Vector3.zero;

        for(int i = 0; i < 5000; i++)
        {
            _newPos.z = Random.Range(_min.z, _max.z);
            _newPos.x = Random.Range(_min.x, _max.x);
            _newPos.y = 0.3f;

            Collider[] _colliders = Physics.OverlapSphere(_newPos, 4, LayerMask.NameToLayer("Vehicle") | LayerMask.NameToLayer("Wall"));

            if (_colliders == null || _colliders.Length <= 0)
                break;
        }
        
        return _newPos;
    }
}
