using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class SharedSounds : MonoBehaviour
{
    private static SharedSounds _that;
    public static SharedSounds that
    {
        get
        {
            if (_that == null)
            {
                var prefab = Resources.Load<GameObject>("SharedSounds");
                var go = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                DontDestroyOnLoad(go);
                _that = go.GetComponent<SharedSounds>();
            }

            return _that;
        }
    }

    public Sound2D _boing;
    public Sound2D _button;
    public Sound2D _clank01;
    public Sound2D _clank02;
    public Sound2D _clank03;
    public Sound2D _clank04;
    public Sound2D _death;
    public Sound2D _eggCollected;
    public Sound2D _falling;
    public Sound2D _landing;
    public Sound2D _medallionCollected;
    public Sound2D _medallionPieceCollected;
    public Sound2D _pop;
    public Sound2D _snap;
    public Sound2D _takeoff;
    public Sound2D _treeHit;

    public static Sound2D boing { get { return that._boing; } }
    public static Sound2D button { get { return that._button; } }
    public static Sound2D clank01 { get { return that._clank01; } }
    public static Sound2D clank02 { get { return that._clank02; } }
    public static Sound2D clank03 { get { return that._clank03; } }
    public static Sound2D clank04 { get { return that._clank04; } }
    public static Sound2D death { get { return that._death; } }
    public static Sound2D medallionCollected { get { return that._medallionCollected; } }
    public static Sound2D medallionPieceCollected { get { return that._medallionPieceCollected; } }
    public static Sound2D eggCollected { get { return that._eggCollected; } }
    public static Sound2D falling { get { return that._falling; } }
    public static Sound2D landing { get { return that._landing; } }
    public static Sound2D snap { get { return that._snap; } }
    public static Sound2D pop { get { return that._pop; } }
    public static Sound2D takeoff { get { return that._takeoff; } }
    public static Sound2D treeHit { get { return that._treeHit; } }

    private void Awake()
    {

    }
}
