using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARTouchEventBehavior : MonoBehaviour
{
    [SerializeField] ARRaycastManager m_RaycastManager;
    [SerializeField] GameObject spawnablePrefab;

    [HideInInspector] public bool HasSpawnedBoard = false;
    [HideInInspector] public GameObject ObjectReference;


    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();

    Camera ARCamera;
    GameObject spawnedObject;


    void Start()
    {
        spawnedObject = null;
        ARCamera = Camera.main;
    }
    // Update is called once per frame
    void Update()
    {
        CheckScreenTouch();
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

            if (hit.collider.gameObject.tag == "Table")
                SpawnPrefab(hit.point);
        }
    }

    private void CheckScreenTouch()
    {
        //Check how many touches there are on the application screen
        if (Input.touchCount == 0 || HasSpawnedBoard)
            return;

        //Hit results of the touch on the screen
        if (!m_RaycastManager.Raycast(Input.GetTouch(0).position, m_Hits))
            return;

        if (Input.GetTouch(0).phase == TouchPhase.Began && spawnedObject == null)
            CheckRaycastHit();
        else if (Input.GetTouch(0).phase == TouchPhase.Moved && spawnedObject != null)
            spawnedObject.transform.position = m_Hits[0].pose.position;


        if (Input.GetTouch(0).phase == TouchPhase.Ended)
            spawnedObject = null;
    }

    void CheckRaycastHit()
    {
        RaycastHit hit;
        Ray ray = ARCamera.ScreenPointToRay(Input.GetTouch(0).position);

        if (!Physics.Raycast(ray, out hit))
            return;

        if (hit.collider.gameObject.tag == "Spawnable")
            spawnedObject = hit.collider.gameObject;
        else
            SpawnPrefab(m_Hits[0].pose.position);

    }

    private void SpawnPrefab(Vector3 spawnPosition)
    {
        if (!HasSpawnedBoard)
        {
            spawnedObject = Instantiate(spawnablePrefab, spawnPosition, Quaternion.identity);
            ObjectReference = spawnedObject;
            ObjectReference.transform.rotation = new Quaternion(0f, 180f, 0f, 1f);
            HasSpawnedBoard = true;
        }
    }
}
