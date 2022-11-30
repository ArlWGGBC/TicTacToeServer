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
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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
    private List<GameRoom> gameRooms;

    private int playersConnected = 0;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        reliableChannelID = config.AddChannel(QosType.Reliable);
        unreliableChannelID = config.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(config, maxConnections);
        hostID = NetworkTransport.AddHost(topology, socketPort, null);

        _message = new MessageType();
        connectedClients = new List<int>();
        gameRooms = new List<GameRoom>();

        StartCoroutine(GameRooms());

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

                    foreach (var pID in room.playerIDs)
                    {
                        SendMessageToClient((_message.Joined + "," + room.roomName + "," + room.playerIDs[0] + "," + room.playerIDs[1]) + "," + room._Tiles[0] + "," + room._Tiles[0]+ "," + room._Tiles[0] + "," + room._Tiles[0] + "," + room._Tiles[0] + "," + room._Tiles[0] + "," + room._Tiles[0] + "," + room._Tiles[0] + "," + room._Tiles[0], + Convert.ToInt32(pID));
                    }

                    if (room.playerIDs.Count > 1)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if (i % 2 == 0)
                            {
                                SendMessageToClient(_message.GameStart + "," + TileType.X.ToString(), Convert.ToInt32(room.playerIDs[i]));
                                Debug.Log((room.playerIDs[i] + " : Assigned : " + TileType.X));
                            }
                            else
                            {
                                SendMessageToClient(_message.GameStart + "," + TileType.O.ToString(), Convert.ToInt32(room.playerIDs[i]));
                                Debug.Log(room.playerIDs[i] + " : Assigned : " + TileType.O);
                            }
                        
                        }
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
                    Debug.Log("Adding player : " + id.ToString());
                    gameRoom.playerIDs.Add(id.ToString());
                    SendMessageToClient((_message.Join + "," + roomName + "," + gameRoom.playerIDs[0]+ "," + gameRoom.playerIDs[1]), id);

                    if (id == 1)
                    {
                        SendMessageToClient(_message.GameStart + "," + roomName + "," + "O", id);
                    }
                    else
                    {
                        SendMessageToClient(_message.GameStart + "," + roomName + "," + "X", id);
                    }
                   
                    return;
                }
            }
            
            //Create new gameroom and assign values.
            GameRoom room = new GameRoom(roomName);
            Debug.Log("Adding player : " + id.ToString());
            room.playerIDs.Add(id.ToString());

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
                    foreach (var playerID in room.playerIDs)
                    {
                        ds.Add(playerID);
                    }


                    //If player who LEFT matches with ID inside game room, remove from game room.

                    foreach (var pID in room.playerIDs)
                    {
                        Debug.Log(pID + "SENDING ! :" + _message.Leave + "," + room.roomName + "," + id);
                        SendMessageToClient(_message.Leave + "," + room.roomName + "," + id, Convert.ToInt32(pID));
                    }

                    foreach (var identity in room.playerIDs)
                    {
                        if (identity == id.ToString())
                        {
                            room.playerIDs.Remove(id.ToString());
                            break;
                        }
                    }

                    if (room.playerIDs.Count == 0)
                    {
                        gameRooms.Remove(room);
                    }
                }
            }
        }
        else if (message[0] == _message.Message)
        {

            foreach (var room in gameRooms)
            {

                if (room.roomName == message[1])
                {
                    foreach (var playerID in room.playerIDs)
                    {
                        Debug.Log("PLAYERID : " + playerID + "Sending Message : " + message[2] + " + " + id);
                        SendMessageToClient(_message.Message + "," + message[2] + "," + id, Convert.ToInt32(playerID));
                    }

                    /*foreach (var spectatorID in room.spectatorIDs)
                    {
                        Debug.Log(message[2] + " " + message[3]);
                        SendMessageToClient(_message.Message + "," + message[2] + "," + message[3], Convert.ToInt32(spectatorID));
                    }*/
                }
            }
        }
        else if (message[0] == _message.MakeMove)
        {

            //TODO: Store tic tac toe data inside GameRoom
            foreach (var room in gameRooms)
            {
                Debug.Log("Roomname" + room.roomName + "   message :" + message[1]);
                //TODO : Fix identifier logic
                if (room.roomName == message[1])
                {
                    //If last moved player makes another move, return;
                    if (message[2] == room.lastMoved.ToString())
                    {
                        return;
                    }
                    
                    //Last moved set to identifier of client who sent move message.
                    room.lastMoved = message[2];
                    
                    foreach (var tile in room._Tiles)
                    {
                        if (Convert.ToInt32(message[3]) == tile._position)
                        {
                            if (tile._tileType == TileType.N.ToString())
                            {
                                
                                Debug.Log("New Move : " + message[3] + " : " + message[2]);
                                tile._tileType = message[2];
                                
                                
                                room.MoveRecord.Add(tile);
                                
                                break;
                            }
                            

                        }
                    }
                    
                    
                    
                    //Debug.Log((GameRoom.TileType)Enum.Parse(typeof(GameRoom.TileType), message[2]));
                    foreach (var playerID in room.playerIDs)
                    {
                        //makemove[0] , boardposition[1] , identity[2]
                        SendMessageToClient(_message.MakeMove + "," + message[3] + "," + message[2], Convert.ToInt32(playerID));
                    }
                    
                    int[] position = room.PositionHelper(Convert.ToInt32(message[3]));
                    room.MakeMove(position[0] - 1, position[1] - 1, message[2]);
                }
            }
        }
        else if (message[0] == _message.GameStart)
        {
            
        }
        else if (message[0] == _message.GetReplays)
        {
            GetReplays(id);
        }
    }



    private void GetReplays(int id)
    {
        
        var fileInfo = Directory.GetFiles(Application.persistentDataPath + "/" + "Replays");


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
                SendMessageToClient(_message.GetReplays + ","  + roomName + "," + winner + "," + move.pos + "," + move.identifier, id);
            }
        }
    }
    IEnumerator GameRooms()
    {
        yield return new WaitForSeconds(2f);
        if (gameRooms.Count > 0)
        {
            foreach (var room in gameRooms)
            {
                Debug.Log("Room : " + room.roomName);
            }
        }

        StartCoroutine(GameRooms());
    }
    private void LoadGames(int gameID)
    {
        
    }
    private void SpectatorJoin()
    {
        throw new NotImplementedException();
    }
}


