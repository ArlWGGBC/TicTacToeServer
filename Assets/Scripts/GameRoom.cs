using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public enum TileType
{
    X,
    O,
    N
}

public class Tile
{
    
    
    public int _position;
    public string _tileType;

    

    public Tile(int position, string tileType)
    {
        _position = position;
        _tileType = tileType;

    }
        
        
        
    public override string ToString() => $"({_position}, {_tileType})";
}
public class GameRoom
{
    private NetworkedServer _server;
    private int n = 3;

    public string[,] ToeBoard;
    public string roomName;
    public string gameType;
    public string player1ID = null;
    public string player2ID = null;
    
    
    public List<string> playerIDs;
    public List<string> spectatorIDs;
    

    public List<Tile> _Tiles;
    public List<Tile> MoveRecord;


    public string lastMoved = "N";
    

    public GameRoom(string roomN)
    {
        player1ID = null;
        player2ID = null;
        roomName = roomN;

        //Players(connection ids) currently in game.
        playerIDs = new List<string>();
        
        //Tiles currently in play.
        _Tiles = new List<Tile>();
        
        //For replays & Players joining
        MoveRecord = new List<Tile>();
        
        //To check game logic
        ToeBoard = new string[3, 3];
        
        
        
        for (int i = 0; i < 9; i++)
        {
            _Tiles.Add(new Tile(i + 1, TileType.N.ToString()));
        }
        
    }


    public Tile getTile(int position)
    {
        foreach (var tile in _Tiles)
        {
            if (position == tile._position)
            {
                return tile;
            }
        }

        return default;
    }


    public void setTile(int position, string identifier)
    {
        foreach (var tile in _Tiles)
        {
            
            if (position == tile._position)
            {
                tile._tileType = identifier;
            }
        }
    }



    public void MakeMove(int row, int col, string Identifier)
    {
        ToeBoard[row, col] = Identifier;
        
        CheckMove(row, col, Identifier);
    }
    public void CheckMove(int row, int col, string identifier)
    {

        string id = identifier;
        //col
        for(int i = 0; i < n; i++){
            if(ToeBoard[row, i] != identifier)
                break;
            if(i == n - 1){
                Debug.Log(identifier + " : Wins!");
                ResetRoom();
            }
        }
        
        //row
        for(int i = 0; i < n; i++){
            if(ToeBoard[i, col] != identifier)
                break;
            if(i == n - 1){
                Debug.Log(identifier + " : Wins!");
                ResetRoom();
            }
        }
        
        //diagonal
        if(row == col){
            //we're on a diagonal
            for(int i = 0; i < n; i++){
                if(ToeBoard[i, i] != identifier)
                    break;
                if(i == n - 1){
                    Debug.Log(identifier + " : Wins!");
                    ResetRoom();
                }
            }
        }
            
        //anti diagonal
        if(row + col == n - 1){
            for(int i = 0; i < n; i++){
                if(ToeBoard[i, (n - 1)-i] != identifier)
                    break;
                if(i == n - 1){
                    Debug.Log(identifier + " : Wins!");
                    ResetRoom();
                }
            }
        }
    }

    public void ResetRoom()
    {
        
        SaveGame(lastMoved);
        
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                ToeBoard[i,j] = TileType.N.ToString();
            }
        }

        foreach (var playerID in playerIDs)
        {
            _server.SendMessageToClient(_server._message.GameWon + "," + lastMoved, Convert.ToInt32(playerID));
        }
        
    }

    public void SaveGame(string winner)
    {
        
        int seed = (int)DateTime.Now.Ticks;
        Random.seed = seed;
        FileInfo info = new FileInfo(Application.persistentDataPath + "/" + "Replays");
        
        if(!info.Directory.Exists)
            Directory.CreateDirectory(info.DirectoryName);

        int gameID = Random.Range(1000, 10000);
        
        string filePath = info + "/" + roomName + gameID + ".sav";

        if (File.Exists(filePath))
        {
            Debug.Log("Game already exists");
            return;
        }
            
        Debug.Log(filePath);
        
        StreamWriter sw = new StreamWriter(filePath);
        sw.WriteLine("name" + "," + roomName);
        sw.WriteLine("ID" + "," +  gameID);
        sw.WriteLine("Winner" + "," + winner);
        for (int i = 0; i < MoveRecord.Count; i++)
        {
            sw.WriteLine("Move" + "," + MoveRecord[i]._position + "," + MoveRecord[i]._tileType);
        }

        sw.Close();
    }


    public int[] PositionHelper(int position)
    {
        int x = 0;
        int y = 0;

        if (position <= 3)
        {
            x = 1;
            y = position;
        }
        else if (position <= 6)
        {
            x = 2;
            y = position - 3;
        }
        else if (position <= 9)
        {
            x = 3;
            y = position - 6;
        }


        return new int[] {x, y};

    }
}

public struct Move
{

    public Move(string position, string id)
    {
        pos = position;
        identifier = id;
    }
    
    
    public string pos;
    public string identifier;
}





