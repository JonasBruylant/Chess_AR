using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ARTouchEventBehavior : MonoBehaviour
{
    [SerializeField] ARRaycastManager m_RaycastManager;
    [SerializeField] GameObject spawnablePrefab;
    [SerializeField] ARPlaneManager planeManager;
    [SerializeField] ChessGameController gameController;
    [SerializeField] GameObject guideTextGO;

    [HideInInspector] public bool HasSpawnedBoard = false;
    [HideInInspector] public GameObject ObjectReference;
    //[HideInInspector] public Vector3 BoardPosition;

    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();

    Camera ARCamera;
    GameObject spawnedObject;
    TMP_Text guideText;
    bool hasTextUpdated = false;

    void Start()
    {
        spawnedObject = null;
        ARCamera = Camera.main;
        guideText = guideTextGO.GetComponent<TMP_Text>();
    }
    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor)
            CheckMouseInput();
        else
            CheckScreenTouch();

        if(!hasTextUpdated && planeManager.trackables.count > 0)
        {
            guideText.text = "Tap anywhere on the surface to place the board";
            hasTextUpdated = true;
        }

    }

    private void CheckMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = ARCamera.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out hit))
                return;

            if (hit.collider.gameObject.tag == "Table" && !HasSpawnedBoard)
            {
                SpawnPrefab(hit.point);
                if (ObjectReference != null)
                    gameController.SetBoardTransform(hit.point, ObjectReference.transform.rotation);
            }
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
        {
            if (HasSpawnedBoard)
                return;

            SpawnPrefab(m_Hits[0].pose.position);
            if (ObjectReference != null)
                gameController.SetBoardTransform(m_Hits[0].pose.position, ObjectReference.transform.rotation);
        }

    }

    private void SpawnPrefab(Vector3 spawnPosition)
    {
        if (!HasSpawnedBoard)
        {
            spawnedObject = Instantiate(spawnablePrefab, spawnPosition, Quaternion.identity);
            ObjectReference = spawnedObject;
            ObjectReference.transform.rotation = new Quaternion(0f, 180f, 0f, 1f);
            HasSpawnedBoard = true;

            guideText.text = "";

            //Stop detecting and spawning planes when board has been spawned
            planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.None;

            //Hide the current planes in the scene when board has been spawned
            planeManager.SetTrackablesActive(false);

        }
    }
}
