using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseHolder : MonoBehaviour
{
    public GameObject heldObject;
    public GameObject visualStamp;
    public GameObject selectedObject;
    public GameObject inspectedObject;
    public GameObject arcology;
    public GameObject cursor;
    public GameObject boomBox;

    // Start is called before the first frame update
    void Start()
    {
        arcology = GameObject.Find("Arcology");
        boomBox = GameObject.Find("BoomBox");
    }

    // Update is called once per frame
    void Update()
    {
        MoveToMouse();
        //TakeHeldShape();
        DropObject();
        PlaceObject();
        RotateObject();
        DemolishObject();
    }

    public void MoveToMouse()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 1000))
        {
            if (heldObject != null)
            {
                Vector3 spot = new Vector3(Mathf.RoundToInt(hit.point.x), Mathf.RoundToInt(hit.point.y), Mathf.RoundToInt(hit.point.z));
                transform.position = spot;
            }
            else
            {
                if (hit.collider.gameObject.GetComponent<BlockFunctions>() != null)
                {
                    transform.position = hit.transform.position;
                    inspectedObject = hit.transform.gameObject;
                    cursor.GetComponent<MeshRenderer>().enabled = true;
                }
                else
                {
                    cursor.GetComponent<MeshRenderer>().enabled = false;
                    inspectedObject = null;
                }
            }
            
        }
    }
    
    public void TakeHeldShape()
    {
        if (heldObject != null)
        {
            gameObject.GetComponent<MeshRenderer>().enabled = true;
            GetComponent<MeshFilter>().sharedMesh = heldObject.GetComponent<MeshFilter>().sharedMesh;
        }
    }

    public void PickUpObject(GameObject pickedObj)
    {
        //Drop the held object if it exists
        if (heldObject != null)
        {
            heldObject = null;
            GameObject stamp = visualStamp.transform.GetChild(0).gameObject;
            Destroy(stamp);
        }
        transform.eulerAngles = new Vector3(0, 0, 0);
        heldObject = pickedObj;
        inspectedObject = null;
        cursor.GetComponent<MeshRenderer>().enabled = false;
        Instantiate(pickedObj, transform.position, transform.rotation, visualStamp.transform);
    }

    public void DropObject()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            if (heldObject != null)
            {
                heldObject = null;
                GameObject stamp = visualStamp.transform.GetChild(0).gameObject;
                Destroy(stamp);
            }
            
        }
        
    }

    public void PlaceObject()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (heldObject != null)
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    if (heldObject.GetComponent<BlockFunctions>().IsAffordable())
                    {
                        boomBox.GetComponent<BoomBox>().PlayConstruct();
                        GameObject newPlacement = Instantiate(heldObject, transform.position, transform.rotation);
                        newPlacement.GetComponent<BlockFunctions>().GetPlaced();
                        newPlacement.GetComponent<BoxCollider>().enabled = true;
                        newPlacement.transform.parent = arcology.transform;
                        arcology.BroadcastMessage("ProduceWellness");
                    }
                    
                }
                
            }
        }
    }

    public void RotateObject()
    {
        if (heldObject != null)
        {
            if (Input.GetButtonDown("RotObjX"))
            {
                if (heldObject.GetComponent<BlockFunctions>() != null)
                {
                    if (heldObject.GetComponent<BlockFunctions>().isXRotatable)
                    {
                        transform.Rotate(Vector3.up * 90);
                    }
                }
                
            }
            if (Input.GetButtonDown("RotObjZ"))
            {
                if (heldObject.GetComponent<BlockFunctions>() != null)
                {
                    if (heldObject.GetComponent<BlockFunctions>().isZRotatable)
                    {
                        transform.Rotate(Vector3.right * 90);
                    }
                }
                    
            }
        }
        
    }

    public void DemolishObject()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            if (inspectedObject != null)
            {
                inspectedObject.GetComponent<BlockFunctions>().Demolish();
                inspectedObject = null;
                boomBox.GetComponent<BoomBox>().PlayDemolish() ;
            }

        }

    }
}
