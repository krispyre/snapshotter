using System;
using System.ComponentModel;
using UnityEngine;

public class Hurtarea : MonoBehaviour
{
    [SerializeField] private float hurtRate = 20f;
    [SerializeField, ReadOnlyInspector] private bool playerInside;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }

    void FixedUpdate()
    {
        if (playerInside)
        {
            PointSystem.Instance.DeductPoints(hurtRate);
        }
    }
}
