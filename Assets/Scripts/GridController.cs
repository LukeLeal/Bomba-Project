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
    // Pode ou não estar depreciado -q
    public bool possibleMove(float x, float y, Vector2 dir) {
        // Cria raycast a partir de (x,y), na direção dir com distância de uma tile
        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(x, y), dir, 1);
        Debug.Log(x + ", " + y);
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.gameObject.tag == "Bomb" && centerPosition(hit.point) == new Vector3(x, y)) {
                // Atravessa colisão apenas se for uma bomba e estiver "dentro" dela.
                continue;
            } else {
                // Qualquer outra colisão, não pode.
                return false;
            }
        }
        return true;
    }

    // Verifica se há alguma colisão que impede o movimento pretendido.
    // Para uso de GameObjects que possuem colliders.
    public bool possibleMove(GameObject go, Vector2 dir) {
        // Cria raycast a partir de (x,y), na direção dir com distância de uma tile
        float x = go.transform.position.x;
        float y = go.transform.position.y;
        go.GetComponent<Collider2D>();
        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(x, y), dir, 1);
        // Debug.Log(x + ", " + y);
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.gameObject.tag == "Bomb" && centerPosition(hit.point) == new Vector3(x, y)) {
                // Atravessa colisão apenas se for uma bomba e estiver "dentro" dela.
                continue;
            } else if (hit.collider.gameObject == go) {
                continue;
            } else { 
                // Qualquer outra colisão, não pode.
                return false;
            }
        }
        return true;
    }

    public GameObject[] tileContents(Vector3 v) {
        // Soon
        return null;
    }

    //// Verifica se há alguma colisão que impede o movimento pretendido
    //// (Talvez seja melhor deixar isso no Boneco já que atm usa nada do gc)
    //public bool possibleMove(float x, float y, Vector2 dir) {
    //    // Cria raycast a partir de (x,y), na direção dir com distância de uma tile
    //    if (Physics2D.Raycast(new Vector2(x, y), dir, 1).collider == null) {
    //        return true;
    //    }
    //    return false;
    //}
}
