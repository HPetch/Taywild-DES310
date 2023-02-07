using UnityEngine;

public class Door : MonoBehaviour
{
    [field: SerializeField] public Partition Partition { get; private set; }

    private void Awake()
    {
        GetComponentInChildren<DoorDebugEditor>().gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            TransitionController.Instance.Transition(Partition);
        }
    }
}