using UnityEngine;

public class PlayerObscuredCheck : MonoBehaviour
{
    /* Idk where to put this behavior so Im making a new class*/
    private Camera mainCamera;
    [ReadOnlyInspector] public bool isObscured { get; private set; }

    void Awake()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }
    void FixedUpdate()
    {
        Vector3 dir = mainCamera.transform.position - transform.position;
        Debug.DrawRay(transform.position, dir, Color.darkRed, 0.5f);

        if (!Physics.Raycast(transform.position, dir)) // doesn't hit something, can reach camera
        {
            Debug.Log("I can see you");
            isObscured = false;
        }
        else
        {
            Debug.Log("Hidden");
            isObscured = true;
        }
    }
}
