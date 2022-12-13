using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerData))]
public class NetworkedServer : MonoBehaviour
{
    private static NetworkedServer instance = null;

   
    private NetworkedServer()
    {
    }

    public static NetworkedServer Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new NetworkedServer();
            }
            return instance;
        }
    }

    int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 8000;

    public MessageType _message;
    private List<int> connectedClients;
    
    Processing _processing;
    private int playersConnected = 0;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        //network stuff
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        reliableChannelID = config.AddChannel(QosType.Reliable);
        unreliableChannelID = config.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(config, maxConnections);
        hostID = NetworkTransport.AddHost(topology, socketPort, null); 
        ///--------



        connectedClients = new List<int>();
        _processing = FindObjectOfType<Processing>();
        if (_processing == null)
            _processing = this.AddComponent<Processing>();
    }

    // Update is called once per frame
    void Update()
    {

        int recHostID;
        int recConnectionID;
        int recChannelID;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error = 0;

        NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID, out recChannelID, recBuffer, bufferSize, out dataSize, out error);

        switch (recNetworkEvent)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("Connection, " + recConnectionID);
                connectedClients.Add(recConnectionID);
                playersConnected++;
                SendPlayersConnectedInfo();
                break;
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                ProcessRecievedMsg(msg, recConnectionID);
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Disconnection, " + recConnectionID);
                connectedClients.Remove(recConnectionID);
                playersConnected--;
                SendPlayersConnectedInfo();
                break;
        }

    }


    public void SendPlayersConnectedInfo()
    {
        foreach (var connection in connectedClients)
        {
            Debug.Log("Sending message to : " + connection + " : " + _message.PlayerCount + connectedClients.Count);
            SendMessageToClient(_message.PlayerCount + "," + connectedClients.Count, connection);
            
        }
       
    }
    
    public void SendMessageToClient(string msg, int id)
    {
        byte error = 0;
        byte[] buffer = Encoding.Unicode.GetBytes(msg);
        NetworkTransport.Send(hostID, id, reliableChannelID, buffer, msg.Length * sizeof(char), out error);
    }
    
    //PROCESS ALL MESSAGES RECEIVED FROM CLIENTS
    private void ProcessRecievedMsg(string msg, int id)
    {
        Debug.Log("msg recieved = " + msg + " connection id = " + id);
        _processing.ProcessMessage(msg, id);
        
            
          
    }

    
    public void GetReplays(int id)
    {
        
        var fileInfo = Directory.GetFiles(Application.persistentDataPath + "/" + "Replays");


        foreach (var files in fileInfo)
        {
            Debug.Log(files);
        }

        if (fileInfo.Length <= 0)
            return;


        foreach (var file in fileInfo)
        {
            //Read in information... we will filter it on client end using room name.
            List<Move> moves;
            moves = new List<Move>();
            
            
            string roomName = null;
            int ID;
            string winner = null;
            
            StreamReader sr = new StreamReader(file);

            string line;

            while ((line = sr.ReadLine()) != null)
            {
                
               

                int moveCount;

                string[] stats = line.Split(',');

                if (stats[0] == "name")
                {
                    roomName = stats[1];
                }
                else if (stats[0] == "ID")
                {
                    ID = Convert.ToInt32(stats[1]);
                }
                else if (stats[0] == "Winner")
                {
                    winner = stats[1];
                }
                else if (stats[0] == "Move")
                {
                    
                    moves.Add(new Move(stats[1], stats[2]));
                }
                

            }

            foreach (var move in moves)
            {
                Debug.Log(_message.GetReplays + ","  + roomName + "," + winner + "," + move.pos + "," + move.identifier);
                SendMessageToClient(_message.GetReplays + ","  + roomName + "," + winner + "," + move.pos + "," + move.identifier, id);
            }
        }
    }
    
}


