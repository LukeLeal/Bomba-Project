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
    */

    /* Constantes de Order in Layer. Nomes beta.
    (13/12/17) Order in Layer nesse projeto, além da função padrão de ordem de aparência dos sprites na sorting layer,
    támbém é usado no tratamento de certas colisões. Provavelmente tem um jeito mais "elegante",
    mas entre as coisas que eu consegui pensar e testar, essa foi a que achei melhor. Então... ¯\_(ツ)_/¯ 
    Se você, caro leitor, tiver uma ideia melhor, please let me know.
    */
    public const int LBackground = 0;
    public const int LObjects = 1;
    public const int LBonecos = 2;
    public const int LAbove = 3;
    public const int LFlying = 4;
    public const int LTop = 5;

    // Use this for initialization
    void Start() {
        Debug.Log("Grid info \nCell Size: " + grid.cellSize
            + "\nCell gap: " + grid.cellGap
            //+ "\nCell "+grid.
            );
        GameObject boneco = GameObject.FindWithTag("Player");
        boneco.transform.position = centerPosition(boneco.transform.position); // Ajusta o boneco pro centro da tile.
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

    // Retorna o GO presente na camada de objects da tile.
    public GameObject tileMainContent(Vector3 v) {
        v = centerPosition(v); // Garante centralização

        // ATENÇÃO (11/12/2017): Deve dar pra melhorar essa parte do raycast / if. 
        RaycastHit2D[] hits = Physics2D.RaycastAll(v, Vector2.up, 0.4f); // Faz um pequeno rc até o limite da tile.
        foreach(RaycastHit2D hit in hits) {
            if (hit.collider.gameObject.GetComponent<Renderer>().sortingOrder == LObjects &&
                centerPosition(hit.point) == v) {

                return hit.collider.gameObject; 
            }
        }
        return null; // Tile vazia.
    }

}
