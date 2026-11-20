using System;
using UnityEngine;

public class PlayerObscuredCheck : MonoBehaviour
{
    /* Idk where to put this behavior so Im making a new class*/
    private Camera mainCamera;
    public event Action<bool> OnObscuredChange;

    // the fake one
    [SerializeField, ReadOnlyInspector] bool _isObscured;
    public bool isObscured { get => _isObscured; private set => _isObscured = value; }

    void Awake()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }
    void FixedUpdate()
    {
        Vector3 dir = mainCamera.transform.position - transform.position;


        bool isObscuredRn = Physics.Raycast(transform.position, dir); // if it hits something player is covered

        if (isObscured == isObscuredRn)
            return;

        // signal on change from true=>false
        isObscured = isObscuredRn;
        OnObscuredChange?.Invoke(isObscuredRn);

    }
}
