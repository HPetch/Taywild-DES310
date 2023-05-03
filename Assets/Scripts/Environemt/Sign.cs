using UnityEngine;
using TMPro;

public class Sign : MonoBehaviour
{
    private TextMeshProUGUI textComponent = null;

    private void Awake()
    {
        textComponent = GetComponentInChildren<TextMeshProUGUI>();
        textComponent.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            textComponent.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            textComponent.gameObject.SetActive(false);
        }
    }
}
