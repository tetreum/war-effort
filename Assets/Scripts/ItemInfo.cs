using System;
using UnityEngine;

namespace Peque
{
    [Serializable]
    public class ItemInfo
    {
        public string name;
        public string description;
        public GameObject prefab;
        public Item.Type type;
    }
}