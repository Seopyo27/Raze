using UnityEngine;

[CreateAssetMenu(fileName = "Vision", menuName = "Vision", order = 0)]
public class Vision : ScriptableObject
{
    [SerializeField] private Vector3 boxSize;
    [SerializeField] private Vector3 boxOffset;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    /// <summary>
    /// 전방 박스 범위 내에 플레이어가 있는지 감지, 레이캐스트로 플레이어 사이에 시야가 차단되는지 추가로 검증
    /// </summary>
    /// <param name="myTransform">시야 판정 기준 트랜스폼</param>
    /// <returns>플레이어가 시야 내에 있고 장애물에 가려지지 않으면 true</returns>
    public bool CheckFrontalVision(Transform myTransform)
    {
        Vector3 boxCenter = myTransform.position + myTransform.TransformDirection(boxOffset);

        Collider[] hits = Physics.OverlapBox(
            boxCenter,
            boxSize / 2f,
            myTransform.rotation,
            playerLayer
        );

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            Vector3 origin    = myTransform.position + (Vector3.up * 1f);
            Vector3 targetPos = hit.transform.position + (Vector3.up * 1f);
            Vector3 direction = (targetPos - origin).normalized;
            float   distance  = Vector3.Distance(origin, targetPos);

            if (!Physics.Raycast(origin, direction, distance, obstacleLayer))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 시야 감지 박스를 Scene 뷰에 기즈모로 시각화합니다.
    /// </summary>
    /// <param name="myTransform">기즈모 기준 트랜스폼</param>
    public void DrawGizmos(Transform myTransform)
    {
        Vector3 boxCenter = myTransform.position + myTransform.TransformDirection(boxOffset);
        Gizmos.color  = Color.yellow;
        Gizmos.matrix = Matrix4x4.TRS(boxCenter, myTransform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}