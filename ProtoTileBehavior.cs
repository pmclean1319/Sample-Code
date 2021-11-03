using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtoTileBehavior : MonoBehaviour
{
    public GameObject sourceBranch;
    public int mallLayer;
    public bool isMainHall;
    public Vector3 dropPoint;
    public int tileType;
    public GameObject tileSetObj;
    public TileSet tileSet;
    public bool floorSpawned = false;
    public bool ceilingSpawned = false;
    public bool stairSpawned = false;
    public bool hasPillar = false;
    public GameObject errorObj;
    public GameObject[] decoAlwaysSpawn;
    public GameObject debugObj;

    //TileDescriptors
    public bool isTraversable,
                isVoidType,
                isFloorType,
                isStairType,
                isCeilingDetailType,
                isCeilingType,
                isSpecWallType,
                isEntranceType,
                isEgress,
                isStairDescend,
                isNoRailType,
                isCeilingEdgeType,
                isNoWallType,
                isCeilingEdgeTile;

    //Tile Location
    public int d, w, h;

    // Start is called before the first frame update
    void Start()
    {
        tileSet = tileSetObj.GetComponent<TileSet>();
        dropPoint = new Vector3(transform.position.x, transform.position.y - .5f, transform.position.z);
        transform.GetChild(0).gameObject.SetActive(false);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator SpawnTilePieces()
    {
            //isSpaceAvailable();
            SpawnPhase1();
            yield return new WaitForEndOfFrame();
            SpawnPhase2();
            //Debug
            if (isTraversable)
        {
            Instantiate(debugObj, transform.position, Quaternion.identity);
        }
        
    }

    public void SpawnPhase1()
    {
        Vector3 p = transform.rotation.eulerAngles;

        //Floor
        if (isVoidType)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1, 1 << 8) == false)
            {
                Instantiate(tileSet.floor_1, dropPoint, transform.rotation, this.gameObject.transform);
                floorSpawned = true;
            }
            else
            {
                if (hit.transform.gameObject.GetComponent<ProtoTileBehavior>().sourceBranch != sourceBranch && hit.transform.gameObject.GetComponent<ProtoTileBehavior>().isCeilingType)
                {
                    Instantiate(tileSet.floor_1, dropPoint, transform.rotation, this.gameObject.transform);
                    floorSpawned = true;
                }
            }
        }
        if (isFloorType && floorSpawned == false)
        {
            if (!IsThereStairSpace())
            {
                Instantiate(tileSet.floor_1, dropPoint, transform.rotation, this.gameObject.transform);
                floorSpawned = true;
            }
            
        }

        //Floor over Ceiling
        if (Physics.Raycast(transform.position, transform.up * -1, out RaycastHit targetl, 2, 1 << 8) == true)
        {
            if (targetl.transform.gameObject.GetComponent<ProtoTileBehavior>().mallLayer != mallLayer)
            {
                Instantiate(tileSet.floor_1, dropPoint, transform.rotation, this.gameObject.transform);
                floorSpawned = true;
            }
        }

        if (isStairType)
        {
            Instantiate(tileSet.stairs_1, dropPoint, transform.rotation, this.gameObject.transform);
            floorSpawned = true;
            return;
        }

        if (isStairDescend)
        {
            Instantiate(tileSet.stairs_descend_1, dropPoint, transform.rotation, this.gameObject.transform);
            floorSpawned = true;
            return;
        }

        //Walls
        if (!isNoWallType)
        {
            SpawnWall(p, transform.forward * .25f, 0);
            SpawnWall(p, transform.right * .25f, 90);
            SpawnWall(p, transform.forward * -.25f, 180);
            SpawnWall(p, transform.right * -.25f, 270);
        }
        



        if (isSpecWallType)
        {
            if (Physics.Raycast(transform.position, transform.forward * -.25f, 1, 1 << 8) == true)
            {
                Instantiate(tileSet.specWall_1, dropPoint, Quaternion.Euler(p.x, p.y + 270, p.z), this.gameObject.transform);
            }
        }

        if (isEntranceType)
        {
            if (Physics.Raycast(transform.position, transform.forward * -.25f, 1, 1 << 8) == true)
            {
                Instantiate(tileSet.entrance_1, dropPoint, Quaternion.Euler(p.x, p.y + 270, p.z), this.gameObject.transform);
            }
        }

        //Ceiling
        if (isCeilingType == true)
        {
            Instantiate(tileSet.ceiling_1, dropPoint, transform.rotation, this.gameObject.transform);
        }
        else
        {
            if (Physics.Raycast(transform.position, Vector3.up, out RaycastHit hit, 1, 1 << 8) == false)
            {
                if (isCeilingDetailType)
                {
                    Instantiate(tileSet.ceilingDecoType_1, dropPoint, transform.rotation, this.gameObject.transform);
                }
                if (isCeilingEdgeTile)
                {
                    //don't spawn ceiling   
                }
                else
                {
                    Instantiate(tileSet.ceiling_1, dropPoint, transform.rotation, this.gameObject.transform);
                }
                ceilingSpawned = true;
            }
            else
            {
                if (hit.collider.gameObject.GetComponent<ProtoTileBehavior>().sourceBranch != sourceBranch && !isMainHall)
                {
                    Instantiate(tileSet.ceiling_1, dropPoint, transform.rotation, this.gameObject.transform);
                    ceilingSpawned = true;
                    //hit.collider.gameObject.GetComponent<ProtoTileBehavior>().SetType("floor");
                }
            }
        }

        




        //Connector
        if (isEgress)
        {
            Instantiate(tileSet.connector, dropPoint + transform.forward, transform.rotation);
        }


        //Always Spawn
        foreach (GameObject deco in decoAlwaysSpawn)
        {
            Instantiate(deco, dropPoint, Quaternion.Euler(p.x, p.y, p.z), this.gameObject.transform);
        }
    }

    public void SpawnPhase2()
    {
        Vector3 p = transform.rotation.eulerAngles;

        //Railings
        if (floorSpawned == true && isStairType == false && !isNoRailType)
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit targetf, 2, 1 << 8) == true)
            {
                if (targetf.transform.GetComponent<ProtoTileBehavior>() != null && targetf.transform.GetComponent<ProtoTileBehavior>().floorSpawned == false)
                {
                    Instantiate(tileSet.railing_1, dropPoint, Quaternion.Euler(p.x, p.y, p.z), this.gameObject.transform);
                }

            }
            if (Physics.Raycast(transform.position, transform.right, out RaycastHit targetr, 2, 1 << 8) == true)
            {
                if (targetr.transform.GetComponent<ProtoTileBehavior>() != null && targetr.transform.GetComponent<ProtoTileBehavior>().floorSpawned == false)
                {
                    Instantiate(tileSet.railing_1, dropPoint, Quaternion.Euler(p.x, p.y + 90, p.z), this.gameObject.transform);
                }

            }
            if (Physics.Raycast(transform.position, transform.forward * -1, out RaycastHit targetb, 2, 1 << 8) == true)
            {
                if (targetb.transform.GetComponent<ProtoTileBehavior>() != null && targetb.transform.GetComponent<ProtoTileBehavior>().floorSpawned == false)
                {
                    Instantiate(tileSet.railing_1, dropPoint, Quaternion.Euler(p.x, p.y + 180, p.z), this.gameObject.transform);
                }

            }
            if (Physics.Raycast(transform.position, transform.right * -1, out RaycastHit targetl, 2, 1 << 8) == true)
            {
                if (targetl.transform.GetComponent<ProtoTileBehavior>() != null && targetl.transform.GetComponent<ProtoTileBehavior>().floorSpawned == false)
                {
                    Instantiate(tileSet.railing_1, dropPoint, Quaternion.Euler(p.x, p.y + 270, p.z), this.gameObject.transform);
                }

            }
        }

        //Pillars
        if (hasPillar)
        {
            GenPillar();
        }

        


    }

    public void GenPillar()
    {
        Instantiate(tileSet.pillar_1, dropPoint, transform.rotation, this.gameObject.transform);
        if (floorSpawned == false)
        {
            if (Physics.Raycast(transform.position, transform.up * -1, out RaycastHit targetd, 2, 1 << 8) == true)
            {
                targetd.transform.gameObject.GetComponent<ProtoTileBehavior>().GenPillar();
            }
        }
        
    }

    public void CallSpawnTile()
    {
        StartCoroutine(SpawnTilePieces());
    }

    public void isSpaceAvailable()
    {
        bool result = true;
        if(Physics.OverlapBox(transform.position, new Vector3(.1f,.1f,.1f), Quaternion.identity, 1 << 8).Length > 2)
        {
            //print("BoxCast Hit!");
            result = false;
            //Instantiate(errorObj, transform.position, Quaternion.identity);
            gameObject.transform.parent.gameObject.SetActive(false);
            //Destroy(transform.parent.gameObject);
        }
        
    }

    public void SetType(string type)
    {
        switch (type)
        {
            case "traversable":
                isTraversable = true;
                break;
            case "void":
                isVoidType = true;
                break;
            case "floor":
                isFloorType = true;
                break;
            case "stair":
                isStairType = true;
                break;
            case "descend":
                isStairDescend = true;
                break;
            case "noRail":
                isNoRailType = true;
                break;
            case "ceilingDetail":
                isCeilingDetailType = true;
                break;
            case "ceiling":
                isCeilingType = true;
                break;
            case "specWall":
                isSpecWallType = true;
                break;
            case "entrance":
                isEntranceType = true;
                break;
            case "pillar":
                hasPillar = true;
                break;
            case "egress":
                isEgress = true;
                break;
            case "isNoWall":
                isNoWallType = true;
                break;
            default:
                break;
        }
    }

    public void AssignLocation(int depth, int width, int height)
    {
        d = depth;
        w = width;
        h = height;
    }

    public bool IsThereStairSpace()
    {
        bool result = false;
        Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(.1f, .1f, .1f));
        foreach (Collider col in colliders)
        {
            if (col.gameObject.tag == "StairSpace")
            {
                result = true;
            }
        }
        return result;

        
    }

    public void SpawnWall(Vector3 p,Vector3 direction, int rot)
    {
        if (Physics.Raycast(transform.position, direction, out RaycastHit info, 1, 1 << 8) == false)
        {
            Instantiate(tileSet.wall_f, dropPoint, Quaternion.Euler(p.x, p.y + rot, p.z), this.gameObject.transform);
            if (isCeilingEdgeType)
            {
                isCeilingEdgeTile = true;
            }
        }
        else
        {
            //Walls between branches
            if (info.transform.GetComponent<ProtoTileBehavior>().sourceBranch != sourceBranch && !isTraversable && !isMainHall)
            {
                if (info.transform.GetComponent<ProtoTileBehavior>().isMainHall)
                {
                    Instantiate(tileSet.boundWall, dropPoint, Quaternion.Euler(p.x, p.y + rot, p.z), this.gameObject.transform);
                }
                else
                {
                    Instantiate(tileSet.wall_f, dropPoint, Quaternion.Euler(p.x, p.y + rot, p.z), this.gameObject.transform);
                }
                
            }
            if (info.transform.GetComponent<ProtoTileBehavior>().sourceBranch != sourceBranch && isTraversable && !isMainHall)
            {
                if (info.transform.GetComponent<ProtoTileBehavior>().isMainHall)
                {
                    Instantiate(tileSet.entrance_1, dropPoint, Quaternion.Euler(p.x, p.y + rot, p.z), this.gameObject.transform);
                }
                else
                {
                    Instantiate(tileSet.entrance_1, dropPoint, Quaternion.Euler(p.x, p.y + rot, p.z), this.gameObject.transform);
                }

            }
        }
    }
}
