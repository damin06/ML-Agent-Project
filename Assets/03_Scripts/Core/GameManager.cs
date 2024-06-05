using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;

    private List<CheckPoint> _checkPoints;

    private Dictionary<Transform, int> _agentsCheckpointIndex = new Dictionary<Transform, int>();

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(Instance);

        foreach(Transform _item in transform)
        {
            _checkPoints.Add(_item.GetComponent<CheckPoint>());
        }
    }

  
}
