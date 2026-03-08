using UnityEngine;

namespace MP1.Combat
{
    public static class AttackRange
    {
        /// <summary>
        /// 공격 범위 박스 내의 콜라이더를 감지
        /// </summary>
        /// <param name="origin">공격 기준 트랜스폼 (위치 및 방향 기준)</param>
        /// <param name="offset">origin으로부터의 박스 중심 오프셋 (x: 좌우, y: 상하, z: 전후)</param>
        /// <param name="size">박스의 크기 (x: 너비, y: 높이, z: 깊이)</param>
        /// <param name="layerMask">감지할 레이어 마스크</param>
        /// <returns>박스 범위 내에 있는 콜라이더 배열</returns>
        public static Collider[] GetEnemiesInBox(Transform origin, Vector3 offset, Vector3 size, LayerMask layerMask)
        {
            Vector3 center = origin.position + (origin.forward * offset.z) + (origin.right * offset.x) + (origin.up * offset.y);
            Vector3 halfExtents = size * 0.5f;
            return Physics.OverlapBox(center, halfExtents, origin.rotation, layerMask);
        }

        /// <summary>
        /// 공격 범위 박스를 기즈모로 시각화
        /// </summary>
        /// <param name="origin">공격 기준 트랜스폼 (위치 및 방향 기준)</param>
        /// <param name="offset">origin으로부터의 박스 중심 오프셋 (x: 좌우, y: 상하, z: 전후)</param>
        /// <param name="size">박스의 크기 (x: 너비, y: 높이, z: 깊이)</param>
        /// <param name="color">기즈모 색상</param>
        public static void DrawAttackGizmos(Transform origin, Vector3 offset, Vector3 size, Color color)
        {
            Gizmos.color = color;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Vector3 center = origin.position + (origin.forward * offset.z) + (origin.right * offset.x) + (origin.up * offset.y);
            Gizmos.matrix = Matrix4x4.TRS(center, origin.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, size);
            Gizmos.matrix = oldMatrix;
        }
    }
}
