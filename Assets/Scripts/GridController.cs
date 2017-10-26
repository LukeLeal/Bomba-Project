using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe intermédio entre a UnityEngine.Grid e o funcionamento dela nesse jogo.
/// </summary>
public class GridController : Singleton<GridController> {

    public Grid grid; // Grid de jogo atual
    /* Grid 101:
     * ^ Y+
     * |
     * |
     * o —————> X+
     * 
     * */

    // Use this for initialization
    void Start() {
        Debug.Log("Grid info \nCell Size: " + grid.cellSize
            + "\nCell gap: " + grid.cellGap
            //+ "\nCell "+grid.
            );
        GameObject boneco = GameObject.FindWithTag("Player");
        boneco.transform.position = centerPosition(boneco.transform.position);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    // Centraliza o objeto na célula atual.
    public Vector3 centerPosition(Vector3 v3) {
        // WorldToCell pega a posição global e converte pra da célula
        // GetCellCenterWorld pega a posição da célula e retorna a posição global do centro dela.
        return grid.GetCellCenterWorld(grid.WorldToCell(v3));
    }

    // Verifica se há alguma colisão que impede o movimento pretendido
    // (Talvez seja melhor deixar isso no Boneco já que atm usa nada do gc)
    public bool possibleMove(float x, float y, Vector2 dir) {
        // Cria raycast a partir de (x,y), na direção dir com distância de uma tile
        if(Physics2D.Raycast(new Vector2(x, y), dir, 1).collider == null) {
            return true;
        }
        return false;
    }
}
