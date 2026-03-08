using UnityEngine;

namespace RPG.Control
{
    public class PatrolPath : MonoBehaviour
    {
        const float waypointGizmoRadius = 0.3f;

        /// <summary>
        /// 다음 웨이포인트의 인덱스를 반환, 마지막 웨이포인트에서 호출 시 첫 번째 인덱스 반환
        /// </summary>
        /// <param name="i">현재 웨이포인트 인덱스</param>
        /// <returns>다음 웨이포인트 인덱스</returns>
        public int GetNextIndex(int i)
        {
            return (i + 1) % transform.childCount;
        }

        /// <summary>
        /// 인덱스의 웨이포인트 위치를 반환
        /// </summary>
        /// <param name="i">웨이포인트 인덱스</param>
        /// <returns>해당 웨이포인트의 월드 좌표</returns>
        public Vector3 GetWayPoint(int i)
        {
            return transform.GetChild(i).position;
        }

        /// <summary>
        /// 웨이포인트와 연결 경로 기즈모로 시각화
        /// </summary>
        private void OnDrawGizmos()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Gizmos.DrawSphere(GetWayPoint(i), waypointGizmoRadius);
                Gizmos.DrawLine(GetWayPoint(i), GetWayPoint(GetNextIndex(i)));
            }
        }
    }
}