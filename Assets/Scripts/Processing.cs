using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Processing : MonoBehaviour
{
    private NetworkedServer _server;
    private MessageType _message;
    private List<GameRoom> gameRooms;
    private PlayerData _playerData;
    private void Start()
    {
        _server = FindObjectOfType<NetworkedServer>();
        _playerData = FindObjectOfType<PlayerData>();
        _message = new MessageType();
        gameRooms = new List<GameRoom>();
    }

    private void SendMessageToClient(string msg, int id)
    {
        _server.SendMessageToClient(msg,id);
    }
    public void ProcessMessage(string msg, int id)
    {
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
         
                             if (room.playerIDs.Count <= 0)
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
                     _server.GetReplays(id);
                 }
                 else if (message[0] == _message.CreateAccount)
                 {
                     _playerData.Name = message[1];
                     _playerData.Password = message[2];
                     if (_playerData.SaveAccount())
                     {
                         SendMessageToClient(_message.CreateAccount + "," + "1", id);
         
                     }
                     else
                     {
                         SendMessageToClient(_message.CreateAccount + "," + "0", id);
                     }
                     
                 }
                 else if (message[0] == _message.LoginAccount)
                 {
                     if (_playerData.LoginAccount(message[1], message[2]))
                     {
                         SendMessageToClient(_message.LoginAccount + "," + "1", id);
                     }
                     else
                     {
                         SendMessageToClient(_message.LoginAccount + "," + "0", id);
                     }
                 }           return;
                }
            }
    }
}
