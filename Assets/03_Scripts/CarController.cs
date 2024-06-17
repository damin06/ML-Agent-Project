using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WheelPos
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

    [SerializeField] private ParticleSystem WheelParticle;
    public ParticleSystem wheelParticle => WheelParticle;

    [SerializeField] private TrailRenderer WheelTraill;
    public TrailRenderer wheelTraill => WheelTraill;
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
    [SerializeField] private float _steeringSpeed = 0.5f;

    [Range(10, 45)]
    [SerializeField] private float _maxSteerAngle = 27;

    [Range(1, 10)]
    [SerializeField] private int handbrakeDriftMultiplier = 5;

    [Range(1, 10)]
    [SerializeField] private int decelerationMultiplier = 2;

    [SerializeField] private float _antiRoll = 5000;

    [SerializeField] private SerializableDict<WheelPos, Wheel> _wheels;

    [SerializeField] private Material _ConeMaterial;

    [SerializeField] private Vector3 _centerOfMass;

    private float horizontalInput, verticalInput;
    private float currentSteerAngle, currentbreakForce;
    private bool isBreaking;
    private bool cancleBreak;
    private bool deceleratingCar;

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
    private float _driftingAxis;


    WheelFrictionCurve FLwheelFriction;
    float FLWextremumSlip;
    WheelFrictionCurve FRwheelFriction;
    float FRWextremumSlip;
    WheelFrictionCurve RLwheelFriction;
    float RLWextremumSlip;
    WheelFrictionCurve RRwheelFriction;
    float RRWextremumSlip;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        _rb.centerOfMass = _centerOfMass;

        FLwheelFriction = new WheelFrictionCurve();
        FLwheelFriction.extremumSlip = _wheels[WheelPos.FrontLeft].wheelCollider.sidewaysFriction.extremumSlip;
        FLWextremumSlip = _wheels[WheelPos.FrontLeft].wheelCollider.sidewaysFriction.extremumSlip;
        FLwheelFriction.extremumValue = _wheels[WheelPos.FrontLeft].wheelCollider.sidewaysFriction.extremumValue;
        FLwheelFriction.asymptoteSlip = _wheels[WheelPos.FrontLeft].wheelCollider.sidewaysFriction.asymptoteSlip;
        FLwheelFriction.asymptoteValue = _wheels[WheelPos.FrontLeft].wheelCollider.sidewaysFriction.asymptoteValue;
        FLwheelFriction.stiffness = _wheels[WheelPos.FrontLeft].wheelCollider.sidewaysFriction.stiffness;

        FRwheelFriction = new WheelFrictionCurve();
        FRwheelFriction.extremumSlip = _wheels[WheelPos.FrontRight].wheelCollider.sidewaysFriction.extremumSlip;
        FRWextremumSlip = _wheels[WheelPos.FrontRight].wheelCollider.sidewaysFriction.extremumSlip;
        FRwheelFriction.extremumValue = _wheels[WheelPos.FrontRight].wheelCollider.sidewaysFriction.extremumValue;
        FRwheelFriction.asymptoteSlip = _wheels[WheelPos.FrontRight].wheelCollider.sidewaysFriction.asymptoteSlip;
        FRwheelFriction.asymptoteValue = _wheels[WheelPos.FrontRight].wheelCollider.sidewaysFriction.asymptoteValue;
        FRwheelFriction.stiffness = _wheels[WheelPos.FrontRight].wheelCollider.sidewaysFriction.stiffness;

        RLwheelFriction = new WheelFrictionCurve();
        RLwheelFriction.extremumSlip = _wheels[WheelPos.RearLeft].wheelCollider.sidewaysFriction.extremumSlip;
        RLWextremumSlip = _wheels[WheelPos.RearLeft].wheelCollider.sidewaysFriction.extremumSlip;
        RLwheelFriction.extremumValue = _wheels[WheelPos.RearLeft].wheelCollider.sidewaysFriction.extremumValue;
        RLwheelFriction.asymptoteSlip = _wheels[WheelPos.RearLeft].wheelCollider.sidewaysFriction.asymptoteSlip;
        RLwheelFriction.asymptoteValue = _wheels[WheelPos.RearLeft].wheelCollider.sidewaysFriction.asymptoteValue;
        RLwheelFriction.stiffness = _wheels[WheelPos.RearLeft].wheelCollider.sidewaysFriction.stiffness;

        RRwheelFriction = new WheelFrictionCurve();
        RRwheelFriction.extremumSlip = _wheels[WheelPos.RearRight].wheelCollider.sidewaysFriction.extremumSlip;
        RRWextremumSlip = _wheels[WheelPos.RearRight].wheelCollider.sidewaysFriction.extremumSlip;
        RRwheelFriction.extremumValue = _wheels[WheelPos.RearRight].wheelCollider.sidewaysFriction.extremumValue;
        RRwheelFriction.asymptoteSlip = _wheels[WheelPos.RearRight].wheelCollider.sidewaysFriction.asymptoteSlip;
        RRwheelFriction.asymptoteValue = _wheels[WheelPos.RearRight].wheelCollider.sidewaysFriction.asymptoteValue;
        RRwheelFriction.stiffness = _wheels[WheelPos.RearRight].wheelCollider.sidewaysFriction.stiffness;
    }

    private void OnEnable()
    {
        transform.position = VehicleArea.GetSpawnPos(VehicleArea.Instance._minPos, VehicleArea.Instance._maxPos);
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
        //Stabilizer();

        if (horizontalInput == 0 && _steeringAxis != 0)
        {
            ResetSteeringAngle();
        }

        if (isBreaking)
        {
            //CancelInvoke("DecelerateCar");
            HandBreak();
            deceleratingCar = false;
        }

        if (verticalInput != 0)
        {
            //CancelInvoke("DecelerateCar");
            deceleratingCar = false;
        }

        if (verticalInput == 0 && !isBreaking && !deceleratingCar)
        {
            //InvokeRepeating("DecelerateCar", 0f, 0.1f);
            deceleratingCar = true;
        }
    }

    private void GetInput()
    {
        // Steering Input
        horizontalInput = Input.GetAxis("Horizontal");
        
        // Acceleration Input
        //verticalInput = Input.GetAxis("Vertical");
        verticalInput = 1;

        // Breaking Input
        isBreaking = Input.GetKey(KeyCode.Space);

        if(Input.GetKeyUp(KeyCode.Space))
        {
            RecoverTraction();
        }
    }

    private void Stabilizer()
    {
        WheelHit hit;
        float travelL = 1;
        float travelR = 1;

        var groundedL = _wheels[WheelPos.FrontLeft].wheelCollider.GetGroundHit(out hit);
        if (groundedL)
            travelL = (-_wheels[WheelPos.FrontLeft].wheelCollider.transform.InverseTransformPoint(hit.point).y - _wheels[WheelPos.FrontLeft].wheelCollider.radius) / _wheels[WheelPos.FrontLeft].wheelCollider.suspensionDistance;

        var groundedR = _wheels[WheelPos.FrontRight].wheelCollider.GetGroundHit(out hit);
        if (groundedR)
            travelR = (-_wheels[WheelPos.FrontRight].wheelCollider.transform.InverseTransformPoint(hit.point).y - _wheels[WheelPos.FrontRight].wheelCollider.radius) / _wheels[WheelPos.FrontRight].wheelCollider.suspensionDistance;

        var antiRollForce = (travelL - travelR) * _antiRoll;

        if (groundedL)
            _rb.AddForceAtPosition(_wheels[WheelPos.FrontLeft].wheelCollider.transform.up * -antiRollForce,
                   _wheels[WheelPos.FrontLeft].wheelCollider.transform.position);
        if (groundedR)
            _rb.AddForceAtPosition(_wheels[WheelPos.FrontRight].wheelCollider.transform.up * antiRollForce,
                   _wheels[WheelPos.FrontLeft].wheelCollider.transform.position);
    }

    private void Acceleration()
    {
        CarParticle();

        _throttleAxis = Mathf.Clamp(_throttleAxis + verticalInput * (Time.deltaTime * 3), -1, 1);

        if(_localVelocityZ < -1 && verticalInput > 0)
        {
            ApplyBreaking();

            _throttleAxis = 0;
            _throttleAxis = Mathf.Clamp(_throttleAxis + verticalInput * (Time.deltaTime * 3), -1, 1);
            return;
        }

        if(_localVelocityZ > 1 && verticalInput < 0)
        {
            ApplyBreaking();

            _throttleAxis = 0;
            _throttleAxis = Mathf.Clamp(_throttleAxis + verticalInput * (Time.deltaTime * 3), -1, 1);
            return;
        }

        float MaxSpeed = verticalInput > 0 ? _maxSpeed : _maxReverseSpeed;

        if (Mathf.Abs(Mathf.RoundToInt(_carSpeed)) < MaxSpeed)
        {
            var _dict = _wheels.GetDict();

            foreach (var _whel in _dict)
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

    //private void DecelerateCar()
    //{
    //    if(verticalInput != 0)
    //    {
    //        //CancelInvoke("DecelerateCar");
    //        return;
    //    }

    //    CarParticle();

    //    if (_throttleAxis != 0f)
    //    {
    //        if (_throttleAxis > 0f)
    //        {
    //            _throttleAxis -= Time.deltaTime * 10f;
    //        }
    //        else if (_throttleAxis < 0f)
    //        {
    //            _throttleAxis += Time.deltaTime * 10f;
    //        }
    //        if (Mathf.Abs(_throttleAxis) < 0.15f)
    //        {
    //            _throttleAxis = 0f;
    //        }
    //    }

    //    _rb.velocity *=  1f / (1f + (0.025f * decelerationMultiplier));
        
    //    var _dict = _wheels.GetDict();

    //    foreach (var _wheel in _dict)
    //    {
    //        _wheel.Value.wheelCollider.motorTorque = 0;
    //    }

    //    //Debug.Log($"rb : {_rb.velocity} , wheel : {_wheels[WheelPos.FrontLeft].wheelCollider.motorTorque}");
    //    if (_rb.velocity.magnitude < 0.25f)
    //    {
    //        _rb.velocity = Vector3.zero;
    //        CancelInvoke("DecelerateCar");
    //    }
    //}

    private void ApplyBreaking()
    {
        //Debug.Log("break" + Time.time);
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

    private void HandBreak()
    {
        _isTractionLocked = true;

        _driftingAxis += Time.deltaTime;
        float secureStartingPoint = _driftingAxis * FLWextremumSlip * handbrakeDriftMultiplier;

        if (secureStartingPoint < FLWextremumSlip)
        {
            _driftingAxis = Mathf.Clamp(FLWextremumSlip / (FLWextremumSlip * handbrakeDriftMultiplier), 0, 1);
        }

        //if (Mathf.Abs(_localVelocityX) > 2.5f)
        //{
        //    isDrifting = true;
        //}
        //else
        //{
        //    isDrifting = false;
        //

        FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * _driftingAxis;
        _wheels[WheelPos.FrontLeft].wheelCollider.sidewaysFriction = FLwheelFriction;

        FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * _driftingAxis;
        _wheels[WheelPos.FrontRight].wheelCollider.sidewaysFriction = FRwheelFriction;

        RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * _driftingAxis;
        _wheels[WheelPos.RearLeft].wheelCollider.sidewaysFriction = RLwheelFriction;

        RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * _driftingAxis;
        _wheels[WheelPos.RearRight].wheelCollider.sidewaysFriction = RRwheelFriction;



        CarParticle();
    }

    private void RecoverTraction()
    {
        _isTractionLocked = false;
        _driftingAxis = Mathf.Clamp(_driftingAxis - (Time.deltaTime / 1.5f), 0, 1);


        if (FLwheelFriction.extremumSlip > FLWextremumSlip)
        {
            FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * _driftingAxis;
            _wheels[WheelPos.FrontLeft].wheelCollider.sidewaysFriction = FLwheelFriction;

            FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * _driftingAxis;
            _wheels[WheelPos.FrontRight].wheelCollider.sidewaysFriction = FRwheelFriction;

            RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * _driftingAxis;
            _wheels[WheelPos.RearLeft].wheelCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * _driftingAxis;
            _wheels[WheelPos.RearRight].wheelCollider.sidewaysFriction = RRwheelFriction;

            Invoke("RecoverTraction", Time.deltaTime);

        }
        else if (FLwheelFriction.extremumSlip < FLWextremumSlip)
        {
            FLwheelFriction.extremumSlip = FLWextremumSlip;
            _wheels[WheelPos.FrontLeft].wheelCollider.sidewaysFriction = FLwheelFriction;

            FRwheelFriction.extremumSlip = FRWextremumSlip;
            _wheels[WheelPos.FrontRight].wheelCollider.sidewaysFriction = FRwheelFriction;

            RLwheelFriction.extremumSlip = RLWextremumSlip;
            _wheels[WheelPos.RearLeft].wheelCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip;
            _wheels[WheelPos.RearRight].wheelCollider.sidewaysFriction = RRwheelFriction;

            _driftingAxis = 0f;
        }
    }

    private void CarParticle()
    {
        var _dict = _wheels.GetDict();

        if (Mathf.Abs(_localVelocityX) > 2.5f)
        {
            foreach (var _item in _dict)
            {
                if(_item.Value.wheelParticle != null)
                    _item.Value.wheelParticle.Play();
            }
        }
        else
        {
            foreach (var _item in _dict)
            {
                if (_item.Value.wheelParticle != null)
                    _item.Value.wheelParticle?.Stop();
            }
        }

        if ((_isTractionLocked || Mathf.Abs(_localVelocityX) > 5f) && Mathf.Abs(_carSpeed) > 12f)
        {
            foreach (var _item in _dict)
            {
                if (_item.Value.wheelTraill != null)
                    _item.Value.wheelTraill.emitting = true;
            }
        }
        else
        {
            foreach (var _item in _dict)
            {
                if (_item.Value.wheelTraill != null)
                    _item.Value.wheelTraill.emitting = false;
            }
        }
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

    private void OnCollisionEnter(Collision collision)
    {


        //Vector3 vec = transform.InverseTransformDirection(collision.transform.position);
        Vector3 vec = transform.InverseTransformPoint(collision.transform.position);
        //Debug.Log("no : " + vec);
        //Debug.Log(Mathf.Atan2(vec.z, vec.x) * Mathf.Rad2Deg);


        //Debug.Log(collision.contacts.);
        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 _hitPos = transform.InverseTransformPoint(contact.point);

            if(_hitPos.z > 2 && Mathf.Abs(_hitPos.x) <= 1)
            {
                if (contact.otherCollider.gameObject.tag == "Vehicle" || contact.otherCollider.gameObject.tag == "Wall")
                {
                    Debug.Log("hit Pos : " + _hitPos);

                }
            }
            //Debug.DrawRay(contact.point, contact.normal, Color.white);
            //Debug.Log(Mathf.Atan2(contact.point.z, contact.point.x) * Mathf.Rad2Deg);

        }

        //Debug.Log(collision.contacts[0].point);
        //    if (vec.z > 2.7f)
        //{
        //    Debug.Log("hit : " + vec);
        //}
        //else
        //{
        //}
    }
}
