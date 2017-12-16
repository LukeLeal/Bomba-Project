using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe intermédio entre a UnityEngine.Grid e o funcionamento dela nesse jogo.
/// </summary>
public class GridController : Singleton<GridController> {

    public Grid grid; // Grid de jogo atual
    TileInfo[,] gridInfo; // Matrix que guarda os estados das tiles. ATM usado apenas pra geração dos blocos aleatórios
    public bool randomBlocks;

/* Grid 101:
    * ^ Y+
    * |
    * |
    * o —————> X+
    * 
    */

    /* Constantes de Order in Layer. Nomes beta.
    (13/12/17) Order in Layer nesse projeto, além da função padrão de ordem de aparência dos sprites na sorting layer,
    támbém é usado no tratamento de certas colisões e raycasts. Provavelmente tem um jeito mais "elegante",
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
        if (randomBlocks) {
            generateBlocks();
        }
        GameObject boneco = GameObject.FindWithTag("Player");
        boneco.transform.position = centerPosition(boneco.transform.position); // Ajusta o boneco pro centro da tile.
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void generateBlocks() {
        // Verifica se o mapa está corretamente posicionado
        Vector2 curPos = centerPosition(new Vector2(0, 0));
        if(curPos != new Vector2(0, 0) || tileMainContent(new Vector2(-1, 0)).tag != "Border" ||
            tileMainContent(new Vector2(0, -1)).tag != "Border") {
            Debug.Log("Deu ruim. Tabuleiro mal formado");
        }

        // Pegando tamanho da parte jogável do mapa
        int x = 1, y = 1;
        do {
            GameObject go = tileMainContent(curPos + Vector2.up);
            if(go != null) {
                if(go.tag == "Border") {
                    break;
                }
            }
            curPos += Vector2.up;
            y++;
        } while (y < 100); // Limite pra impedir loop infinito :P

        do {
            GameObject go = tileMainContent(curPos + Vector2.right);
            if (go != null) {
                if (go.tag == "Border") {
                    break;
                }
            }
            curPos += Vector2.right;
            x++;
        } while (x < 100);

        // População da gridInfo
        gridInfo = new TileInfo[x, y];
        for(int i = 0; i < x; i++) {
            for(int j = 0; j < y; j++) {
                gridInfo[i, j] = new TileInfo(centerPosition(new Vector2(i, j)));
                GameObject go = tileMainContent(new Vector2(i, j));
                if (go != null) {
                    gridInfo[i, j].Block = go.tag;
                }
            }
        }

        // Setagem de spawn beta. 
        gridInfo[0, 0].Spawn = true;
        gridInfo[1, 0].Spawn = true;
        gridInfo[0, 1].Spawn = true;

        // Criação dos blocos destrutíveis aleatórios
        foreach (TileInfo t in gridInfo) {
            if (!t.Spawn && t.Block == "") {
                if (Random.Range(0, 100) < 70) {
                    Instantiate(Resources.Load<RegularBlock>("Prefabs/RegularBlock"), t.Center, Quaternion.identity);
                }
            }
        }
    }

    // Centraliza o objeto na célula atual.
    public Vector2 centerPosition(Vector2 v) {
        // WorldToCell pega a posição global e converte pra da célula
        // GetCellCenterWorld pega a posição da célula e retorna a posição global do centro dela.
        return grid.GetCellCenterWorld(grid.WorldToCell(v));
    }

    // Retorna o GO presente na camada de objects da tile.
    public GameObject tileMainContent(Vector2 v) {
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
