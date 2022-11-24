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
                    
                    if (room.player2ID == null)
                        room.player2ID = id.ToString();
                    if (room.player1ID == null)
                        room.player1ID = id.ToString();
                    
                    
                    List<string> ds = new List<string>();
                    if (ds == null) throw new ArgumentNullException(nameof(ds));

                    ds.Add(room.player1ID);
                    ds.Add(room.player2ID);
                    


                    foreach (var pID in ds)
                    {
                        SendMessageToClient((_message.Joined + "," + room.roomName + "," + room.player1ID + "," + room.player2ID), Convert.ToInt32(pID));
                    }
                }
            }
        }
        else if (message[0] == _message.Create)
        {
            //Assign values from message for clarity
            string roomName = message[1];



            foreach (var gameRoom in gameRooms)
            {
                if (gameRoom.roomName == message[1])
                {
                    SendMessageToClient((_message.Join + "," + roomName + "," + gameRoom.player1ID + "," + gameRoom.player2ID), id);
                    return;
                }
            }
            
            //Create new gameroom and assign values.
            GameRoom room = new GameRoom(roomName);
            room.player1ID = id.ToString();
            
            //Add gameroom to list of active game rooms.
            gameRooms.Add(room);
            
            //Send
            SendMessageToClient((_message.Create + "," + roomName + "," + id), id);
        }
        else if (message[0] == _message.PlayerCount)
        {
            
            
        }
        else if (message[0] == _message.Leave)
        {
            foreach (var room in gameRooms)
            {
                if (message[1] == room.roomName)
                {
                    
                    List<string> ds = new List<string>();
                    if (ds == null) throw new ArgumentNullException(nameof(ds));

                    //Create list of all players/spec inside game to send message to them to update UI.
                    ds.Add(room.player1ID);
                    ds.Add(room.player2ID);
                    
                    
                    
                    //If player who LEFT matches with ID inside game room, remove from game room.
                    
                    if (id.ToString() == room.player1ID)
                    {
                        room.player1ID = null;
                        
                    }
                    else if (id.ToString() == room.player2ID)
                    {
                        room.player2ID = null;
                        
                    }
                    
                    
                    foreach (var pID in ds)
                    {
                        Debug.Log(pID + "SENDING ! :" + _message.Leave + "," + room.roomName + "," + id);
                        SendMessageToClient(_message.Leave + "," + room.roomName + "," + id, Convert.ToInt32(pID));
                    }
                }
            }
        }
        else if (message[0] == _message.MakeMove)
        {
            
        }
    }

    private void SpectatorJoin()
    {
        throw new NotImplementedException();
    }
}


