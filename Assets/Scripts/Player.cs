using System;
using UnityEngine;

[Serializable]
public class Player
{
    public int ID { get; set; }
    public string Name { get; set; }
    public Color Color { get; set; }

    public GameObject Car { get; set; }

    public Player(string name, Color color, int id)
    {
        Name = name;
        Color = color;
        ID = id;
    }
}