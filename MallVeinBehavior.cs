using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MallVeinBehavior : MonoBehaviour
{
    public int step = 1;
    public GameObject theMall;

    //Random Direction Variance
    public int deviationChance = 10;
    public int elevationChance = 10;
    public int ascendChance = 50;

    //Room Dimensions
    public int width, depth, height;

    //Position List
    public List<Vector3> tilePositions;

    //Tile
    public GameObject tileBlock;

    //Can or cannot pass through other placed tiles
    public bool isPassable;

    public string stepType;
    public Vector3 nextStep;

    //Path List
    public List<GameObject> pathList;

    //Edge List
    public List<GameObject> edgeList;

    //Deviate Inhibitor
    public int deviateInhibitor;
    public int deviateInhibitorMax = 4;

    //Additional Veins
    public GameObject[] addVeins;
    public int numOfAddVeins;
    public int veinsGrown;

    //ParentVein
    public GameObject parentVein;
    
    //Main Hall?
    public bool isMainHall;

    //Debug Object
    public GameObject debugObject;

    //Start is called before the first frame update
    void Start()
    {
        theMall = GameObject.Find("The Mall");
    }

    //Debug Color
    private Color debugCol = Color.blue;

    //StairSpace
    public GameObject stairSpace;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Step()
    {
        
        stepType = DetermineNextStep();
        ExactLocation();
        RecordPosition();
        Debug.DrawLine(transform.position, nextStep, debugCol, 999f);
        transform.position = nextStep;
        LayoutRoom();
           
    }

    public string DetermineNextStep()
    {
        string result = "error";
        int rand = theMall.GetComponent<MallSeed>().ReturnRollToInt(100);
        //Determine whether path deviates
        //Forward
        if (rand > deviationChance || deviateInhibitor>0)
        {
            deviateInhibitor --;
            debugCol = Color.blue;
            if (deviateInhibitor < 0)
            {
                deviateInhibitor = 0;
                debugCol = Color.yellow;
            }

            //Check for forward collision
            Collider[] colliders = Physics.OverlapBox(transform.position + transform.forward * step, new Vector3(.1f, .1f, .1f), transform.rotation, LayerMask.GetMask("Tile"));
            if (colliders.Length == 0 || isPassable || colliders[0].GetComponent<ProtoTileBehavior>().mallLayer == tileBlock.GetComponent<ProtoTileBehavior>().mallLayer)
            {
                nextStep = transform.position + transform.forward * step;
                result = "forward";
            }
            else
            {
                //Halt
                nextStep = transform.position;
                depth = 0;

                ////Turn instead
                //rand = deviationChance;
                //deviateInhibitor = 0;
            }
            
        }
        //Deviate
        if (rand <= deviationChance && deviateInhibitor == 0)
        {
            deviateInhibitor = deviateInhibitorMax;
            debugCol = Color.red;
            rand = theMall.GetComponent<MallSeed>().ReturnRollToInt(100);
            //Turn
            if (rand > elevationChance)
            {
                int turnRand = theMall.GetComponent<MallSeed>().ReturnRollToInt(100);
                //Right
                if (turnRand < 50)
                {
                    transform.Rotate(transform.up * 90);
                    nextStep = transform.position + transform.forward * step;
                    result = "right";
                }
                //Left
                if (turnRand >= 50)
                {
                    transform.Rotate(transform.up * -90);
                    nextStep = transform.position + transform.forward * step;
                    result = "left";
                }
            }
            if (rand <= elevationChance)
            {
                print("elevated");
                rand = theMall.GetComponent<MallSeed>().ReturnRollToInt(100);
                //Ascend
                if (rand < ascendChance)
                {
                    nextStep = transform.position + (transform.forward * step + transform.up * step);
                    result = "up";
                }
                //Descend
                if (rand >= ascendChance)
                {
                    nextStep = transform.position + (transform.forward * step + transform.up * -step);
                    result = "down";
                }
            }
        }

        //Return the result
        return result;

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
    }

    public IEnumerator GrowPhase1()
    {
        theMall = GameObject.Find("The Mall");
        while (depth > 0)
        {
            Step();
            depth--;
            yield return new WaitForEndOfFrame();
        }

        foreach (GameObject tile in pathList)
        {
            tile.GetComponent<ProtoTileBehavior>().SetType("traversable");
        }

        AddVeins();
        //theMall.GetComponent<MallIncubator>().DrawTiles();
    }

    public void CallGrowPhase1()
    {
        StartCoroutine("GrowPhase1");
    }
     
    public void RecordPosition()
    {
        tilePositions.Add(transform.position);
    }

    public void ExactLocation()
    {
        nextStep = new Vector3(Mathf.RoundToInt(nextStep.x), Mathf.RoundToInt(nextStep.y), Mathf.RoundToInt(nextStep.z));
    }

    public void LayoutRoom()
    {
       
            Vector3 nextSpawnSpot = transform.position;
            CheckForEmptySpace(nextSpawnSpot, "path");

            float d = nextSpawnSpot.x;
                for (int w = 1; w <= width; w += step)
                {
                //Right Side
                if (w != width)
            {
                CheckForEmptySpace(nextSpawnSpot + (transform.right * w), "normal");
            }
            else
            {
                CheckForEmptySpace(nextSpawnSpot + (transform.right * w), "rightEdge");
            }
                
            
                //Left Side
                if (w != width)
            {
                CheckForEmptySpace(nextSpawnSpot + (transform.right * -w), "normal");
            }
            else
            {
                CheckForEmptySpace(nextSpawnSpot + (transform.right * -w), "leftEdge");
            }
 
                }

                for (int h = 1; h <= height; h += step)
                {

                    CheckForEmptySpace(nextSpawnSpot + (transform.up * h), "normal");

                    Vector3 nextSpawnSpot2 = nextSpawnSpot + (transform.up * h);

                    for (int w = 1; w <= width; w += step)
                    {
                        //Right Side
                        CheckForEmptySpace(nextSpawnSpot2 + (transform.right * w), "normal");
                        //Left Side
                        CheckForEmptySpace(nextSpawnSpot2 + (transform.right * -w), "normal");
                    }
                }   
    }

    public void CheckForEmptySpace(Vector3 input, string type)
    {
     

        Collider[] colliders = Physics.OverlapBox(input, new Vector3(.1f, .1f, .1f), transform.rotation, LayerMask.GetMask("Tile"));
        if (colliders.Length == 0)
        {
         
            StampTile(input, type);
        }
        else
        {
            foreach (Collider collider in colliders)
            {

                if (collider.GetComponent<ProtoTileBehavior>() != null &&
                    type == "path")
                {
                    collider.GetComponent<ProtoTileBehavior>().SetType("floor");
                    collider.GetComponent<ProtoTileBehavior>().SetType("traversable");
                    if (stepType == "up")
                    {
                        collider.GetComponent<ProtoTileBehavior>().SetType("stair");
                        collider.transform.rotation = transform.rotation;
                        stepType = "";
                    }
                    if (stepType == "down")
                    {
                        collider.GetComponent<ProtoTileBehavior>().SetType("descend");
                        collider.transform.rotation = transform.rotation;
                        stepType = "";
                    }
                }
                else if (collider.gameObject.tag == "StairSpace")
                {
                    //Do nothing
                }
            }
        }
    }

    public void StampTile(Vector3 input, string type)
    {
        GameObject newTile = Instantiate(tileBlock, input, transform.rotation, theMall.transform);
        SetRoomReference(newTile);

        if (type == "path")
        {
            newTile.GetComponent<ProtoTileBehavior>().SetType("floor");
            newTile.GetComponent<ProtoTileBehavior>().SetType("traversable");
            pathList.Add(newTile);
        }

        if (type == "rightEdge")
        {
            edgeList.Add(newTile);
            newTile.transform.Rotate(transform.up * 90);
        }
        if (type == "leftEdge")
        {
            edgeList.Add(newTile);
            newTile.transform.Rotate(transform.up * -90);
        }

        if (stepType == "up")
        {
            newTile.GetComponent<ProtoTileBehavior>().SetType("stair");
            pathList.Add(newTile);

            Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(.3f, .3f, .3f), transform.rotation, LayerMask.GetMask("Tile"));



            foreach (Collider col in colliders)
            {
                if (col.transform.gameObject.GetComponent<ProtoTileBehavior>())
                {
                    Instantiate(debugObject, transform.position, Quaternion.identity);
                    col.GetComponent<ProtoTileBehavior>().SetType("noRail");
                }
            }

            Instantiate(stairSpace, transform.position + transform.forward * -1, transform.rotation, theMall.transform);
            Instantiate(stairSpace, transform.position + transform.forward + transform.up, transform.rotation, theMall.transform);
            Instantiate(stairSpace, transform.position + (transform.forward * -1) + transform.up, transform.rotation, theMall.transform);

            //StairLast();
            stepType = "";
        }
        if (stepType == "down")
        {
            newTile.GetComponent<ProtoTileBehavior>().SetType("descend");
            pathList.Add(newTile);

            print("Step Down");

            Collider[] colliders = Physics.OverlapBox(transform.position + ((transform.forward * -1) + (transform.up)), new Vector3(.3f, .3f, .3f), transform.rotation, LayerMask.GetMask("Tile"));

            

            foreach(Collider col in colliders)
            {
                if (col.transform.gameObject.GetComponent<ProtoTileBehavior>())
                {
                    Instantiate(debugObject, transform.position + ((transform.forward * -1) + (transform.up)), Quaternion.identity);
                    col.GetComponent<ProtoTileBehavior>().SetType("noRail");
                }
            }

            Instantiate(stairSpace, transform.position + transform.up, transform.rotation,theMall.transform);
            Instantiate(stairSpace, transform.position + transform.forward, transform.rotation, theMall.transform);
            Instantiate(stairSpace, transform.position + transform.up + transform.forward, transform.rotation, theMall.transform);

            //StairLast();
            stepType = "";
        }
    }

    public void AddVeins()
    {
        PruneEdgeList();
        for(int i = 0; i < numOfAddVeins; i++)
        {
            int veinType = theMall.GetComponent<MallSeed>().ReturnRollToInt(addVeins.Length);
            int veinPosChoice = theMall.GetComponent<MallSeed>().ReturnRollToInt(edgeList.Count);
            Vector3 veinPosition = edgeList[veinPosChoice].gameObject.transform.position;
            print(veinType);
            print("VeinPosChoice is: " + veinPosChoice);

            GameObject newVein = Instantiate(addVeins[veinType], veinPosition, edgeList[veinPosChoice].transform.rotation, theMall.transform);
            newVein.GetComponent<MallVeinBehavior>().CallGrowPhase1();
            newVein.GetComponent<MallVeinBehavior>().parentVein = this.gameObject;
        }
    }

    public void SetRoomReference(GameObject target)
    {
        target.GetComponent<ApplyRoomAttribute>().room = this.gameObject;
        target.GetComponent<ProtoTileBehavior>().sourceBranch = gameObject;

        if (isMainHall)
        {
            target.GetComponent<ProtoTileBehavior>().isMainHall = true;
        }

    }

    public void PruneEdgeList()
    {
        for (int i = 0; i < edgeList.Count-1; i++)
        {
            bool isNearEmptySpace = false;
            CheckAdjacent(edgeList[i].transform.position, transform.forward * step, isNearEmptySpace);
            CheckAdjacent(edgeList[i].transform.position, transform.right * step, isNearEmptySpace);
            CheckAdjacent(edgeList[i].transform.position, transform.forward * -step, isNearEmptySpace);
            CheckAdjacent(edgeList[i].transform.position, transform.right * -step, isNearEmptySpace);

            if (!isNearEmptySpace)
            {
                edgeList.Remove(edgeList[i]);
            }
        }
    }


    public void CheckAdjacent(Vector3 p, Vector3 direction, bool isNearEmptySpace)
    {
        if (Physics.Raycast(transform.position, direction, out RaycastHit info, 1, 1 << 8) == false)
        { 
            isNearEmptySpace = true;
        }
        
    }

}

