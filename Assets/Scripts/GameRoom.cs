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
    public List<string> spectatorIDs;
    
   
    

    public GameRoom(string roomN)
    {
        player1ID = null;
        player2ID = null;
        roomName = roomN;

        playerIDs = new List<string>();
    }
}


