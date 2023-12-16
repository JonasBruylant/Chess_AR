using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ColliderInputReceiver : InputReceiver
{
    private Vector3 clickPosition;
    private Vector3 origin;
    private Vector3 hitpoint;

    Camera ARCamera;
    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();
    ARRaycastManager RaycastManager;

    Text _Touches;
    Text _RayCastHit;
    Text _RayCastManagerHit;


    private void Start()
    {
        GetARDependencies();
        //GetEditorDependencies();

        _Touches = GameObject.Find("Touches").GetComponent<Text>();
        _RayCastHit = GameObject.Find("RaycastManager hit").GetComponent<Text>();
        _RayCastManagerHit = GameObject.Find("Raycast hit").GetComponent<Text>();
    }

    private void GetEditorDependencies()
    {
        ARCamera = GameObject.Find("Camera").GetComponent<Camera>();
    }

    private void GetARDependencies()
    {
        ARCamera = GameObject.Find("AR Camera").GetComponent<Camera>();
        RaycastManager = GameObject.Find("AR Session Origin").GetComponent<ARRaycastManager>();
    }

    private void Update()
    {
        CheckScreenTouches();
        //CheckMouseInput();

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
        _Touches.text = Input.touchCount.ToString();
        if (Input.touchCount == 0)
            return;

        _RayCastManagerHit.text = RaycastManager.Raycast(Input.GetTouch(0).position, m_Hits).ToString();
        //Hit results of the touch on the screen
        if (!RaycastManager.Raycast(Input.GetTouch(0).position, m_Hits))
            return;

        RaycastHit hit;
        Ray ray = ARCamera.ScreenPointToRay(Input.GetTouch(0).position);

        if (!Physics.Raycast(ray, out hit))
            return;

        _RayCastHit.text = hit.transform.gameObject.name;

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
