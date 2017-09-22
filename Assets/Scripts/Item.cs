using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item {

    public string name;
    public string description;
    public string iconPath;
    public bool stackable;
    public float weight;
    public int columns, rows;

    public Item(){
        name = "";
        description = "";
        iconPath = "";
        stackable = true;
        weight = 0f;
        columns = 1;
        rows = 1;
    }
    public Item(string name, string description, string iconPath, bool stackable, float weight, int columns, int rows){
        this.name = name;
        this.description = description;
        this.iconPath = iconPath;
        this.stackable = stackable;
        this.weight = weight;
        this.columns = columns;
        this.rows = rows;
    }

}
