using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ColliderInputReceiver : InputReceiver
{

    private Vector3 clickPosition;
    private Vector3 origin;
    private Vector3 hitpoint;

    Camera ARCamera;
    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();
    ARRaycastManager RaycastManager;


    private void Start()
    {
        GetARDependencies();
        //GetEditorDependencies();
    }

    private void GetEditorDependencies()
    {
        ARCamera = GameObject.Find("Camera").GetComponent<Camera>();
    }

    private void GetARDependencies()
    {
        if (Application.isEditor)
        {
            ARCamera = Camera.main;
        }
        else
        {
            ARCamera = GameObject.Find("AR Camera").GetComponent<Camera>();
            RaycastManager = GameObject.Find("AR Session Origin").GetComponent<ARRaycastManager>();
        }

    }

    private void Update()
    {
        if (Application.isEditor)
            CheckMouseInput();
        else
            CheckScreenTouches();

    }

    private void CheckMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = ARCamera.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out hit))
                return;

            origin = ray.origin;
            hitpoint = hit.point;
            clickPosition = hit.point;
            OnInputReceived();
        }
    }

    private void CheckScreenTouches()
    {


        //If Raycast logic no work, this is issue :)
        if (Input.touchCount == 0 || Input.GetTouch(0).phase != TouchPhase.Began)
            return;

        //Hit results of the touch on the screen
        if (!RaycastManager.Raycast(Input.GetTouch(0).position, m_Hits))
            return;

        RaycastHit hit;
        Ray ray = ARCamera.ScreenPointToRay(Input.GetTouch(0).position);

        if (!Physics.Raycast(ray, out hit))
            return;


        clickPosition = hit.point;
        OnInputReceived();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(origin, hitpoint);
    }

    public override void OnInputReceived()
    {
        foreach (var handler in inputHandlers)
        {
            handler.ProcessInput(clickPosition, null, null);
        }
    }
}
