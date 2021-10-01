using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MedallionSpawnPoint : MonoBehaviour
{
    void Start() {
		Game.that.medallionSpawnPoints.Add(this);
    }

    void OnDestroy() {
        Game.that.medallionSpawnPoints.Remove(this);
    }
}
