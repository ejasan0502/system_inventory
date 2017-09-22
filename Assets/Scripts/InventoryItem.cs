using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem {

    public const int MAX_AMOUNT = 100;

    public Item item;
    public int amount;
    public int index = 0;

    public InventoryItem(Item item, int amount, int index){
        this.item = item;
        this.amount = amount;
        this.index = index;
    }

}
