using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SlotGame {
    public static class CustomUtility 
    {
        public static Rect ToRect(this Bounds bounds) {
            return new Rect(bounds.min, bounds.size);
        }
        public static Vector2 InverseScale(this Vector2 origin, Vector2 other) {
            return new Vector2(origin.x / other.x, origin.y / other.y);
        }
        /// <summary>
        /// Rect를 특정 사이즈로 균등하게 자른 후 해당 위치의 Rect를 가져옵니다.
        /// 시작 위치(0,0)는 왼쪽 아래 기준
        /// </summary>
        public static Rect Split(this Rect rect, int splitX, int splitY, int x, int y) {
            Vector2 rectSize = InverseScale(rect.size, new Vector2(splitX, splitY));
            Vector2 firstPos = (Vector2)rect.min + Vector2.Scale(rectSize, new Vector2(x, y));
            return new Rect(firstPos, rectSize);
        }
    }
}
