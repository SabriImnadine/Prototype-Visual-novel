using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Wheel Physics")]
    [SerializeField] private Rigidbody2D frontWheel;
    [SerializeField] private Rigidbody2D rearWheel;
    [SerializeField] private Rigidbody2D carBody;

    [Header("Settings")]
    [SerializeField] private float driveForce = 150f;
    [SerializeField] private float turnForce = 300f;

    private float steeringInput;

    void Update()
    {
        
        steeringInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        
        if (Mathf.Abs(steeringInput) > 0.01f)
        {
            float torqueAmount = steeringInput * driveForce * Time.fixedDeltaTime;

            frontWheel.AddTorque(-torqueAmount);
            rearWheel.AddTorque(-torqueAmount);

          
            carBody.AddTorque(steeringInput * turnForce * Time.fixedDeltaTime);
        }
    }
}
