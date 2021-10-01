using UnityEngine;
using System.Collections;
using System;

public class Session
{
    public const int PointsPerMeter = 1;
    public const int PointsPerEgg = 100;
    public const int PointsPerMedallionPiece = 500;
    public const int PointsPerWholeMedallion = 3000;

    public bool alive = true;
    public float startTime;
    public float endTime;
    public int pointsCollected;
    public float currentDistanceTravelled;
    public float totalDistanceTravelled;
    public int eggsCount;
    public int medallionPieceCount;
    public int medallionCount;

    public Session() {
        startTime = Time.time;
        endTime = startTime;
    }

    public static Session current = null;

    public static void StartNew() {
        current = new Session();
    }

    public static void EndCurrent() {
        current.endTime = Time.time;
    }

    public static void ContinueCurrent()
    {
        current.startTime += (Time.time - current.endTime);
        current.endTime = 0;
        current.totalDistanceTravelled += current.currentDistanceTravelled;
        current.currentDistanceTravelled = 0;
        current.alive = true;
    }

    public static int Points {
        get { return current.pointsCollected + Mathf.RoundToInt(DistanceTravelled * PointsPerMeter); }
    }

    public static int EggCount {
        get { return current.eggsCount; }
    }

    public static int MedallionPieceCount {
        get { return current.medallionPieceCount; }
    }

    public static int MedallionCount {
        get { return current.medallionCount; }
    }

    public static float DistanceTravelled {
        get { return current.totalDistanceTravelled + current.currentDistanceTravelled; }
    }

    public static bool IsAlive {
        get { return current.alive; }
    }

    public static float Elapsed {
        get { return Time.time - current.startTime; }
    }

    public static float Length {
        get { return current.endTime - current.startTime; }
    }

    public static bool KillPlayer()
    {
        bool prevState = current.alive;
        current.alive = false;
        return prevState;
    }

    public static void MedallionPieceCollected(bool assignPoints = true)
    {
        if(assignPoints)
            current.pointsCollected += PointsPerMedallionPiece;

        if(++current.medallionPieceCount == 5)
            current.medallionPieceCount = 0;
    }

    public static void FullMedallionCollected()
    {
        current.pointsCollected += PointsPerWholeMedallion;
        current.medallionCount++;
    }

    public static void EggCollected()
    {
        current.pointsCollected += PointsPerEgg;
        current.eggsCount++;
    }

    public static void UpdateDistanceTravelled(float distance)
    {
        current.currentDistanceTravelled = distance;
    }
}
