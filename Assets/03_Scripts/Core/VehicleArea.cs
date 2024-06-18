using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VehicleArea : MonoBehaviour
{
    public static VehicleArea Instance;

    [SerializeField] private ParticleSystem _particle;
    [SerializeField] private Transform _menuCame;
    [SerializeField] private TextMeshProUGUI _textMeshPro;
    [SerializeField] private TextMeshProUGUI _scoreTxt;
    [SerializeField] Transform _qr;

    [SerializeField] private string m_gameName;
    [SerializeField] private string m_endMessage;

    public Vector3 _minPos;
    public Vector3 _maxPos;

    private int score = 0;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Update()
    {
        float size = Mathf.Sin(Time.time);
        _qr.localScale = new Vector3(size, size, size);
    }

    public void SpawnParticle(Vector3 pos)
    {
        GameObject NewParticle = Instantiate(_particle.gameObject);
        NewParticle.transform.position = pos;
    }

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

    public void GoToMenu()
    {
        _textMeshPro.text = m_endMessage + "\n" + score.ToString();
        _menuCame.gameObject.SetActive(true);
        Debug.Log(score);
        score = 0;
        _scoreTxt.text = score.ToString();

    }

    public void AddScore(int Newscore = 10)
    {
        score += Newscore;

        _scoreTxt.text = score.ToString();
    }
}
