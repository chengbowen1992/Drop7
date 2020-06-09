using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lesson1
{
    public class DropNode
    {
        public int Value { get; private set; } = 0;
        public Vector2Int Position { get; private set; } = Vector2Int.zero;

        private DropNode()
        {
        }

        public DropNode(Vector2Int pos, int val)
        {
            Position = pos;
            Value = val;
        }

        public void UpdatePosition(Vector2Int pos)
        {
            Position = pos;
        }

        public void UpdateVal(int val)
        {
            Value = val;
        }

        public override string ToString()
        {
            return $"DropNode:[{Position.x},{Position.y}] = {Value}";
        }
    }
}