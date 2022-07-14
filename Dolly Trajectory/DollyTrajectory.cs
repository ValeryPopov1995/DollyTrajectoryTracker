using Cinemachine;
using UnityEngine;

public class DollyTrajectory : MonoBehaviour
{
    public bool IsImpact { get; private set; }

    public CinemachineSmoothPath Path => path;

    [SerializeField] CinemachineSmoothPath path;
    [SerializeField] LineRenderer line;

    private Transform camera;

    private void Awake()
    {
        camera = Camera.main.transform;
    }

    /// <summary>
    /// Строит путь по дуге от А к Б, где середина дуги может отклоняться на заданный вектор, в локальных координатах
    /// </summary>
    /// <param name="startPosition">начальная точка траектории</param>
    /// <param name="endPosition">конечная точка траектории</param>
    /// <param name="middleLocalOffset">сдвиг средней точки траектории, от середины прямого пути между А и Б, в локальных координатах</param>
    /// <param name="updateVisibleTrajectory">обновить визульную составляющую траектории</param>
    public void UpdatePath(Vector3 startPosition, Vector3 endPosition, Vector3 middleLocalOffset = new Vector3(), bool updateVisibleTrajectory = true)
    {
        SetWaypointsCount(3);
        path.m_Looped = false;

        var middleStreightPoint = (endPosition - startPosition) / 2 + startPosition; // середина прямого пути в мировом пространстве

        var middleLocalOffsetResult = 
            camera.right * middleLocalOffset.x // кривизна дуги
            + Vector3.up * middleLocalOffset.y // высота середины дуги
            + Vector3.Cross(camera.right, Vector3.up) * middleLocalOffset.z; // смещение середины дуги в сторону начала или конца дуги

        path.m_Waypoints[0].position = startPosition;
        path.m_Waypoints[1].position =
            middleStreightPoint + middleLocalOffsetResult;
        path.m_Waypoints[2].position = endPosition;
        
        path.InvalidateDistanceCache();

        if (updateVisibleTrajectory)
            UpdateTrajectory();
    }

    /// <summary>
    /// Строит путь по дуге от А и обратно через Б, где четверть пути отклоняется на заданный вектор, в локальных координатах
    /// </summary>
    /// <param name="startEndPosition">начальная/конечная точка траектории</param>
    /// <param name="middlePosition">средняя точка пути, целевая точка попадания бумеранга</param>
    /// <param name="quarterLocalOffset">сдвиг точки четверти траектории от середины прямого пути между А и Б, в локальных координатах</param>
    /// /// <param name="updateVisibleTrajectory">обновить визульную составляющую траектории</param>
    public void UpdatePathBoomerang(Vector3 startEndPosition, Vector3 middlePosition, Vector3 quarterLocalOffset, bool updateVisibleTrajectory = true)
    {
        SetWaypointsCount(5);
        path.m_Looped = false;

        var middleStreightPoint = (middlePosition - startEndPosition) / 2 + startEndPosition; // середина прямого пути в мировом пространстве

        var firstQuarterLocalOffsetResult =
            camera.right * quarterLocalOffset.x // кривизна дуги
            + Vector3.up * quarterLocalOffset.y // высота дуги
            + Vector3.Cross(camera.right, Vector3.up) * quarterLocalOffset.z; // смещение четверти дуги в сторону А или Б

        var lastQuarterLocalOffsetResult = -quarterLocalOffset; // если первая четверть пути будет выше и правее от середины прямого пути, то 3/4 пути будет противоположно (ниже и левее)
        lastQuarterLocalOffsetResult.z *= -1; // но также ближе к Б, а не противоположно (дальше)

        path.m_Waypoints[0].position = startEndPosition;
        path.m_Waypoints[1].position = middleStreightPoint + firstQuarterLocalOffsetResult;
        path.m_Waypoints[2].position = middlePosition;
        path.m_Waypoints[3].position = middleStreightPoint + lastQuarterLocalOffsetResult;
        path.m_Waypoints[4].position = startEndPosition;

        path.InvalidateDistanceCache();

        if (updateVisibleTrajectory)
            UpdateTrajectory();
    }

    public void UpdateTrajectory(bool checkTrajectoryVisible = true, bool checkImpacts = true)
    {
        if (checkTrajectoryVisible && !line.enabled) ShowTrajectory(true);
        if (!line.enabled) return;

        bool triggered = false;
        Vector3 lastPoint = new Vector3();
        Vector3[] points = new Vector3[line.positionCount];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = path.EvaluatePositionAtUnit((float)i / points.Length * path.PathLength, CinemachinePathBase.PositionUnits.Distance);

            if (checkImpacts && i > 0 && Physics.Linecast(lastPoint, points[i]) && !triggered)
                triggered = true;

            lastPoint = points[i];
        }

        line.SetPositions(points);

        if (checkImpacts)
            IsImpact = triggered;
    }

    public void ShowTrajectory(bool enabled)
    {
        line.enabled = enabled;
    }

    private void SetWaypointsCount(int count)
    {
        if (path.m_Waypoints.Length == count) return;

        path.m_Waypoints = new CinemachineSmoothPath.Waypoint[count];
    }
}
