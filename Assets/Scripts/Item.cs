using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Peque
{
    public class Item
    {
        public enum Type
        {
            Coal = 1,
            IronOre = 2,
            Iron = 3,
        }

        public System.Guid id;
        public Vector3 parent;
        public Vector3 position;
        public Type type;
        public Transform transform;

        public ItemInfo info {
            get {
                return GameGrid.Instance.getItemInfo(type);
            }
        }
    }
}