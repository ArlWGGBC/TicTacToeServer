using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameRoom
{

    public string roomName;
    public string gameType;
    public string player1ID = null;
    public string player2ID = null;
    public List<string> playerIDs;
    public List<int> spectatorIDs;

    
    public string spectator1;
    public string spectator2;
    public string spectator3;
    public string spectator4;
    public string spectator5;
   
    

    public GameRoom(string roomN)
    {
        player1ID = null;
        player2ID = null;
        spectator2 = null;
        spectator1 = null;
        roomName = roomN;

        playerIDs = new List<string>();
    }
}


