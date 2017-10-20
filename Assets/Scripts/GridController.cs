using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : Singleton<GridController> {

    public Grid grid;

    // Use this for initialization
    void Start() {
        Debug.Log("Grid info \nCell Size: " + grid.cellSize
            + "\nCell gap: " + grid.cellGap
            //+ "\nCell "+grid.
            );
        GameObject boneco = GameObject.FindWithTag("Player");
        boneco.transform.position = grid.GetCellCenterWorld(grid.WorldToCell(boneco.transform.position));
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
