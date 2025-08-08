using System;
using Feel.FeelDemos.SquashAndStretch.Scripts;
using UnityEngine;

[Serializable]
public class Player
{
    public int ID { get; set; }
    public string Name { get; set; }
    public Color Color { get; set; }

    public CarController Car { get; set; }

    public Player(string name, Color color, int id)
    {
        Name = name;
        Color = color;
        ID = id;
    }
}