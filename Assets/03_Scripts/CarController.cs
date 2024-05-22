using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum WheelPos
{
    FrontLeft,
    FrontRight,
    RearLeft,
    RearRight,
}

[System.Serializable]
public struct Wheel
{
    [SerializeField] private WheelCollider WheelCollider;
    public WheelCollider wheelCollider => WheelCollider;

    [SerializeField] private GameObject WheelMesh;
    public GameObject wheelMesh => WheelMesh;
}

public class CarController : MonoBehaviour
{
    [Header("CAR SETUP")]

    [Range(20, 190)]
    [SerializeField]private int _maxSpeed = 90;

    [Range(10, 120)]
    [SerializeField]private int _maxReverseSpeed = 45;

    [Range(1, 10)]
    [SerializeField]private int _accelerationMultiplier = 2;
    
    [Range(100, 600)]
    [SerializeField] private float _breakForce = 350;

    [Range(0.1f, 1f)]
    private float _steeringSpeed = 0.5f;

    [Range(10, 45)]
    [SerializeField] private float _maxSteerAngle = 27;

    [SerializeField] private SerializableDict<WheelPos, Wheel> _wheels;

    private float horizontalInput, verticalInput;
    private float currentSteerAngle, currentbreakForce;
    private bool isBreaking;

    //Data 
    [HideInInspector]
    public float _carSpeed; 
    [HideInInspector]
    public bool _isDrifting; 
    [HideInInspector]
    public bool _isTractionLocked;

    private Rigidbody _rb;
    private float _steeringAxis;
    private float _throttleAxis;
    private float _localVelocityZ;
    private float _localVelocityX;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        _carSpeed = (2 * Mathf.PI * _wheels[WheelPos.FrontRight].wheelCollider.radius * _wheels[WheelPos.FrontLeft].wheelCollider.rpm * 60) / 1000;
        _localVelocityX = transform.InverseTransformDirection(_rb.velocity).x;
        _localVelocityZ = transform.InverseTransformDirection(_rb.velocity).z;

        GetInput();
    }

    private void FixedUpdate()
    {
        Acceleration();
        HandleSteering();
        UpdateWheels();

        if(horizontalInput == 0 && _steeringAxis != 0)
        {
            ResetSteeringAngle();
        }
    }

    private void GetInput()
    {
        // Steering Input
        horizontalInput = Input.GetAxis("Horizontal");
        
        // Acceleration Input
        verticalInput = Input.GetAxis("Vertical");

        // Breaking Input
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    private void Acceleration()
    {
        _throttleAxis = Mathf.Clamp(_throttleAxis + verticalInput * (Time.deltaTime * 3), -1, 1);

        if(_localVelocityZ < -1 && verticalInput > 0)
        {
            ApplyBreaking();
        }
        else if(_localVelocityZ > 1 && verticalInput < 0)
        {
            ApplyBreaking();
        }
        else
        {
            float MaxSpeed = verticalInput > 0 ? _maxSpeed : _maxReverseSpeed;

            if(Mathf.Abs(Mathf.RoundToInt(_carSpeed)) < MaxSpeed)
            {
                var _dict = _wheels.GetDict();
                
                foreach(var _whel in _dict)
                {
                    _whel.Value.wheelCollider.brakeTorque = 0;
                    _whel.Value.wheelCollider.motorTorque = (_accelerationMultiplier * 50f) * _throttleAxis;
                }
            }
            else
            {
                var _dict = _wheels.GetDict();

                foreach (var _whel in _dict)
                {
                    _whel.Value.wheelCollider.motorTorque = 0;
                }
            }
        }
    }


    private void ApplyBreaking()
    {
        var _dict = _wheels.GetDict();

        foreach (var _item in _dict)
        {
            _item.Value.wheelCollider.brakeTorque = currentbreakForce;
        }
    }

    private void HandleSteering()
    {
        _steeringAxis = Mathf.Clamp(_steeringAxis + horizontalInput * (Time.deltaTime * 10 * _steeringSpeed), -1, 1);

        var steeringAngle = _steeringAxis * _maxSteerAngle;

        _wheels[WheelPos.FrontLeft].wheelCollider.steerAngle =
            Mathf.Lerp
            (
                _wheels[WheelPos.FrontLeft].wheelCollider.steerAngle,
                steeringAngle,
                _steeringSpeed
            );

        _wheels[WheelPos.FrontRight].wheelCollider.steerAngle =
            Mathf.Lerp
            (
                _wheels[WheelPos.FrontRight].wheelCollider.steerAngle,
                steeringAngle,
                _steeringSpeed
            );
    }

    public void ResetSteeringAngle()
    {
        if (_steeringAxis < 0f)
        {
            _steeringAxis = _steeringAxis + (Time.deltaTime * 10f * _steeringSpeed);
        }
        else if (_steeringAxis > 0f)
        {
            _steeringAxis = _steeringAxis - (Time.deltaTime * 10f * _steeringSpeed);
        }
        if (Mathf.Abs(_wheels[WheelPos.FrontLeft].wheelCollider.steerAngle) < 1f)
        {
            _steeringAxis = 0f;
        }

        var steeringAngle = _steeringAxis * _maxSteerAngle;

        _wheels[WheelPos.FrontLeft].wheelCollider.steerAngle =
            Mathf.Lerp
            (
                _wheels[WheelPos.FrontLeft].wheelCollider.steerAngle,
                steeringAngle,
                _steeringSpeed
            );

        _wheels[WheelPos.FrontRight].wheelCollider.steerAngle =
            Mathf.Lerp
            (
                _wheels[WheelPos.FrontRight].wheelCollider.steerAngle,
                steeringAngle,
                _steeringSpeed
            );
    }

    private void UpdateWheels()
    {
        var _dict = _wheels.GetDict();

        foreach(var _item in _dict)
        {
            UpdateSingleWheel(_item.Value.wheelCollider, _item.Value.wheelMesh.transform);
        }
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }
}
