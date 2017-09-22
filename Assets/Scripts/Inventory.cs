using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {

    [Header("Settings")]
    public bool weightBased;            // Have inventory use a weight limit
    public bool slotBased;              // Have inventory use slots to display items
    public bool gridBased;              // Have inventory use size to display items
    
    public float maxWeight = 100f;      // Absolute max weight inventory can hold
    public float weightCushion = 0.75f; // At what percentage of maxWeight is considered to be heavy
    public int maxSlots = 100;          // Max slots inventory can have
    public int columns = 5;             // How columns of boxes should the inventory have

    private int rows;
    private List<InventoryItem> items = new List<InventoryItem>();
    private int[,] itemGrid;

    public float weight { get; private set; }
    public bool isOverWeight {
        get {
            return weight >= maxWeight*weightCushion;
        }
    }

    void Awake(){
        if ( gridBased ){
            rows = Mathf.RoundToInt(maxSlots/columns);
            itemGrid = new int[columns,rows];
            for (int i = 0; i < columns; i++){
                for (int j = 0; j < rows; j++){
                    itemGrid[i,j] = -1;
                }
            }
        }
    }

    // Add item to inventory
    public void AddItem(Item item, int amt){
        // Weight check
        if ( weightBased ){
            if ( weight >= maxWeight ) {
                Debug.Log("Cannot hold any more!");
                return;
            }   

            if ( isOverWeight ) {
                Debug.Log("Inventory is heavy!");
            }
        }
        // Slot Check
        if ( slotBased ){
            if ( items.Count >= maxSlots ){
                Debug.Log("Cannot hold anymore!");
                return;
            }
        }
        
        #region Slot Based Inventory
        if ( !gridBased ){
            // Fill previous slots with item first before making a new slot
            IEnumerable<InventoryItem> duplicates = items.Where<InventoryItem>((ii) => ii.item.name == item.name);
            if ( item.stackable && duplicates.Count<InventoryItem>() > 0 ){
                foreach (InventoryItem ii in duplicates){
                    if ( ii.amount == InventoryItem.MAX_AMOUNT ){
                        continue;
                    } else {
                        int amtLeft = InventoryItem.MAX_AMOUNT-ii.amount;
                        if ( amtLeft > 0 ){
                            int diff = amtLeft - amt;
                            if ( diff >= 0 ){
                                ii.amount += amt;
                            } else {
                                ii.amount = InventoryItem.MAX_AMOUNT;
                                amt = Mathf.Abs(diff);
                            }
                        }
                    }
                }

                if ( amt > 0 ){
                    items.Add(new InventoryItem(item,amt,items.Count));
                }
            } else {
                // Item cannot be found in inventory
                if ( item.stackable ){
                    items.Add(new InventoryItem(item,amt,items.Count));
                } else {
                    for (int i = 0; i < amt; i++){
                        items.Add(new InventoryItem(item,1,items.Count));
                    }
                }
            }
        } else {
        #endregion
        #region Grid Based Inventory
            List<Vector2i> spaces = FindSpace(item);
            if ( item.stackable ){
                IEnumerable<InventoryItem> duplicates = items.Where<InventoryItem>((ii) => ii.item.name == item.name);
                if ( duplicates.Count<InventoryItem>() > 0 ){
                    foreach (InventoryItem ii in duplicates){
                        if ( ii.amount == InventoryItem.MAX_AMOUNT ){
                            continue;
                        } else {
                            int amtLeft = InventoryItem.MAX_AMOUNT-ii.amount;
                            if ( amtLeft > 0 ){
                                int diff = amtLeft - amt;
                                if ( diff >= 0 ){
                                    ii.amount += amt;
                                } else {
                                    ii.amount = InventoryItem.MAX_AMOUNT;
                                    amt = Mathf.Abs(diff);
                                }
                            }
                        }
                    }
                }
            }

            if ( amt > 0 && spaces != null ){
                if ( item.stackable ){
                    items.Add( new InventoryItem(item,amt,items.Count) );
                    foreach (Vector2i space in spaces){
                        itemGrid[(int)space.x,(int)space.y] = items.Count-1;
                    }
                } else {
                    for (int i = 0; i < amt; i++){
                        if ( i != 0 ) spaces = FindSpace(item);

                        if ( spaces != null ){
                            items.Add( new InventoryItem(item,amt,items.Count) );
                            foreach (Vector2i space in spaces){
                                itemGrid[(int)space.x,(int)space.y] = items.Count-1;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        // Recaculate weight
        if ( weightBased ) CalculateWeight();
    }

    // Remove specific item in inventory
    public void RemoveItem(Item item, int amt){
        List<InventoryItem> duplicates = items.Where<InventoryItem>((ii) => ii.item.name == item.name).ToList<InventoryItem>();
        if ( duplicates.Count > 0 ){
            List<int> toRemove = new List<int>();

            for (int i = 0; i < duplicates.Count; i++){
                InventoryItem ii = duplicates[i];
                if ( ii.amount < amt ){
                    amt = amt - ii.amount;
                    ii.amount = 0;
                } else {
                    ii.amount -= amt;
                }
                if ( ii.amount < 1 ){
                    int x = i;
                    toRemove.Add(i);
                }
            }
            for (int i = toRemove.Count-1; i >= 0; i--){
                items.RemoveAt(toRemove[i]);

                if ( gridBased ){
                    for (int x = 0; x < columns; x++){
                        for (int y = 0; y < rows; y++){
                            if ( itemGrid[x,y] == toRemove[i] ){
                                itemGrid[x,y] = -1;
                            }
                        }
                    }
                }
            }

            if ( weightBased ) CalculateWeight();
        } else {
            Debug.Log(item.name + " cannot be found in inventory!");
        }
    }
    // Remove item with specific index in inventory
    public void RemoveItem(int index, int amt){
        if ( items.Count > index && index >= 0 ){
            items[index].amount -= amt;
            if ( items[index].amount < 1 ){
                items.RemoveAt(index);

                if ( gridBased ){
                    for (int i = 0; i < columns; i++){
                        for (int j = 0; j < rows; j++){
                            if ( itemGrid[i,j] == index ){
                                itemGrid[i,j] = -1;
                            }
                        }
                    } 
                }
            }
        } else {
            Debug.LogError("Invalid index, " + index);
        }
    }

    private void CalculateWeight(){
        weight = 0f;
        
        foreach (InventoryItem ii in items){
            weight += ii.item.weight*ii.amount;
        }
    }
    private List<Vector2i> FindSpace(Item item){
        List<Vector2i> fill = new List<Vector2i>();
        for (int i = 0; i < item.columns; i++){
            for (int j = 0; j < item.rows; j++){
                fill.Add(new Vector2i(i,j));
            }
        }

        for (int i = 0; i < columns; i++){
            for (int j = 0; j < rows-(item.rows-1); j++){
                if ( HasSpace(fill,i,j) ){
                    foreach (Vector2i f in fill){
                        f.x += i;
                        f.y += j;
                    }
                    return fill;
                }
            }
        }

        return null;
    }
    private bool HasSpace(List<Vector2i> fill, int x, int y){
        foreach (Vector2i f in fill){
            if ( itemGrid[x+f.x,y+f.y] != -1 )
                return false;
        }

        return true;
    }
}
