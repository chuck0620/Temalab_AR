
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

enum ActivePlayer
{
    PLAYER1,
    PLAYER2
}

public class PlacementIndicator : MonoBehaviour
{
    public GameObject objectToPlace;
    public GameObject hitDetectionObject;
    private GameObject placedHitDetectionObject;
    private GameObject placedMap;
    private List<GameObject> player1Ships;
    private List<GameObject> player2Ships;
    public GameObject mapToPlace;
    private ARRaycastManager rayManager;
    private GameObject visual;
    private Pose placementPose;
    private Boolean isFirstPlacement = true;
    private Boolean placementPhase = true;
    private ActivePlayer activePlayer;

    private BoxCollider boxCollider;
    

    void Start()
    {
        // get the components
        rayManager = FindObjectOfType<ARRaycastManager>();
        visual = transform.GetChild(0).gameObject;

        // hide the placement indicator visual
        visual.SetActive(false);
        player1Ships = new List<GameObject>();
        player2Ships = new List<GameObject>();
        activePlayer = ActivePlayer.PLAYER2;

    }

    void Update()
    {
        // shoot a raycast from the center of the screen
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        rayManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.Planes);

        // if we hit an AR plane surface, update the position and rotation
        if (hits.Count > 0)
        {
            var cameraForward = Camera.current.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            transform.position = hits[0].pose.position;
            placementPose.position = hits[0].pose.position;
            transform.rotation = hits[0].pose.rotation;
            placementPose.rotation = hits[0].pose.rotation;
            if (placedMap != null)
            {
                placementPose.position.y = placedMap.transform.position.y;
                Vector3 temp = new Vector3(transform.position.x, placedMap.transform.position.y, transform.position.z);
                transform.position = temp;
            }
            // enable the visual if it's disabled
            if (!visual.activeInHierarchy)
                visual.SetActive(true);
            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && placementPhase)
            {
                PlaceObject();
            }
            else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && !placementPhase)
            {
                Shoot();
            }
        }
        
    }

    private void Shoot()
    {
        if (activePlayer == ActivePlayer.PLAYER1)
        {
            if (boxCollider.bounds.Contains(placementPose.position) && placementPose.position.z > placedMap.transform.position.z)
            {
                placedHitDetectionObject = Instantiate(hitDetectionObject, placementPose.position, placementPose.rotation);
                for (int i = 0; i < player2Ships.Count; i++)
                {
                    var item = player2Ships[i];
                    if (placedHitDetectionObject.GetComponentInChildren<SphereCollider>()
                        .bounds.Intersects(item.GetComponentInChildren<MeshCollider>().bounds))
                    {
                        player2Ships.Remove(item);
                        Destroy(item);
                        Debug.Log("Battleship hit!");
                        

                    }
                }
                activePlayer = ActivePlayer.PLAYER2;
                TurnOver();
            }
        }
        else
        {
            if (boxCollider.bounds.Contains(placementPose.position) && placementPose.position.z < placedMap.transform.position.z)
            {
                placedHitDetectionObject = Instantiate(hitDetectionObject, placementPose.position, placementPose.rotation);
                for(int i = 0; i<player1Ships.Count;i++)
                {
                    var item = player1Ships[i];
                    if (placedHitDetectionObject.GetComponentInChildren<SphereCollider>()
                        .bounds.Intersects(item.GetComponentInChildren<MeshCollider>().bounds))
                    {
                        player1Ships.Remove(item);
                        Destroy(item);
                        Debug.Log("Battleship hit!");
                    }
                }
                activePlayer = ActivePlayer.PLAYER1;
                TurnOver();
            }
        }
    }

    private void TurnOver()
    {
        Debug.Log("The turn ended!");
        DisabbleAllShips();
        WaitForNextTurn();
    }

    private void DisabbleAllShips()
    {
        foreach (var item in player1Ships)
        {
            var meshRenderers = new List<MeshRenderer>();
            meshRenderers.AddRange(item.GetComponentsInChildren<MeshRenderer>());

            foreach (var renderer in meshRenderers)
            {
                renderer.enabled = !renderer.enabled;
            }
        }
        foreach (var item in player2Ships)
        {
            var meshRenderers = new List<MeshRenderer>();
            meshRenderers.AddRange(item.GetComponentsInChildren<MeshRenderer>());

            foreach (var renderer in meshRenderers)
            {
                renderer.enabled = !renderer.enabled;
            }
        }
    }

    private IEnumerator WaitForNextTurn()
    {
        yield return new WaitForSecondsRealtime (5);
        RevealInactivePlayer();
    }

    private void RevealInactivePlayer()
    {
        switch (activePlayer)
        {
            case ActivePlayer.PLAYER1:
                foreach (var item in player2Ships)
                {
                    var meshRenderers = new List<MeshRenderer>();
                    meshRenderers.AddRange(item.GetComponentsInChildren<MeshRenderer>());

                    foreach (var renderer in meshRenderers)
                    {
                        renderer.enabled = !renderer.enabled;
                    }
                }
                break;
            case ActivePlayer.PLAYER2:
                foreach (var item in player1Ships)
                {
                    var meshRenderers = new List<MeshRenderer>();
                    meshRenderers.AddRange(item.GetComponentsInChildren<MeshRenderer>());

                    foreach (var renderer in meshRenderers)
                    {
                        renderer.enabled = !renderer.enabled;
                    }
                }
                break;
            default:
                break;
        }
    }

    private void PlaceObject()
    {
        if (isFirstPlacement)
        {
            
            isFirstPlacement = false;
            placedMap = Instantiate(mapToPlace, placementPose.position, placementPose.rotation);
            boxCollider = placedMap.GetComponentInChildren<BoxCollider>();
            if (boxCollider == null)
            {
                Debug.Log("Cannot find BoxCollider!");
            }

        }
        else
        {
            if (boxCollider.bounds.Contains(placementPose.position) 
                && player1Ships.Count < 5 
                && placementPose.position.z < placedMap.transform.position.z)
            {
                GameObject temp = Instantiate(objectToPlace, placementPose.position, placementPose.rotation);
                Boolean intersects = false;
                foreach (var item in player1Ships)
                {
                    if (temp.GetComponentInChildren<MeshCollider>().bounds.Intersects(item.GetComponentInChildren<MeshCollider>().bounds))
                    {
                        intersects = true;
                    }
                }
                if (!intersects)
                {
                    player1Ships.Add(temp);
                }
                else
                {
                    Destroy(temp);
                }
                if (player1Ships.Count >= 5)
                {
                    foreach (GameObject item in player1Ships)
                    {
                        var meshRenderers = new List<MeshRenderer>();
                        meshRenderers.AddRange(item.GetComponentsInChildren<MeshRenderer>());
                        foreach (var renderer in meshRenderers)
                        {
                            renderer.enabled = false;
                        }
                    }
                }
            }
            else if(boxCollider.bounds.Contains(placementPose.position) 
                && player2Ships.Count < 5 && player1Ships.Count>= 5 
                && placementPose.position.z > placedMap.transform.position.z)
            {
                GameObject temp = Instantiate(objectToPlace, placementPose.position, placementPose.rotation);
                Boolean intersects = false;
                foreach (var item in player2Ships)
                {
                    if (temp.GetComponentInChildren<MeshCollider>().bounds.Intersects(item.GetComponentInChildren<MeshCollider>().bounds))
                    {
                        intersects = true;
                    }
                }
                if (!intersects)
                {
                    player2Ships.Add(temp);
                }
                else
                {
                    Destroy(temp);
                }
                if (player2Ships.Count == 5)
                {
                    placementPhase = false;
                }
            }

        }

    }
}