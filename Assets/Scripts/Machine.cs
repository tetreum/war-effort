using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Peque;
using Peque.Machines;
using System.Linq;

public class Machine
{
    public enum Direction
    {
        Up = 270,
        Left = 180,
        Down = 90,
        Right = 0,
    }
    [System.Serializable]
    public enum Type
    {
        Belt = 1,
        IronOreBroker = 2,
        IronFoundry = 3,
        IronSeller = 4,
    }
    public enum ExecutionType
    {
        Generator = 1,
        Converter = 2,
        Seller = 3,
    }

    public enum ConnectionType
    {
        Output = 0,
        Input = 1,
    }

    public MachineInfo info {
        get {
            return GameGrid.Instance.getMachineInfo(type);
        }
    }

    public Vector3 position;
    public Direction direction;
    public Type type;
    public GameObject gameObject;
    public Dictionary<Vector3, ConnectionType> connections = new Dictionary<Vector3, ConnectionType>();
    public Dictionary<Item.Type, int> storedItems = new Dictionary<Item.Type, int>();
    public int ticksUntilNextExecution = 0;

    public Machine (GameObject gameObject, Type type, Vector3 position) {
        this.gameObject = gameObject;
        this.type = type;
        this.position = position;
    }

    public void delete () {
        if (connections != null) {
            foreach (Vector3 neighbor in connections.Keys) {
                Machine neighborMachine = GameGrid.Instance.getMachineAt(neighbor);

                if (neighborMachine.type == Type.Belt) {
                    ((Belt)neighborMachine).removeConnection(position, type);
                } else {
                    neighborMachine.removeConnection(position, type);
                }
            }
        }

        GameObject.Destroy(gameObject);

        foreach(Vector3 pos in GameGrid.Instance.getMachineBlocks(position, type)) {
            GameGrid.Instance.grid.Remove(pos);
        }
        GameGrid.Instance.machines.Remove(position);
    }

    public void addConnection (Vector3 neighbor, ConnectionType connectionType) {
        if (connections.ContainsKey(neighbor)) {
            return;
        }
        connections.Add(neighbor, connectionType);

        onUpdateConnection(neighbor);
    }

    /**
     * This method receives the "main" grid position of neighbor machine,
     * but since it may be linked from another of it's blocks
     * we need to check each one individually
    */
    public void removeConnection (Vector3 neighbor, Type type) {
        foreach (var pos in GameGrid.Instance.getMachineBlocks(neighbor, type)) {
            if (connections.ContainsKey(pos)) {
                connections.Remove(pos);
                onUpdateConnection(pos);
            }
        }
    }

    protected virtual void onUpdateConnection(Vector3 neighbor) {
    }

    public virtual void run () {
        ticksUntilNextExecution--;

        if (ticksUntilNextExecution > 0) {
            return;
        }

        MachineInfo machineInfo = GameGrid.Instance.getMachineInfo(type);

        // reset timer
        ticksUntilNextExecution = machineInfo.ticksBetweenExecution;

        if (machineInfo.executionType != ExecutionType.Seller) {
            sendItemsToConveyorBelt(machineInfo);
        }
        
        if (!hasRequiredItemsToProduce(machineInfo)) {
            return;
        }

        if (machineInfo.executionType != ExecutionType.Seller && !canStoreProducedItems(machineInfo)) {
            return;
        }

        deductItemsToProduce(machineInfo);

        switch (machineInfo.executionType) {
            case ExecutionType.Converter:
            case ExecutionType.Generator:
                foreach (ItemUI item in machineInfo.itemsThatProduces) {
                    if (!storedItems.ContainsKey(item.type)) {
                        storedItems.Add(item.type, 0);
                    }
                    storedItems[item.type] += item.quantity;
                }
                break;
            case ExecutionType.Seller:
                GameGrid.Instance.money += info.moneyThatGenerates;
                break;
        }
    }

    private void sendItemsToConveyorBelt (MachineInfo machineInfo) {
        bool hasItemsToSend = false;

        foreach (ItemUI item in machineInfo.itemsThatProduces) {
            if (storedItems.ContainsKey(item.type) && storedItems[item.type] > 0) {
                hasItemsToSend = true;
                break;
            }
        }

        if (!hasItemsToSend) {
            return;
        }

        // know how many output connections has to divide the output between
        int outputConnections = 0;

        foreach (KeyValuePair<Vector3, ConnectionType> connection in connections) {
            if (connection.Value == ConnectionType.Output && ((Belt)GameGrid.Instance.getMachineAt(connection.Key)).hasFreeSlots) {
                outputConnections++;
            }
        }

        if (outputConnections < 1) {
            return;
        }

        // TEMPORAL
        storedItems[machineInfo.itemsThatProduces.First().type]--;

        foreach (KeyValuePair<Vector3, ConnectionType> connection in connections) {
            Belt belt = (Belt)GameGrid.Instance.getMachineAt(connection.Key);
            if (connection.Value == ConnectionType.Output && belt.hasFreeSlots) {
                belt.addItem(machineInfo.itemsThatProduces.First().type);
                break;
            }
        }
    }

    private bool hasRequiredItemsToProduce (MachineInfo machineInfo) {
        foreach (ItemUI item in machineInfo.requiredItemsToProduce) {
            if (!storedItems.ContainsKey(item.type) || storedItems[item.type] < item.quantity) {
                return false;
            }
        }
        return true;
    }

    private void deductItemsToProduce (MachineInfo machineInfo) {
        foreach (ItemUI item in machineInfo.requiredItemsToProduce) {
            storedItems[item.type] -= item.quantity;
        }
    }

    public bool canStoreItem (Item.Type type, int quantity = 1) {
        int currentQuantity = storedItems.ContainsKey(type) ? storedItems[type] : 0;
        if ((currentQuantity + quantity) > info.storageLimits[type]) {
            return false;
        }
        return true;
    }

    private bool canStoreProducedItems(MachineInfo machineInfo) {
        switch (machineInfo.executionType) {
            case ExecutionType.Converter:
            case ExecutionType.Generator:
                foreach (ItemUI item in machineInfo.itemsThatProduces) {
                    if (!canStoreItem(item.type, item.quantity)) {
                        return false;
                    }
                }
                break;
        }
        return true;
    }
}
