using Peque;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MachineInfo
{
    public Machine.Type type;
    public Machine.ExecutionType executionType;
    public string name;
    public string description;
    public int x = 1;
    public int y = 1;
    public int ticksBetweenExecution = 10;
    public GameObject prefab;
    public Texture icon;
    public ItemUI[] requiredItemsToProduce;
    public ItemUI[] itemsThatProduces;
    public ItemUI[] _storageLimits;
    public Dictionary<Item.Type, int> storageLimits = new Dictionary<Item.Type, int>();
    public int moneyThatGenerates = 0;
    public int price = 100;
}
[Serializable]
public class ItemUI
{
    public Item.Type type;
    public int quantity;
}