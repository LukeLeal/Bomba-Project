using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBlocks : MonoBehaviour, IZOrder {

    int zOrder;

    public int ZOrder {
        get { return zOrder; }
    }

    private void Awake() {
        zOrder = GetComponent<Renderer>().sortingOrder;
    }

    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
