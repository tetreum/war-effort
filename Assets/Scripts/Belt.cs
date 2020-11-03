using System.Collections.Generic;
using UnityEngine;

namespace Peque.Machines
{
    public class Belt : Machine
    {
        public Dictionary<System.Guid, Item.Type> items = new Dictionary<System.Guid, Item.Type>();
        public Dictionary<int, System.Guid> itemPositions = new Dictionary<int, System.Guid>();
        public Dictionary<int, Vector3> positions = new Dictionary<int, Vector3>() {
            {1, new Vector3(-0.458f, 1, 0)},
            {2, new Vector3(-0.151f, 1, 0)},
            {3, new Vector3(0.194f, 1, 0)},
            {4, new Vector3(0.44f, 1, 0)},
        };

        public bool hasFreeSlots {
            get {
                return !itemPositions.ContainsKey(1);
            }
        }

        public Belt (GameObject gameObject) : base(gameObject, Type.Belt, gameObject.transform.position)  {
        }

        public void addItem (Item.Type type) {
            if (!hasFreeSlots) {
                Debug.LogError("Trying to add new item to a busy belt " + type);
                return;
            }

            Vector3 spawnPos = position;
            spawnPos.y = 1;

            Item item = new Item();
            item.id = System.Guid.NewGuid();
            item.type = type;
            item.position = spawnPos;
            item.parent = position;

            GameGrid.Instance.items.Add(item.id, item);
            GameGrid.Instance.itemsToCreate.Enqueue(item.id);

           // GameObject obj = GameGrid.Instantiate(GameGrid.Instance.getItemInfo(type).prefab, spawnPos, Quaternion.Euler(-90, 0, 0), gameObject.transform);

            addToItems(item.id, type);
        }

        public void addToItems (System.Guid item, Item.Type type) {
            items.Add(item, type);
            itemPositions.Add(1, item);

            GameGrid.Instance.items[item].parent = position;
            GameGrid.Instance.items[item].position = positions[1];
            GameGrid.Instance.itemsToMove.Enqueue(item);
        }

        public override void run() {
            // start moving last item
            if (itemPositions.ContainsKey(4)) {
                Item.Type itemType = items[itemPositions[4]];
                foreach (KeyValuePair<Vector3, ConnectionType> connection in connections) {
                    if (connection.Value == ConnectionType.Input) {
                        continue;
                    }

                    Machine neighborMachine = GameGrid.Instance.getMachineAt(connection.Key);

                    if (neighborMachine.type == Type.Belt) {
                        Belt belt = (Belt)neighborMachine;
                        if (belt.hasFreeSlots) {
                            belt.addToItems(itemPositions[4], itemType);

                            // unlink from this belt
                            items.Remove(itemPositions[4]);
                            itemPositions.Remove(4);
                            break;
                        }
                    } else {
                        // check if item can be stored in that machine
                        if (!neighborMachine.info.storageLimits.ContainsKey(itemType) || !neighborMachine.canStoreItem(itemType)) {
                            continue;
                        }

                        if (!neighborMachine.storedItems.ContainsKey(itemType)) {
                            neighborMachine.storedItems.Add(itemType, 0);
                        }
                        neighborMachine.storedItems[itemType]++;

                        // unlink from this belt
                        items.Remove(itemPositions[4]);
                        //GameGrid.Destroy(itemPositions[4].gameObject);
                        itemPositions.Remove(4);
                    }
                }
            }

            if (itemPositions.ContainsKey(3) && !itemPositions.ContainsKey(4)) {
                GameGrid.Instance.moveItem(itemPositions[3], positions[4]);
                itemPositions.Add(4, itemPositions[3]);
                itemPositions.Remove(3);
            }

            if (itemPositions.ContainsKey(2) && !itemPositions.ContainsKey(3)) {
                GameGrid.Instance.moveItem(itemPositions[2], positions[3]);
                itemPositions.Add(3, itemPositions[2]);
                itemPositions.Remove(2);
            }
            if (itemPositions.ContainsKey(1) && !itemPositions.ContainsKey(2)) {
                GameGrid.Instance.moveItem(itemPositions[1], positions[2]);
                itemPositions.Add(2, itemPositions[1]);
                itemPositions.Remove(1);
            }
        }

        protected override void onUpdateConnection(Vector3 neighbor) {
            refreshBarriers();
        }

        public void refreshBarriers() {
            gameObject.transform.Find(Direction.Up.ToString()).gameObject.SetActive(true);
            gameObject.transform.Find(Direction.Down.ToString()).gameObject.SetActive(true);
            gameObject.transform.Find(Direction.Left.ToString()).gameObject.SetActive(true);
            gameObject.transform.Find(Direction.Right.ToString()).gameObject.SetActive(true);

            foreach (Vector3 neighbor in connections.Keys) {
                refreshBarrier(neighbor);
            }
        }

        public void refreshBarrier(Vector3 neighbor) {
            Direction barrierToRemove = Direction.Right;
            GameGrid.Position neighborPosition = GameGrid.Instance.getNeighborPosition(position, neighbor);

            switch (neighborPosition) {
                case GameGrid.Position.Right:
                    switch (direction) {
                        case Direction.Up:
                            barrierToRemove = Direction.Right;
                            break;
                        case Direction.Down:
                            barrierToRemove = Direction.Left;
                            break;
                        case Direction.Right:
                            barrierToRemove = Direction.Up;
                            break;
                        case Direction.Left:
                            barrierToRemove = Direction.Down;
                            break;
                    }
                    break;
                case GameGrid.Position.Left:
                    switch (direction) {
                        case Direction.Up:
                            barrierToRemove = Direction.Left;
                            break;
                        case Direction.Down:
                            barrierToRemove = Direction.Right;
                            break;
                        case Direction.Right:
                            barrierToRemove = Direction.Down;
                            break;
                        case Direction.Left:
                            barrierToRemove = Direction.Up;
                            break;
                    }
                    break;
                case GameGrid.Position.Top:
                    switch (direction) {
                        case Direction.Up:
                            barrierToRemove = Direction.Up;
                            break;
                        case Direction.Down:
                            barrierToRemove = Direction.Down;
                            break;
                        case Direction.Right:
                            barrierToRemove = Direction.Left;
                            break;
                        case Direction.Left:
                            barrierToRemove = Direction.Right;
                            break;
                    }
                    break;
                case GameGrid.Position.Bottom:
                    switch (direction) {
                        case Direction.Up:
                            barrierToRemove = Direction.Down;
                            break;
                        case Direction.Down:
                            barrierToRemove = Direction.Up;
                            break;
                        case Direction.Right:
                            barrierToRemove = Direction.Right;
                            break;
                        case Direction.Left:
                            barrierToRemove = Direction.Left;
                            break;
                    }
                    break;
            }

            gameObject.transform.Find(barrierToRemove.ToString()).gameObject.SetActive(false);
        }
    }
}
