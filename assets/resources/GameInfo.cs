using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class GameInfo : Resource
{

    //Note: not a node, this is a resource
    [Export]
    public PackedScene GameScene;

    //we can't get any information from a node (or a scene) directly from something that isn't a node. This is a resource, so we have to do this. annoying, but fine.
    [Export]
    public string ID;

    //Collection of difficulties and weights associated with them. used to create an array in GameManager for a given difficulty
    [Export]
    public Godot.Collections.Dictionary<int, float> DifficultyWeights = new Godot.Collections.Dictionary<int, float>();

}
