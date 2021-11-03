using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.SceneManagement;

public class PlayerBehavior : MonoBehaviour {
    public bool isDebug = false;


    public float health = 1;
    public int healthMax = 10;
    public int regenCooldown = 0;
    public int regenCooldownMax = 500;
    public float regenRate = 2.00f;
    public bool isRegenReady = true;
    public bool isRegenCooledDown = true;

    public float mouseRotationX;
    public float mouseRotationY;
    Quaternion originalRotation;

    public float snapSpeed = 10;

    public float speedIncrease;
    public float minSpeed;
    public float speed;
    public float maxSpeed;
    public float lookUpMax = -30, lookDnMax = 30;
    public float dodgeSpeed = 15;
    public int invertLook = 1;

    bool isMovingForward =true;

    public float aimspeed;

    public float rotXMin;
    public float rotXMax;
    public float rotXSpeed;

    public float rotY, rotX, mouseSensitivity, clampAngle;

    public float jump;
    public float wallJump;
    public float jumpDirection;

    public float liftForce;

    public float dashSpeed;

    public float wallRunTime;
    public bool isAbleToWallRun = true;
    public bool isWallRunning = false;

    public GameObject mainCamera;
    public GameObject gunCamera;
    public GameObject feet;
    public GameObject toes;
    public GameObject hands;
    public GameObject left, right;
    public GameObject head;
    public GameObject shotpoint;
    public GameObject targetLock;
    public AudioSource speaker;
    public AudioSource healthSpeaker;
    public AudioSource lowHealthAlarm;
    public GameObject disintegrateEffect;

    public AudioClip deathSound;
    public AudioClip takeDamageSound;
    public AudioClip healSound;
    public AudioClip deathFrazzle;
    public AudioClip aimSound;

    public float facingRotation;

    public bool isTurnable=true;

    public bool isRunning = true;

    public bool isFiring = false;

    public bool movedWhileFiring=false;

    public bool isTraining = false;

    public GameObject bodyModel;
    public GameObject gun;

    public GameObject targetZone;
    public GameObject currentTarget;
    public GameObject currentPaintedTarget;
    public GameObject UI;

    public float lookSpeed;
    public float aimSpeed;
    public GameObject lookStick;
    public GameObject lookField;

    public GameObject globalVariables;
    public GameObject account;
    public GameObject healthMonitor;

    public int credits = 0;

    public bool isDying = false;

    public bool wallClimbCoolDown = false;

    private bool isjumpSpinning = false;

    public bool isPunching = false;

    public DumpsterScript dumpster;

	// Use this for initialization
	void Start () {
        dumpster = GameObject.Find("Dumpster").GetComponent<DumpsterScript>();
        lookStick = GameObject.Find("lookstick");
        globalVariables = GameObject.Find("GlobalVariables");
        UI = GameObject.Find("UIPlayer");
        AdaptToAccount();
        HandleInversionControls();
        //targetLock = GameObject.Find("TargetLockBillboard");
        speaker = GetComponent<AudioSource>();
        //StartCoroutine(Regen());
        healthMonitor = GameObject.Find("PlayerHealthBar");
        account.GetComponent<AccountInfo>().currentLevel = 0;
	}

	// Update is called once per frame
	void Update () {


        Turn();
        if (isDebug == false)
        {
            //Look();
            FreeLook();
        }
            
        else
        {
            mainCamera.GetComponent<Animator>().enabled = false;
            MouseLook();
        }
            
        Run();
        
        Jump();
        Grab();
//        Lean();
        SetDirection();
        if (isPunching == false)
        {
            FireWeapon();
        }
        
        WallRun();
        Lift();
        Dash();
        RegenCoolDown();
        Die();
        //        RotationReset();
        Unstucker();
	}

    void Look()
    {
        float tempLookSpeed = lookSpeed;
        int layerMask = (1 << 10);
        float lockOnGum = 0;

        if ((account.GetComponent<AccountInfo>().lockOnEnabled) && (lookStick.GetComponent<Joystick>().isDragging))
        {


            RaycastHit hit;
            if (Physics.SphereCast(mainCamera.transform.position, 10, mainCamera.transform.forward, out hit, 300f, layerMask))
            {

                if (hit.collider.tag == "Enemy")
                {
                    tempLookSpeed = aimSpeed;

                    GameObject target = hit.collider.transform.gameObject;

                    if (currentTarget == null)
                    {
                        GameObject paintTarget = Instantiate(targetLock, target.transform.position, Quaternion.identity) as GameObject;
                        paintTarget.transform.parent = target.transform;
                        currentTarget = target;
                        currentPaintedTarget = paintTarget;
                    }


                }

            }

            if (currentTarget != null)
            {
                Vector3 targetPoint = new Vector3(currentTarget.transform.position.x, transform.position.y, currentTarget.transform.position.z) - transform.position;
                Vector3 targetRotation = currentTarget.transform.position - transform.position;
                targetRotation.x = 0.0f;
                targetRotation.z = 0.0f;
                transform.LookAt(currentTarget.transform.position);
                Vector3 eulerAngles = transform.rotation.eulerAngles;

                //Set up lock on rubberbanding
                lockOnGum = eulerAngles.x + 5;
               

                eulerAngles.x = 0;
                eulerAngles.z = 0;





                // Set the altered rotation back
                transform.rotation = Quaternion.Euler(eulerAngles);


                //Trigger punch attack
                if (Vector3.Distance(transform.position, currentTarget.transform.position) < 10)
                {
                    if (isPunching == false && currentTarget.GetComponent<EnemyScript>().isPunchable == true)
                    {
                        Punch(currentTarget);
                    }

                }


                if (currentTarget.GetComponent<EnemyScript>() != null)
                {
                    if (currentTarget.GetComponent<EnemyScript>().isAlive == false)
                    {
                        currentTarget = null;
                    }
                }
                
            }
        }

        if (Mathf.Abs(CrossPlatformInputManager.GetAxis("LookVertical")) > .9f || Mathf.Abs(CrossPlatformInputManager.GetAxis("LookHorizontal")) > .9f)
        {

                mainCamera.GetComponent<Animator>().enabled = false;
                rotY = ((CrossPlatformInputManager.GetAxis("LookVertical") * tempLookSpeed) * invertLook);

                //x coordinate lock
                float adjustedRot = mainCamera.transform.eulerAngles.x;
                if (adjustedRot > 180)
                    adjustedRot = (adjustedRot - 360);

                if (rotY > 0 && adjustedRot < lookDnMax)
                {
                    mainCamera.transform.Rotate(rotY/4, 0, 0);
                    //speaker.PlayOneShot(aimSound);
                }
                    

                if (rotY < 0 && (adjustedRot > lookUpMax))
                {
                    mainCamera.transform.Rotate(rotY/4, 0, 0);
                    //speaker.PlayOneShot(aimSound);
                }
                    

                if (mainCamera.transform.eulerAngles.x > lockOnGum && lockOnGum != 0)
                {
                    float origY = mainCamera.transform.eulerAngles.y;
                    float origZ = mainCamera.transform.eulerAngles.z;
                    Vector3 lockonRotation = new Vector3(lockOnGum, origY, origZ);
                    //mainCamera.transform.eulerAngles = lockonRotation;
                }


            
        }
        else
        {
            float origY = mainCamera.transform.eulerAngles.y;
            float origZ = mainCamera.transform.eulerAngles.z;
            mainCamera.transform.eulerAngles = new Vector3(0, origY, origZ);
        }
            
                    if (currentTarget == null)
        rotX = (CrossPlatformInputManager.GetAxis("LookHorizontal") * tempLookSpeed);
        transform.Rotate(0, rotX, 0);

        if (lookStick.GetComponent<Joystick>().isDragging == false)
        {
            if (mainCamera.transform.eulerAngles.x > 180)
            {
                mainCamera.transform.Rotate(Vector3.left * snapSpeed * -1);
            }
            if (mainCamera.transform.eulerAngles.x < 180)
            {
                mainCamera.transform.Rotate(Vector3.left * snapSpeed);
            }
            if (mainCamera.transform.eulerAngles.x < 370 && mainCamera.transform.eulerAngles.x > 350)
            {
                mainCamera.GetComponent<Animator>().enabled = true;
            }
            
            rotY = 0;
            rotX = 0;


            shotpoint.transform.rotation = new Quaternion(0, 0, 0, 0);

            if (currentPaintedTarget != null)
            {
                Destroy(currentPaintedTarget);
            }
                

            currentTarget = null;
            
            
        }
        
    }

    void MouseLook()
    {
        print("Rotations");
        mouseRotationX += Input.GetAxis("Mouse X") * .2f;
        mouseRotationY += Input.GetAxis("Mouse Y") * .2f;

        Quaternion xQuaternion = Quaternion.AngleAxis(mouseRotationX, Vector3.up);
        Quaternion yQuaternion = Quaternion.AngleAxis(mouseRotationY, -Vector3.right);
        mainCamera.transform.Rotate(mouseRotationY, 0, 0);

    }

    void Run()
    {
        if (isRunning)
        {
            
            if (GetComponent<Rigidbody>().velocity.z < maxSpeed)
            {
                if (isFiring && ((Input.GetAxis("Vertical") != 0 || CrossPlatformInputManager.GetAxis("Vertical") != 0)
                    || (Input.GetAxis("Horizontal") != 0 || CrossPlatformInputManager.GetAxis("Horizontal") != 0)))
                {
                    transform.position += ((transform.forward * ((Input.GetAxis("Vertical") + (CrossPlatformInputManager.GetAxis("Vertical")))) * Time.deltaTime * maxSpeed));
                    speed = maxSpeed;
                    movedWhileFiring = true;
                }


                else if (((Input.GetAxis("Vertical") < -.5 || (CrossPlatformInputManager.GetAxis("Vertical") < -.5)) && isWallRunning == false))
                {
                    isMovingForward = false;
                    bodyModel.GetComponent<Animator>().SetBool("isSteppingBack", true);
                    transform.position += ((transform.forward * ((Input.GetAxis("Vertical") + (CrossPlatformInputManager.GetAxis("Vertical")))) * Time.deltaTime * maxSpeed));
                }

                else
                    if ((movedWhileFiring == false))
                {
                    bodyModel.GetComponent<Animator>().SetBool("isSteppingBack", false);
                    transform.position += ((transform.forward * ((Input.GetAxis("Vertical") + (CrossPlatformInputManager.GetAxis("Vertical")))) * Time.deltaTime * maxSpeed));
                    transform.position += ((transform.right * Time.deltaTime * jumpDirection));
                }
                    
            }
        }
    }

    void Turn()
    {
        if (isTurnable)
        {
            if ((Input.GetAxis("Horizontal") < 0) || (CrossPlatformInputManager.GetAxis("Horizontal")<0))
            {
                if (bodyModel.transform.eulerAngles.z < 10 || bodyModel.transform.eulerAngles.z > 349)
                {
                    //Do Nothing
                }
                
                if (isFiring || Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    transform.position += (transform.right * CrossPlatformInputManager.GetAxis("Horizontal")) * Time.deltaTime * (maxSpeed +3);
                    speed = maxSpeed;
                }
                else
                    transform.Rotate(0, (rotXSpeed * ((Input.GetAxis("Horizontal") + (CrossPlatformInputManager.GetAxis("Horizontal"))))) * Time.deltaTime , 0);
            }
            if ((Input.GetAxis("Horizontal") > 0) || (CrossPlatformInputManager.GetAxis("Horizontal") > 0))
            {
                if (bodyModel.transform.eulerAngles.z > 350 || bodyModel.transform.eulerAngles.z < 11)
                {
                    //Do Nothing
                }
                    

                if (isFiring || Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    transform.position += (transform.right * CrossPlatformInputManager.GetAxis("Horizontal")) * Time.deltaTime * (maxSpeed +3);
                    speed = maxSpeed;
                }
                else
                    transform.Rotate(0, (rotXSpeed * ((Input.GetAxis("Horizontal") + (CrossPlatformInputManager.GetAxis("Horizontal"))))) * Time.deltaTime ,0);
            }
            if ((Input.GetAxis("Horizontal")==0) && (CrossPlatformInputManager.GetAxis("Horizontal") == 0))
            {
                bodyModel.transform.rotation = new Quaternion(0, 0, 0, 0);
                
            }

        }
    }

    void Jump()
    {
        if ((Input.GetButtonDown("Jump") || CrossPlatformInputManager.GetButtonDown("Jump")))
        {


            if ((feet.GetComponent<FeetChecker>().isOnFloor) && (hands.GetComponent<HandChecker>().isContacting == false) && (head.GetComponent<HandChecker>().isContacting == false))
                transform.GetComponent<Rigidbody>().AddRelativeForce(0, jump, 0);

            if (feet.GetComponent<FeetChecker>().isOnFloor == false && isWallRunning == true)
            {
                                    transform.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                                   
                if (left.GetComponent<HandChecker>().isContacting)
                {
                                        transform.GetComponent<Rigidbody>().AddForce(0, jump, 0);
                                        jumpDirection = 5;

                }
                if (right.GetComponent<HandChecker>().isContacting)
                {
                    transform.GetComponent<Rigidbody>().AddForce(0, jump, 0);
                    jumpDirection = -5;

                }
                
            }

            mainCamera.GetComponent<Animator>().SetBool("isClimbing", false);
            bodyModel.GetComponent<Animator>().SetBool("isClimbing", false);

            mainCamera.GetComponent<Animator>().SetBool("isRunningOnFloor", false);
            mainCamera.GetComponent<Animator>().SetBool("isJumping", true);

            bodyModel.GetComponent<Animator>().SetBool("isRunningOnFloor", false);
            bodyModel.GetComponent<Animator>().SetBool("isJumping", true);

            feet.GetComponent<FeetChecker>().isOnFloor = false;
            isWallRunning = false;
        }
        if (feet.GetComponent<FeetChecker>().isOnFloor)
        {
            jumpDirection = 0;
        }
    }

    void Lean()
    {
        
        RaycastHit hit;
        Ray ray = new Ray(transform.position, Vector3.down);
        
        if (Physics.Raycast(ray,out hit, 2))
        {
            Quaternion rotCur = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            Vector3 posCur = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, posCur, Time.deltaTime * 5);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotCur, Time.deltaTime * 5);
        }
        
    }

    void Grab()
    {
        if (hands.GetComponent<HandChecker>().isContacting)
        {
            isRunning = false;
        }
        if (hands.GetComponent<HandChecker>().isContacting == false)
        {
            isRunning = true;
        }
    }

    void Lift()
    {
        if (head.GetComponent<HandChecker>().isContacting == false && (Input.GetAxis("Vertical") > 0) || (CrossPlatformInputManager.GetAxis("Vertical") > 0))
        {

            if ((hands.GetComponent<HandChecker>().isContacting == true))
            {
                transform.localPosition += transform.up * Time.deltaTime * liftForce;
                mainCamera.GetComponent<Animator>().SetBool("isClimbing", true);
                mainCamera.GetComponent<Animator>().SetBool("isWallRunningLeft", false);
                mainCamera.GetComponent<Animator>().SetBool("isWallRunningRight", false);
                bodyModel.GetComponent<Animator>().SetBool("isClimbing", true);
                isWallRunning = false;
                GetComponent<Rigidbody>().isKinematic = true;
                if (head.GetComponent<HandChecker>().isContacting == true)
                {
                    liftForce = 0;
                    print("head hit ceiling");
                    StartCoroutine(WallClimbReset());
                }

            }
            else
            {
                mainCamera.GetComponent<Animator>().SetBool("isClimbing", false);
                bodyModel.GetComponent<Animator>().SetBool("isClimbing", false);
                GetComponent<Rigidbody>().isKinematic = false;
                liftForce = 10;
                if (head.GetComponent<HandChecker>().isContacting)
                {
                    liftForce = 0;
                }
  

            }
            
        }
        else
        {
            StartCoroutine(WallClimbReset());
        }
        
    }


    void FireWeapon()
    {
        if ((lookField.GetComponent<FreeLookFieldScript>().Pressed || Input.GetButton("Fire1")) && isDying == false)
        {
            isFiring = true;
            
            bodyModel.GetComponent<Animator>().SetBool("isShooting", true);
            if (gun.GetComponent<GunScript>().rate > 1 && gun.GetComponent<GunScript>().rate < 4)
                bodyModel.GetComponent<Animator>().SetBool("averageShoot", true);

            if (gun.GetComponent<GunScript>().rate <= 1)
                bodyModel.GetComponent<Animator>().SetBool("slowShoot", true);

            if (gun.GetComponent<GunScript>().rate > 4)
                bodyModel.GetComponent<Animator>().SetBool("fastShoot", true);

            if (isPunching == false)
            {
                bodyModel.GetComponent<Animator>().speed = gun.GetComponent<GunScript>().rate;
            }
            

            mainCamera.GetComponent<Animator>().SetBool("isShooting", true);
        }
        else
        {
            isFiring = false;
            movedWhileFiring = false;
            bodyModel.GetComponent<Animator>().speed = 1;
            bodyModel.GetComponent<Animator>().SetBool("isShooting", false);
            bodyModel.GetComponent<Animator>().SetBool("averageShoot", false);
            bodyModel.GetComponent<Animator>().SetBool("slowShoot", false);
            bodyModel.GetComponent<Animator>().SetBool("fastShoot", false);
            mainCamera.GetComponent<Animator>().SetBool("isShooting", false);
        }
    }

    void Attack()
    {
        if (isPunching == false && isDying == false)
        {


            RaycastHit hit;
            if ((CrossPlatformInputManager.GetButton("Fire1") && (isWallRunning == false)))
            {
                if (targetZone.GetComponent<TargetZoneScript>().currentTarget != null)
                {
                    Debug.DrawRay(mainCamera.transform.position, (targetZone.GetComponent<TargetZoneScript>().currentTarget.transform.position - transform.position), Color.green, 4);
                    if (Physics.Raycast(mainCamera.transform.position, (targetZone.GetComponent<TargetZoneScript>().currentTarget.transform.position - transform.position), out hit, 1000.0f))
                    {
                        if (hit.collider.tag == "Enemy")
                        {
                            if (hit.collider.gameObject.transform.GetComponent<EnemyScript>().isAlive)
                            {


                                isFiring = true;
                                bodyModel.GetComponent<Animator>().SetBool("isShooting", true);
                                bodyModel.GetComponent<Animator>().SetBool("gunFired", true);
                                mainCamera.GetComponent<Animator>().SetBool("isShooting", true);
                                //            StartCoroutine(FiringGun());
                                if (isPunching == false)
                                {
                                    bodyModel.GetComponent<Animator>().speed = gun.GetComponent<GunScript>().rate;
                                }

                                //            transform.LookAt(targetZone.GetComponent<TargetZoneScript>().currentTarget.transform);
                                //            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetZone.GetComponent<TargetZoneScript>().currentTarget.transform.position), speed * Time.deltaTime);
                                Vector3 relativePos = targetZone.GetComponent<TargetZoneScript>().currentTarget.transform.position - transform.position;
                                Quaternion rotation = Quaternion.LookRotation(relativePos);
                                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.time * aimspeed);
                            }

                        }
                        else
                        {
                            print(hit.collider);
                            bodyModel.GetComponent<Animator>().SetBool("isShooting", false);
                            bodyModel.GetComponent<Animator>().SetBool("gunFired", false);
                            mainCamera.GetComponent<Animator>().SetBool("isShooting", false);
                            isFiring = false;
                            bodyModel.GetComponent<Animator>().speed = 1;
                            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                        }

                    }
                }

            }

            if (CrossPlatformInputManager.GetButtonUp("Fire1"))

            {
                bodyModel.GetComponent<Animator>().SetBool("isShooting", false);
                bodyModel.GetComponent<Animator>().SetBool("gunFired", false);
                mainCamera.GetComponent<Animator>().SetBool("isShooting", false);
                isFiring = false;
                bodyModel.GetComponent<Animator>().speed = 1;

            }

        }
    }

    void WallRun()
    {
        if ((left.GetComponent<HandChecker>().isContacting) || (right.GetComponent<HandChecker>().isContacting))
        {
                
            if ( (feet.GetComponent<FeetChecker>().isOnFloor == false))

            {

                isWallRunning = true;
                isAbleToWallRun = false;
                if (right.GetComponent<HandChecker>().isContacting == true)
                {
                    mainCamera.GetComponent<Animator>().SetBool("isWallRunningLeft", true);
                    bodyModel.GetComponent<Animator>().SetBool("isWallRunningLeft", true);
                    
                    
                }
                if (right.GetComponent<HandChecker>().isContacting == false)
                {
                    mainCamera.GetComponent<Animator>().SetBool("isWallRunningRight", true);
                    bodyModel.GetComponent<Animator>().SetBool("isWallRunningRight", true);
                    

                }

            }
        }
        if (((feet.GetComponent<FeetChecker>().isOnFloor) || (isWallRunning==false) || ((left.GetComponent<HandChecker>().isContacting == false) && (right.GetComponent<HandChecker>().isContacting==false))))
        {
            if (feet.GetComponent<FeetChecker>().isOnFloor)
            {
                isAbleToWallRun = true;
            }
            isWallRunning = false;
            mainCamera.GetComponent<Animator>().SetBool("isWallRunningLeft", false);
            mainCamera.GetComponent<Animator>().SetBool("isWallRunningRight", false);
            bodyModel.GetComponent<Animator>().SetBool("isWallRunningLeft", false);
            bodyModel.GetComponent<Animator>().SetBool("isWallRunningRight", false);
            isTurnable = true;
        }
        if (isWallRunning == true)
        {
            GetComponent<Rigidbody>().AddForce(new Vector3(0, wallRunTime, 0));
        }

    }
    private void Dash()
    {
        speed = minSpeed + dashSpeed;

        if ((Input.GetAxis("Vertical") > .1 || (CrossPlatformInputManager.GetAxis("Vertical") > .1)))
        {
            if (isFiring == false)
            {
                if (mainCamera.GetComponent<Camera>().fieldOfView < 80)
                {
                    mainCamera.GetComponent<Camera>().fieldOfView = mainCamera.GetComponent<Camera>().fieldOfView + .5f;
                }
                if (speed < maxSpeed)
                {
                    dashSpeed = dashSpeed + speedIncrease;
                }
                bodyModel.GetComponent<Animator>().SetBool("isDashing", true);

            }
            else
            {
                speed = maxSpeed;
            }
            
        }
        else
        {
            if (mainCamera.GetComponent<Camera>().fieldOfView > 69.6)
            {
                mainCamera.GetComponent<Camera>().fieldOfView = mainCamera.GetComponent<Camera>().fieldOfView - .5f;
            }
            if (dashSpeed > 0)
            {
                dashSpeed = dashSpeed - speedIncrease;
            }
            bodyModel.GetComponent<Animator>().SetBool("isDashing", false);

        }
                
    }

    public IEnumerator DashAttack()
    {
        print("DashAttackBlip");
        float tSpeed = speed;
        speed += 5;
        yield return new WaitForSeconds(.7f);
        speed = tSpeed;
        isPunching = false;
        bodyModel.GetComponent<Animator>().SetBool("isPunching", false);
    }

    private void SetDirection()
    {
        float facing = transform.eulerAngles.y;

        if (facing > 315f || facing < 45f)
        {
            facingRotation = 1f;
        }
        if (facing>45f && facing < 135f)
        {
            facingRotation = 90f;
        }
        if (facing>135f && facing < 225f)
        {
            facingRotation = 180f;
        }
        if (facing > 225f && facing < 315f)
        {
            facingRotation = 270f;
        }
    }

    

    IEnumerator WallRunTime()
    {
        print("Wallrunning");
        transform.eulerAngles = new Vector3(0, facingRotation, 0);
        isTurnable = false;
        isWallRunning = true;
        if (right.GetComponent<HandChecker>().isContacting == true)
        {
            mainCamera.GetComponent<Animator>().SetBool("isWallRunningLeft", true);
        }
        if (right.GetComponent<HandChecker>().isContacting == false)
        {
            mainCamera.GetComponent<Animator>().SetBool("isWallRunningRight", true);
        }
        
        yield return new WaitForSeconds(wallRunTime);


        mainCamera.GetComponent<Animator>().SetBool("isWallRunningLeft", false);
        mainCamera.GetComponent<Animator>().SetBool("isWallRunningRight", false);
        isTurnable = true;
        yield return new WaitForSeconds(1);

        isWallRunning = false;
    }

    IEnumerator FiringGun()
    {
        while (isFiring == true && isDying == false)
        {
            bodyModel.GetComponent<Animator>().SetBool("isShooting", true);
            yield return new WaitForSeconds(.01f);
            
            gun.GetComponent<GunScript>().startFiring();
            
            print("test");
            
        }
    }

    public void Punch(GameObject enemy)
    {
        isPunching = true;
        isFiring = false;
        movedWhileFiring = false;
        bodyModel.GetComponent<Animator>().speed = 1;
        bodyModel.GetComponent<Animator>().SetBool("isShooting", false);
        bodyModel.GetComponent<Animator>().SetBool("averageShoot", false);
        bodyModel.GetComponent<Animator>().SetBool("slowShoot", false);
        bodyModel.GetComponent<Animator>().SetBool("fastShoot", false);
        mainCamera.GetComponent<Animator>().SetBool("isShooting", false);
        bodyModel.GetComponent<Animator>().SetBool("isPunching", true);
        StartCoroutine(DashAttack());
        enemy.GetComponent<EnemyScript>().TakeDamage(500);
    }

    public void OnCollisionEnter(Collision collision)
    {
        transform.GetComponent<Rigidbody>().freezeRotation=true;
    }

    public void TakeDamage(float dmg)
    {
        StartCoroutine(BlurCamera());
        if (health > 0)
        {
            health = (health - dmg);
        }
        
        isRegenCooledDown = false;
        regenCooldown = 0;
        speaker.PlayOneShot(takeDamageSound);
        if (health < 3)
        {
            lowHealthAlarm.volume = .2f;
        }

    }

    public IEnumerator Regen()
    {
        bool forever = true;
        while (forever)
        {
            if ((isRegenReady == true))
            {
                    isRegenReady = false;
                    yield return new WaitForSeconds(regenRate);
                if ((health < healthMax) && (isRegenCooledDown))
                    {
                        health++;
                    }   
                    isRegenReady = true;
            }
        }
    }

    public void RegenCoolDown()
    {
        if (isRegenCooledDown == false)
        {
            regenCooldown++;
        }
        if (regenCooldown >= regenCooldownMax)
        {
            isRegenCooledDown = true;
        }
    }

    public void Die()
    {
        if (isTraining == false)
        {
            if (transform.position.y < -50 || health < 1)
            {
                if (isDying == false)
                {
                    isDying = true;

                    dumpster.CompactGarbage();


                    
                    bodyModel.GetComponent<Animator>().SetBool("isDying", true);
                    mainCamera.GetComponent<Animator>().SetBool("isBlasted", true);
                    account.GetComponent<AccountInfo>().gameHasStarted = true;
                    
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    StartCoroutine(DieCinematic());
                }

            }
        }
        
    }

    public void GainHealth()
    {
        if (health < healthMax)
        {
            health++;
            healthSpeaker.pitch = health / healthMax;
            healthSpeaker.PlayOneShot(healSound);
            
            healthMonitor.transform.GetComponent<PlayerHealthBarScript>().HealthGainHelper();

            if (health > 3)
            {
                lowHealthAlarm.volume = 0;
            }
        }

    }

    public void GainHealthInc(float amt)
    {
        if (health < healthMax)
        {
            health += amt;
            healthSpeaker.pitch = health / healthMax;


            healthMonitor.transform.GetComponent<PlayerHealthBarScript>().HealthGainHelper();
        }
    }

    public IEnumerator BlurCamera()
    {
        mainCamera.GetComponent<NoiseAndScratches>().enabled=true;
        mainCamera.GetComponent<NoiseAndScratches>().grainIntensityMin = 3;
        yield return new WaitForSeconds(.2f);
        mainCamera.GetComponent<NoiseAndScratches>().grainIntensityMin = 0;
        if(!isDying)
            mainCamera.GetComponent<NoiseAndScratches>().enabled=false;
    }

    public void DefeatBoss()
    {
        bodyModel.GetComponent<Animator>().SetBool("isThumbingUp", true);
        StartCoroutine(DieCinematic());
    }

    public IEnumerator DieCinematic()
    {
        speaker.PlayOneShot(deathFrazzle);
        Time.timeScale = .5f;
        globalVariables.GetComponent<GlobalVariables>().credits += credits;
        account.GetComponent<AccountInfo>().credits += credits;
        account.GetComponent<AccountInfo>().Save();

        
        GameObject screen = GameObject.Find("FX");
        speaker.PlayOneShot(deathSound);
        minSpeed = 0;

        float whiteout = 0;
        mainCamera.GetComponent<NoiseAndScratches>().enabled = true;
        mainCamera.GetComponent<NoiseAndScratches>().grainIntensityMin = 0;
        
        UI.GetComponent<Animator>().SetBool("Die", true);
        yield return new WaitForSeconds(3);
        
        Time.timeScale = 1;
        SceneManager.LoadScene("AdScreen", LoadSceneMode.Single);
    }

    private void VerticalWallJump()
    {
        

        StartCoroutine(WallClimbReset());
        transform.GetComponent<Rigidbody>().isKinematic = false;
        
        
    }

    public IEnumerator WallClimbReset()
    {
        
        GetComponent<Rigidbody>().isKinematic = false;
        wallClimbCoolDown = true;
        yield return new WaitForSeconds(2);
        wallClimbCoolDown = false;
    }

    public void AdaptToAccount()
    {
        account = GameObject.Find("Account");
        gunCamera = GameObject.Find("Gun Camera");
        health = account.GetComponent<AccountInfo>().health;
        rotXSpeed = account.GetComponent<AccountInfo>().sensitivityTurn;
        lookSpeed = account.GetComponent<AccountInfo>().sensitivityAim;
        shotpoint = GameObject.Find("ShotPoint");
        
        
        
    }

    public void RotationReset()
    {
        transform.rotation = new Quaternion(0, transform.rotation.y,0,0);
        
    }

    private void Unstucker()
    {
        bool isTouching = false;
        Collider[] colliders = Physics.OverlapSphere(transform.position,.25f); 
        {
            foreach (Collider collider in colliders)
            {
                if (collider.tag == "Floor")
                {
                    isTouching = true;
                }
            }
            if (isTouching)
            {
                transform.Translate(Vector3.forward * 1);
                print("KICKED BACK!");
            }
            
        }    
    }
    
    private void HandleInversionControls()
    {
        if (account.GetComponent<AccountInfo>().isInverted)
        {
            invertLook = 1;
        }
        else
        {
            invertLook = -1;
        }
    }

    private void FreeLook()
    {
        {
            float tempLookSpeed = lookSpeed;
            int layerMask = (1 << 10);
            float lockOnGum = 0;

                mainCamera.GetComponent<Animator>().enabled = false;
                rotY = ((((lookField.GetComponent<FreeLookFieldScript>().TouchDist.y / 25) * lookSpeed ) * invertLook));

                //x coordinate lock
                float adjustedRot = mainCamera.transform.eulerAngles.x;
                if (adjustedRot > 180)
                    adjustedRot = (adjustedRot - 360);

                if (rotY > 0 && adjustedRot < lookDnMax)
                {
                    mainCamera.transform.Rotate(rotY, 0, 0);
                    
                }


                if (rotY < 0 && (adjustedRot > lookUpMax))
                {
                    mainCamera.transform.Rotate(rotY, 0, 0);
                    
                }

                transform.Rotate(new Vector3(0, (lookField.GetComponent<FreeLookFieldScript>().TouchDist.x / 25) * lookSpeed, 0));

        }
    }

 
    

}
