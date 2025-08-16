using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BenzineController : MonoBehaviour
{
        public static BenzineController Instance { get; private set; }

    
    [Header("UI")]
    
    [SerializeField] private Image fuelBar;         

    [Header("Fuel Settings")]
    [SerializeField] private float maxFuel = 100f;  
    [SerializeField] private float drainPerSecond = 1f;
    [SerializeField] private bool autoDrain = true;  
     [Header("Color Settings")]
    [SerializeField] private Gradient fuelGradient; 

    private float fuel;                              

    void Awake()
    {
        Instance = this;
        if (fuelBar == null) fuelBar = GetComponent<Image>();
    }

    void OnEnable()
    {
        ResetFuel();
    }

    void Update()
    {
        if (!autoDrain || maxFuel <= 0f) return;

        Consume(drainPerSecond * Time.deltaTime);
    }

    public void ResetFuel()
    {
        fuel = Mathf.Max(0f, maxFuel);
        RefreshUI();
    }

       public void Consume(float amount)
    {
        if (amount <= 0f) return;
        fuel = Mathf.Clamp(fuel - amount, 0f, maxFuel);
        RefreshUI();
    }

   
    public void AddFuel(float amount)
    {
        if (amount <= 0f) return;
        fuel = Mathf.Clamp(fuel + amount, 0f, maxFuel);
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (fuelBar == null) return;


        float normalized = (maxFuel > 0f) ? fuel / maxFuel : 0f;


        fuelBar.fillAmount = normalized;


        if (fuelGradient != null)
            fuelBar.color = fuelGradient.Evaluate(normalized);
    }

public void FillFuel()
{
    fuel = maxFuel;
    RefreshUI();
}


    public float Current => fuel;
    public float Max => maxFuel;
    public float Normalized => (maxFuel > 0f) ? fuel / maxFuel : 0f;
}
