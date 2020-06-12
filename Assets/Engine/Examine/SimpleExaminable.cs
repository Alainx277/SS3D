﻿using UnityEngine;

namespace SS3D.Engine.Examine
{
    public class SimpleExaminable : MonoBehaviour, IExaminable
    {
        [TextArea(5, 15)]
        public string Text;
        public float MaxDistance;

        public bool CanExamine(GameObject examinator)
        {
            if (MaxDistance <= 0)
            {
                return true;
            }
            
            Vector3 sourcePosition = examinator.transform.position;
            var position = transform.position;
            if (Vector2.Distance(new Vector2(sourcePosition.x, sourcePosition.z), new Vector2(position.x, position.z)) > MaxDistance)
            {
                return false;
            }

            return true;
        }

        public string GetDescription(GameObject examinator)
        {
            return Text;
        }
    }
}