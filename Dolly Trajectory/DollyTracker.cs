using System.Collections;
using Cinemachine;
using UnityEngine;

public class DollyTracker : MonoBehaviour
{
    /// <summary>
    /// Трекер находиться на пути движения
    /// </summary>
    public bool IsOnTrack => trackingCoroutine != null;

    [Tooltip("Базовый путь для движения трекера, в аргументах метода запуска трекера можно задать другой")]
    [SerializeField] CinemachineSmoothPath path;

    private Coroutine trackingCoroutine;
    private GameObject tempPathGameobject;

    /// <summary>
    /// Запускает корутину движения трекера по траектории с заданной скоростью
    /// </summary>
    /// <param name="createPathCopy">создает копию пути, чтобы во время движения трекера его путь не менялся во времени</param>
    /// <param name="path">путь движения трекера, если null, то использует путь, указанный в своих настройках</param>
    /// <param name="moveSpeed">скорость движения трекера</param>
    /// <param name="usePathOrientation">вращать трекер по скрученности указанного пути, задается в CinemachineSmoothPath > Waypoints > Roll</param>
    public void StartTracking(CinemachineSmoothPath path = null, float moveSpeed = 1, bool createPathCopy = true, bool usePathOrientation = true)
    {
        if (trackingCoroutine != null)
        {
            StopAllCoroutines();
            trackingCoroutine = null;

            if (tempPathGameobject)
                Destroy(tempPathGameobject);
        }
        trackingCoroutine = StartCoroutine(TrackingCoroutine(path ? path : this.path, moveSpeed, createPathCopy, usePathOrientation));
    }

    private IEnumerator TrackingCoroutine(CinemachineSmoothPath path, float moveSpeed, bool createPathCopy, bool usePathOrientation)
    {
        CinemachineSmoothPath tempPath = createPathCopy ? Instantiate(path) : path;
        if (createPathCopy) tempPathGameobject = tempPath.gameObject;

        float pathByFixedSpeed = moveSpeed * Time.fixedDeltaTime;
        float trackCurrentPosition = 0;

        while (trackCurrentPosition < tempPath.PathLength)
        {
            yield return new WaitForFixedUpdate();
            trackCurrentPosition += pathByFixedSpeed;

            transform.position = tempPath.EvaluatePositionAtUnit(trackCurrentPosition, CinemachinePathBase.PositionUnits.Distance);
            if (usePathOrientation)
                transform.rotation = tempPath.EvaluateOrientationAtUnit(trackCurrentPosition, CinemachinePathBase.PositionUnits.Distance);
        }

        if (createPathCopy)
            Destroy(tempPath.gameObject);

        yield return new WaitForFixedUpdate();

        trackingCoroutine = null;
    }
}
