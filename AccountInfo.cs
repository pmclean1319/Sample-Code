using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AccountInfo : MonoBehaviour {
    public bool isNewAccount = true;


    public string handle;
    public int credits;
    public List<string> guns = new List<string>() { "test" };
    public List<string> armors = new List<string>() { "test" };
    public List<string> testString = new List<string>();
    public string equippedGun;
    public string equippedArmor;
    public static GameObject account;
    public GameObject loadoutGun;
    public float sensitivityTurn =75, sensitivityAim =10;
    public bool visFX;
    public int health = 999;
    public int creditsToAdd = 0;

    public bool gameHasStarted = false;
    public bool cinematicHasShown = false;
    public bool isResettingAccountData = false;

    public GameObject opPanel;
    public bool bossHasBeenDefeated = false;
    public bool isSpecialGunEquipped = false;
    public bool isInverted = false;

    public int creditLinkIncrease = 0;
    public bool isCreditLinked = false;
    public int creditLinkTimer = 500;

    public int bloodLinkIncrease = 0;
    public bool isBloodLinked = false;
    public int bloodLinkTimer = 0;

    public bool lockOnEnabled = false;

    public float[] levelProgress;

    public int currentLevel;

    // Use this for initialization
    void Start () {
        if (account == null)
        {
            DontDestroyOnLoad(gameObject);
            account = this.gameObject;
            //gameHasStarted = true;
//            StartCoroutine(CreditLinkCountdown());
        }
        else if(account != this.gameObject)
        {
            Destroy(gameObject);
        }
        if (isResettingAccountData == true)
        {
            isResettingAccountData = false;
            Save();
        }
        Load();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        CreditLinkCountdown();
        BloodLinkCountdown();
	}

    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/playerInfo.dat");

        PlayerData data = new PlayerData();
        data.handle = handle;
        data.credits = credits;
        data.guns = guns;
        data.armors = armors;
        data.equippedGun = equippedGun;
        data.equippedArmor = equippedArmor;
        data.sensitivityTurn = sensitivityTurn;
        data.sensitivityAim = sensitivityAim;
        data.visFX = visFX;
        data.isNewAccount = isNewAccount;
        data.isSpecialGunEquipped = isSpecialGunEquipped;
        data.levelProgress = levelProgress;
        data.lockOnEnabled = lockOnEnabled;
        data.isInverted = isInverted;

        bf.Serialize(file, data);
        file.Close();
    }

    public void Load()
    {
        if(File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);
            PlayerData data = (PlayerData)bf.Deserialize(file);
            file.Close();

            handle = data.handle;
            credits = data.credits;
            guns = new List<string>();
            for (int i = 0; i < data.guns.Count; i++)
            {
                guns.Add(data.guns[i]);
            }
            armors = new List<string>();
            for (int i = 0; i < data.armors.Count; i++)
            {
                armors.Add(data.armors[i]);
            }
            equippedGun = data.equippedGun;
            equippedArmor = data.equippedArmor;
            sensitivityTurn = data.sensitivityTurn;
            sensitivityAim = data.sensitivityAim;
            visFX = data.visFX;
            isNewAccount = data.isNewAccount;
            isSpecialGunEquipped = data.isSpecialGunEquipped;
            levelProgress = data.levelProgress;
            lockOnEnabled = data.lockOnEnabled;
            isInverted = data.isInverted;
        }
        else
        {
            print("File Not There");
        }
    }

    public void AddGun(string newgun)
    {
        guns.Add(newgun);
        Save();
    }

    public void RemoveGun(int selected)
    {
        if (guns[selected] != equippedGun)
        {
            guns.Remove(guns[selected]);
            credits += 100;
        }
    }

    public void AddArmor(string newArmor)
    {
        armors.Add(newArmor);
        Save();
    }

    public void RemoveArmor(int selected)
    {
        if (armors[selected] != equippedGun)
        {
            armors.Remove(armors[selected]);
            credits += 100;
        }
    }

    public void SetEquippedGun(string dataString)
    {
        equippedGun = dataString;
        Save();
    }

    public void AcquireOpPanel()
    {
        if (opPanel = null)
        {
            opPanel = GameObject.Find("opPanel");
        }
    }

    public void CreditLinkCountdown()
    {
        {
            if (isCreditLinked)
            {
                creditLinkTimer--;
                if (creditLinkTimer < 1)
                    {
                        creditLinkIncrease = 0;
                        isCreditLinked = false;
                    }
            }

            
        }
        
        
    }
    
    public void BloodLinkCountdown()
    {
        {
            if (isBloodLinked)
            {
                bloodLinkTimer--;
                
                if (bloodLinkTimer < 1)
                {
                    bloodLinkIncrease = 0;
                    isBloodLinked = false;
                }
            }


        }


    }

    public void CallDelete()
    {
        DeleteAllSaveFiles();
    }

    public static void DeleteAllSaveFiles()
    {
        string path = Application.persistentDataPath + "/playerInfo.dat";
        DirectoryInfo directory = new DirectoryInfo(path);
        directory.Delete(true);
        
        //Directory.CreateDirectory(path);
    }

}


[System.Serializable]
class PlayerData
{
    public string handle;
    public int credits;
    public List<string> guns;
    public List<string> armors;

    public string equippedGun;
    public string equippedArmor;
    public float sensitivityTurn;
    public float sensitivityAim;
    public bool visFX;
    public bool isNewAccount;
    public bool isSpecialGunEquipped;
    public bool lockOnEnabled;
    public bool isInverted;
    public float[] levelProgress;
}