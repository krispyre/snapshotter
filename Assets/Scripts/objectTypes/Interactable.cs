using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private Elevator elevator;
    protected bool engaged;
    public string prompt;
    private Collider enterTrigger;
    protected void Awake()
    {
        enterTrigger = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("notice me");
            engaged = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("unnotice me");
            engaged = false;
        }
    }

    protected virtual void OnInteract()
    {
        Debug.Log(prompt);
    }
}
