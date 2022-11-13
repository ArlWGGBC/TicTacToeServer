using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class NetworkedServer : MonoBehaviour
{
    int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 8000;

    private MessageType _message;
    private List<int> connectedClients;
    private List<GameRoom> gameRooms;

    private int playersConnected = 0;

    // Start is called before the first frame update
    void Start()
    {
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        reliableChannelID = config.AddChannel(QosType.Reliable);
        unreliableChannelID = config.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(config, maxConnections);
        hostID = NetworkTransport.AddHost(topology, socketPort, null);

        _message = new MessageType();
        connectedClients = new List<int>();
        gameRooms = new List<GameRoom>();
        


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
    
    public void SendPlayersInfo()
    {
        foreach (var connection in connectedClients)
        {
            SendMessageToClient("PLAYERCOUNT," + connectedClients.Count, connection);
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
        Debug.Log("msg recieved = " + msg + ".  connection id = " + id);
        
        string[] message = msg.Split(',');
        
        // message[0] == Signifier, message[1] == Room Name, message[2] == Room owner ID(player1)
        if (_message == null)
        {
            _message = new MessageType();
        }
        
        
        if (message[0] == _message.Join)
        {
            //Assign values from message for clarity
            string roomName = message[1];
            
            foreach (var room in gameRooms)
            {
                if (room.roomName == roomName)
                {
                    SendMessageToClient((_message.Joined + "," + room.roomName + "," + room.player1ID), id);
                }
            }
        }
        else if (message[0] == _message.Create)
        {
            //Assign values from message for clarity
            string roomName = message[1];
            
            //Create new gameroom and assign values.
            GameRoom room = new GameRoom(roomName);
            room.player1ID = id.ToString();
            
            //Add gameroom to list of active game rooms.
            gameRooms.Add(room);
            
            SendMessageToClient((_message.Create + "," + roomName + "," + id), id);
        }
        else if (message[0] == _message.PlayerCount)
        {
            //Send connected players count to all clients
            
        }
    }
    
    

   
        

}


