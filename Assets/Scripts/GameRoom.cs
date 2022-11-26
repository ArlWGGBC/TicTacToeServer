using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
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


    
    public string roomName;
    public string gameType;
    public string player1ID = null;
    public string player2ID = null;
    
    
    public List<string> playerIDs;
    public List<string> spectatorIDs;
    

    public List<Tile> _Tiles;
    
    

    public GameRoom(string roomN)
    {
        player1ID = null;
        player2ID = null;
        roomName = roomN;

        playerIDs = new List<string>();
        _Tiles = new List<Tile>();

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
    
}

public class EnumHelper : Attribute
{
    public Type EnumType;
    public EnumHelper(Type enumType)
    {
        EnumType = enumType;
    }
}




