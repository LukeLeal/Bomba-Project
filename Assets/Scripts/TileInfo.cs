using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInfo {

    bool spawn;
    string block;
    Vector2 center;

    public TileInfo(Vector2 v) {
        center = v;
        spawn = false;
        block = "";
    }

    public bool Spawn {
        get {
            return spawn;
        }

        set {
            spawn = value;
        }
    }

    public string Block {
        get {
            return block;
        }

        set {
            block = value;
        }
    }

    public Vector2 Center {
        get {
            return center;
        }
        set {
            center = value;
        }
    }
}
