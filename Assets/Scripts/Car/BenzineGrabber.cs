using UnityEngine;

public class BenzineGrabber : MonoBehaviour
{
    [Tooltip("Hoeveelheid benzine toegevoed; <= 0 = volledig vullen")]
    [SerializeField] private float refillAmount = -1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var meter = BenzineController.Instance ?? FindObjectOfType<BenzineController>();
        if (meter == null)
        {
            Debug.LogWarning("BenzineGrabber: geen BenzineController gevonden in de scene.");
            return;
        }

        if (refillAmount <= 0f) meter.FillFuel();
        else meter.AddFuel(refillAmount);

        Destroy(gameObject);
    }
}
