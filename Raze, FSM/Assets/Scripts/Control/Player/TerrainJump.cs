using UnityEngine;
using UnityEngine.AI;

namespace MP1.Control
{
    public class TerrainJump : MonoBehaviour
    {
        [Header("참조 설정")]
        [SerializeField] private Transform _player; // 실제 캐릭터 모델 또는 피벗
        
        [Header("지형 레이어")]
        [SerializeField] private LayerMask _groundLayer; // 판정을 위한 지형 레이어

        [Header("클라임 점프 판정")]
        [SerializeField] private float climbJumpColGap = 3f; // 클라임 점프 선 세로 간격
        [SerializeField] private float climbJumpRowGap = 1f; // 클라임 점프 선 가로 간격
        [SerializeField] private float climbJumpForwardOffset = 1.5f; // 클라임 점프 판정 거리
        [SerializeField] private float climbJumpUpOffset = 3f; // 클라임 점프 시작 높이선 
        [SerializeField] private float climbJumpMinHeight = 2.0f; // 클라임 점프 최소가능 높이
        [SerializeField] private float climbJumpMaxHeight = 5.0f; // 클라임 점프 최대가능 높이

        [Header("절벽 판정")]
        [SerializeField] private float CliffCheckForwardOffset = 1.0f;   // 절벽 판정 거리
        [SerializeField] private float cliffCheckUpOffset = 1.0f; // 절벽 판정 시작 높이
        [SerializeField] private float cliffCheckDepthOffset = 2.0f; // 절벽 판정 깊이

        [Header("벽 판정")]
        [SerializeField] private Vector3 boxSize = new Vector3(0.6f, 1.8f, 0.1f); // 캐릭터 크기에 맞춘 박스
        [SerializeField] private float detectionDistance = 1.0f; // 얼마나 앞에서 벽을 인식할지
        [SerializeField] private Vector3 boxOffset = new Vector3(0, 1.0f, 0.5f); // 박스 중심점 (발바닥 기준 위로, 앞으로)

        [Header("리프 점프 판정")]
        [SerializeField] private float leapJumpCheckForwardOffset = 3.0f;  // 리프 점프 판정 거리
        [SerializeField] private float leapJumpCheckheightTolerance = 0.7f;  // 리프 점프 허용 높이 차이 (플레이어 피벗, 발 기준)
        [SerializeField] private float leapJumpCheckLineGap = 0.6f; // 리프 점프 선 간격
        [SerializeField] private float leapJumpCheckUpOffset = 3f; // 리프 점프 선 시작 높이
        [SerializeField] private float leapJumpCheckDepth = 0.6f; // 리프 점프 선 깊이

        [Header("기즈모 설정")]
        [SerializeField] private bool viewIsCliffAheadGizmo;     
        [SerializeField] private bool viewIsWallAheadGizmo;
        [SerializeField] private bool viewGetBestLeapJumpPositionGizmo;
        [SerializeField] private bool viewGetBestClimbJumpPositionGizmo;
        [SerializeField] private bool viewTerrainJumpTargetPositionGizmo;
        

        private Vector3 debugTargetPos; 
        private RaycastHit wallHit;
        
        /// <summary>
        /// 전방에 벽이 있을 때 올라갈 수 있는 클라임 점프 목표 위치를 반환
        /// </summary>
        /// <returns>클라임 점프 목표 월드 좌표. 유효하지 않으면 Vector3.zero</returns>
        public Vector3 GetClimbJumpPosition()
        {
            if (!IsWallAhead()) return Vector3.zero;
            Vector3 best = GetBestClimbJumpPosition();
            debugTargetPos = best;
            return best;
        }

        /// <summary>
        /// 전방에 절벽이 있고 벽이 없을 때 리프 점프 목표 위치를 반환
        /// </summary>
        /// <returns>리프 점프 목표 월드 좌표. 유효하지 않으면 Vector3.zero</returns>
        public Vector3 GetLeapJumpPosition()
        {
            //벽 겁사
            if (IsWallAhead() || !IsCliffAhead()) return Vector3.zero;
            Vector3 best = GetBestLeapJumpPosition();
            debugTargetPos = best;
            return best;
        }

        /// <summary>
        /// 전방 격자 스캔으로 클라임 점프 가능한 착지 지점 중 가장 가까운 NavMesh 위치 반환
        /// </summary>
        /// <returns>가장 가까운 클라임 점프 목표 위치. 없으면 Vector3.zero</returns>
        private Vector3 GetBestClimbJumpPosition()
        {
            Vector3 bestTarget = Vector3.zero;
            float closestDistance = float.MaxValue;

            Vector3 originCenter = transform.position + Vector3.up * climbJumpUpOffset + _player.forward * climbJumpForwardOffset;

            // 3 * 2 격자 스캔
            for (int x = -1; x <= 1; x++)
            {
                for (int z = 0; z <= 1; z++)
                {
                    // 1. 각 레이 시작 지점 계산
                    Vector3 scanOffset = (transform.right * x * (climbJumpRowGap / 2f)) + (_player.forward * z * (climbJumpColGap / 2f));
                    Vector3 rayStartPosition = originCenter + scanOffset;

                    // 2. 시작 지점에서 위에서 아래로 레이를 쏴서 바닥 충돌 확인
                    if (!Physics.Raycast(rayStartPosition, Vector3.down, out RaycastHit hit, climbJumpUpOffset, _groundLayer)) continue;

                    // 3. 이동 가능한 NavMesh 위인지 확인
                    NavMeshHit navHit;
                    if (!NavMesh.SamplePosition(hit.point, out navHit, 0.5f, NavMesh.AllAreas)) continue;

                    // 4. 캐릭터가 점프할 수 있는 높이 인지 확인
                    float heightDiff = navHit.position.y - transform.position.y;
                    if (!(heightDiff >= climbJumpMinHeight && heightDiff <= climbJumpMaxHeight)) continue;

                    // 5. 가장 가까운 곳인지 확인
                    float dist = Vector3.Distance(transform.position, navHit.position);
                    if (dist >= closestDistance) continue;

                    closestDistance = dist;
                    bestTarget = navHit.position;
                    debugTargetPos = bestTarget;
                }
            }

            return bestTarget;
        }


        /// <summary>
        /// 전방을 스캔하고 리프 점프 가능한 착지 지점 중 가장 가까운 NavMesh 위치 반환
        /// </summary>
        /// <returns>가장 가까운 리프 점프 목표 위치. 없으면 Vector3.zero</returns>
        private Vector3 GetBestLeapJumpPosition()
        {
            Vector3 bestTarget = Vector3.zero;
            float closestDist = float.MaxValue;

            //전방 3곳 스캔
            for (int i = -1; i <= 1; i++)
            {
                // 1. 각 레이 시작 지점 계산
                Vector3 horizontalOffset = transform.right * (i * leapJumpCheckLineGap * 0.5f);
                Vector3 leapgroundLayStartPosition = transform.position + (transform.forward * leapJumpCheckForwardOffset) + horizontalOffset + (Vector3.up * leapJumpCheckUpOffset);

                // 2. 시작 지점에서 위에서 아래로 레이를 쏴서 바닥 충돌 확인
                if (!Physics.Raycast(leapgroundLayStartPosition, Vector3.down, out RaycastHit hit, leapJumpCheckDepth, _groundLayer)) continue;

                // 3. 이동 가능한 NavMesh 위인지 확인
                NavMeshHit navHit;
                if (!NavMesh.SamplePosition(hit.point, out navHit, 0.5f, NavMesh.AllAreas)) continue;

                // 4. 캐릭터가 점프할 수 있는 높이 인지 확인
                float heightDiff = Mathf.Abs(navHit.position.y - transform.position.y);

                if (heightDiff <= leapJumpCheckheightTolerance)
                {
                    float d = Vector3.Distance(transform.position, navHit.position);
                    if (d < closestDist)
                    {
                        closestDist = d;
                        bestTarget = navHit.position;
                    }
                }
            }

            return bestTarget;
        }

        /// <summary>
        /// 전방 일정 거리에 바닥이 없는지 레이캐스트로 절벽 여부 판정
        /// </summary>
        /// <returns>전방에 절벽이 있으면 true</returns>
        private bool IsCliffAhead()
        {
            Vector3 voidRayStartPostion = transform.position + (transform.forward * CliffCheckForwardOffset) + (Vector3.up * cliffCheckUpOffset);
            return !Physics.Raycast(voidRayStartPostion, Vector3.down, out RaycastHit edgeHit, cliffCheckDepthOffset, _groundLayer);
        }

        /// <summary>
        /// 전방에 벽이 있는지 판정합니다.
        /// </summary>
        /// <returns>전방에 벽이 있으면 true</returns>
        public bool IsWallAhead()
        {
            Vector3 worldCenter = transform.TransformPoint(boxOffset);

            return Physics.BoxCast(worldCenter, boxSize / 2, transform.forward, 
                                   out wallHit, transform.rotation, detectionDistance, _groundLayer);
        }

        /// <summary>
        /// 벽 판정 범위 시각화
        /// </summary>
        private void DrawIsWallAheadGizmo()
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;

            Gizmos.matrix = transform.localToWorldMatrix;
            Vector3 centerOfVolume = boxOffset + (Vector3.forward * (detectionDistance / 2f));
            Vector3 totalBoxSize = new Vector3(boxSize.x, boxSize.y, detectionDistance);

            Gizmos.color = new Color(0, 1, 1, 0.2f);
            Gizmos.DrawCube(centerOfVolume, totalBoxSize);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(centerOfVolume, totalBoxSize);

            Gizmos.matrix = oldMatrix;
        }

        /// <summary>
        /// 절벽 판정 레이 시각화
        /// </summary>
        private void DrawIsCliffAheadGizmo()
        {
            Gizmos.color = Color.red;
            Vector3 edgeCheckOrigin = transform.position + (transform.forward * CliffCheckForwardOffset) + (Vector3.up * cliffCheckUpOffset);
            Gizmos.DrawLine(edgeCheckOrigin, edgeCheckOrigin + Vector3.down * cliffCheckDepthOffset);
            Gizmos.DrawSphere(edgeCheckOrigin, 0.05f); // 시작점 표시
        }

        /// <summary>
        /// 리프 점프 스캔 레이 시각화
        /// </summary>
        private void DrawGetBestLeapJumpPositionGizmo()
        {        
            Gizmos.color = Color.yellow;
            for (int i = -1; i <= 1; i++)
            {
                Vector3 horizontalOffset = transform.right * (i * leapJumpCheckLineGap * 0.5f);
                Vector3 scanStart = transform.position + (transform.forward * leapJumpCheckForwardOffset) + horizontalOffset + (Vector3.up * leapJumpCheckUpOffset);
                Gizmos.DrawLine(scanStart, scanStart + Vector3.down * leapJumpCheckDepth);
                Gizmos.DrawSphere(scanStart, 0.03f); 
            }
        }

        /// <summary>
        /// 클라임 점프 스캔 레이 & 점프 가능 높이 범위 시각화
        /// </summary>
        private void DrawGetBestClimbJumpPositionGizmo()
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;

            float rangeHeight = climbJumpMaxHeight - climbJumpMinHeight;
            float rangeCenterY = climbJumpMinHeight + (rangeHeight / 2f);

            Vector3 rangeCenter = new Vector3(0, rangeCenterY, 0.5f); 
            Vector3 boxSize = new Vector3(climbJumpRowGap, rangeHeight, 0.1f);

            Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
            Gizmos.DrawCube(rangeCenter, boxSize);
            Gizmos.color = Color.orange;
            Gizmos.DrawWireCube(rangeCenter, boxSize);

            Vector3 localOriginCenter = new Vector3(0, climbJumpUpOffset, climbJumpForwardOffset);
            Gizmos.color = Color.yellow;

            for (int x = -1; x <= 1; x++)
            {
                for (int z = 0; z <= 1; z++)
                {
                    Vector3 scanOffset = new Vector3(x * (climbJumpRowGap / 2f), 0, z * (climbJumpColGap / 2f));
                    Vector3 rayStart = localOriginCenter + scanOffset;
                    Vector3 rayEnd = rayStart + Vector3.down * (climbJumpUpOffset + 1f);

                    Gizmos.DrawLine(rayStart, rayEnd);
                    Gizmos.DrawSphere(rayStart, 0.05f);
                }
            }

            Gizmos.matrix = oldMatrix;
        }

        /// <summary>
        /// 점프 목표 위치 시각화
        /// </summary>
        private void DrawTerrainJumpTargetPositionGizmo()
        {
            if (debugTargetPos != Vector3.zero)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(debugTargetPos, 0.3f);
                Gizmos.DrawLine(transform.position + Vector3.up * 1f, debugTargetPos);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (viewIsWallAheadGizmo) DrawIsWallAheadGizmo();
            if (viewIsCliffAheadGizmo) DrawIsCliffAheadGizmo();
            if (viewGetBestLeapJumpPositionGizmo) DrawGetBestLeapJumpPositionGizmo();
            if (viewTerrainJumpTargetPositionGizmo) DrawTerrainJumpTargetPositionGizmo();
            if (viewGetBestClimbJumpPositionGizmo) DrawGetBestClimbJumpPositionGizmo();
        }
    }
}