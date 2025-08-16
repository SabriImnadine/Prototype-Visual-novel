using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class MeterController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TMP_Text label;      
    [SerializeField] private Transform runner;     

    [Header("Affichage")]
    [SerializeField] private string unitSuffix = " m"; 
    [SerializeField] private int decimals = 0;         

    float _startX;
    System.Text.StringBuilder _sb;

    void Awake()
    {
        if (!label) label = GetComponent<TMP_Text>();
        if (!runner) runner = Camera.main ? Camera.main.transform : null;
        _sb = new System.Text.StringBuilder(16);
    }

    void OnEnable()
    {
        
        _startX = runner ? runner.position.x : 0f;
        WriteDistance(0f);
    }

    void Update()
    {
        if (!runner || !label) return;

   
        float dx = Mathf.Max(0f, runner.position.x - _startX);

        WriteDistance(dx);
    }

    void WriteDistance(float meters)
    {
        _sb.Clear();
        
        float rounded = (decimals <= 0)
            ? Mathf.Round(meters)
            : Mathf.Round(meters * Mathf.Pow(10, decimals)) / Mathf.Pow(10, decimals);

        _sb.Append(rounded);
        if (!string.IsNullOrEmpty(unitSuffix)) _sb.Append(unitSuffix);
        label.text = _sb.ToString();
    }
}
