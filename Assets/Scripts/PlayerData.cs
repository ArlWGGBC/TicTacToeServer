using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;
[RequireComponent(typeof(NetworkedServer))]
public class PlayerData : MonoBehaviour
{
    private NetworkedServer _server;
    private string name;
    private string password;

    public void Awake()
    {
        _server = GetComponentInChildren<NetworkedServer>();
    }

    public string Name
    {
        get => name;
        set => name = value;
    }

    public string Password
    {
        get => password;
        set => password = value;
    }


    public bool SaveAccount()
    {
        string filePath = Application.persistentDataPath + "/" + Name + ".sav";

        if (File.Exists(filePath))
        {
            
            Debug.Log("User already exists");
            return false;
        }


        StreamWriter sw = new StreamWriter(filePath);
        sw.WriteLine("name" + "," + Name);
        sw.WriteLine("password" + "," + Password);
        sw.WriteLine("UniqueID" + Random.Range(1000, 3000));

        sw.Close();

        return true;
    }

    public bool LoginAccount(string name, string password)
    {
        bool nameCorrect = false;
        bool passwordCorrect = false;

        string saveFilePath = Application.persistentDataPath + "/account.sav";
        var fileInfo = Directory.GetFiles(Application.persistentDataPath);


        if (fileInfo.Length <= 0)
            return false;

        foreach (var file in fileInfo)
        {
            StreamReader sr = new StreamReader(file);

            string line;

            while ((line = sr.ReadLine()) != null)
            {
                string[] stats = line.Split(',');

                if (stats[0] == "name")
                {
                    if (stats[1] == Name)
                    {
                        nameCorrect = true;
                    }
                }
                else if (stats[0] == "password")
                {
                    if (stats[1] == Password)
                    {
                        passwordCorrect = true;
                    }
                }
            }
            
            sr.Close();
            
            if (nameCorrect && passwordCorrect)
            {
                return true;
            }
        }

        return false;


    }
    
    
    
}
