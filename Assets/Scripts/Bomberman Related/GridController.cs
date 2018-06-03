using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Responsável por preparar / posicionar elementos do jogo na grid, como spawn dos bonecos, blocos e itens. Nome pode ser melhorado.
/// </summary>
public class GridController : MonoBehaviour {

    GridCalculator gc; 
    TileInfo[,] gridInfo; // Matrix que guarda os estados das tiles. ATM usado apenas pra geração dos blocos aleatórios

    /// <summary>
    /// Se verdadeiro, nenhum soft-block será criado e os bonecos já terão vários power-ups.
    /// </summary>
    public bool sandboxMode;

    /* Grid 101:
        * ^ Y+
        * |
        * |
        * 0 —————> X+
        */

    // Use this for initialization
    void Start() {
        //Debug.Log("Grid info \nCell Size: " + grid.cellSize
        //    + "\nCell gap: " + grid.cellGap
        //    //+ "\nCell "+grid.
        //    );

        gc = GridCalculator.Instance;

        Boneco player1 = Instantiate<Boneco>(Resources.Load<Boneco>("Prefabs/Boneco 1"));
        player1.transform.position = gc.centerPosition(player1.transform.position); // Ajusta o boneco pro centro da tile.
        player1.setup(sandboxMode);

        if (!sandboxMode) {
            generateBlocks();
        }
    }
	
	// Update is called once per frame
	void Update () {
        
    }

    #region Grid Setup
    /// <summary>
    /// Cria e posiciona os blocos destrutíveis no mapa. 
    /// </summary>
    void generateBlocks() {
        // Verifica se o mapa está corretamente posicionado
        Vector2 curPos = gc.centerPosition(new Vector2(0, 0));
        if (curPos != new Vector2(0, 0) || !gc.tileContent(new Vector2(-1, 0)).CompareTag("Border") ||
            !gc.tileContent(new Vector2(0, -1)).CompareTag("Border")) {
            Debug.Log("Deu ruim. Tabuleiro mal formado"); // PutaVida.exception
        }

        // Pegando tamanho da parte jogável do mapa
        int x = 1, y = 1;
        do {
            GameObject content = gc.tileContent(curPos + Vector2.up);
            if (content != null && content.CompareTag("Border")) {
                break;
            }
            curPos += Vector2.up;
            y++;
        } while (y < 100); // Limite arbitrário pra impedir loop infinito :P

        do {
            GameObject content = gc.tileContent(curPos + Vector2.right);
            if (content != null && content.CompareTag("Border")) {
                break;
            }
            curPos += Vector2.right;
            x++;
        } while (x < 100);

        // População da gridInfo
        gridInfo = new TileInfo[x, y];
        for (int i = 0; i < x; i++) {
            for (int j = 0; j < y; j++) {
                gridInfo[i, j] = new TileInfo(gc.centerPosition(new Vector2(i, j)));
                GameObject content = gc.tileContent(new Vector2(i, j));
                if (content != null) {
                    gridInfo[i, j].Block = content.tag;
                }
            }
        }

        // Setagem de spawn beta. 
        gridInfo[0, 0].Spawn = true;
        gridInfo[1, 0].Spawn = true;
        gridInfo[0, 1].Spawn = true;

        // Criação dos blocos destrutíveis aleatórios
        List<SoftBlock> rbs = new List<SoftBlock>();
        foreach (TileInfo t in gridInfo) {
            if (!t.Spawn && t.Block == "") {
                if (UnityEngine.Random.Range(0, 100) < 70) {
                    t.Block = "SoftBlock";
                    rbs.Add(Instantiate(Resources.Load<SoftBlock>("Prefabs/SoftBlock"), t.Center, Quaternion.identity));
                }
            }
        }
        randomizeItems(rbs);
    }

    /// <summary>
    /// Define aleatoriamente quais blocos terão quais items (também aleatórios).
    /// </summary>
    /// - Atenção (23/01/2018): Otimizar o loop pra não correr risco de "rng infinita" no rngBlock
    /// <param name="blocks"> Lista de blocos que estão no mapa. </param>
    void randomizeItems(List<SoftBlock> blocks) {

        List<Tuple<string, int>> itemList = new List<Tuple<string, int>> {
            new Tuple<string, int>("BombUp", 7),
            new Tuple<string, int>("FireUp", 5),
            new Tuple<string, int>("SpeedUp", 8),
            new Tuple<string, int>("Kick", 2)
            //new Tuple<string, int>("BombUp", 10)
        };

        do {
            int rngBlock = UnityEngine.Random.Range(0, blocks.Count);
            if (blocks[rngBlock].ItemName == "") {
                int rngItem = UnityEngine.Random.Range(0, itemList.Count);
                blocks[rngBlock].ItemName = itemList[rngItem].item1;
                itemList[rngItem].item2--;
                if (itemList[rngItem].item2 <= 0) {
                    itemList.RemoveAt(rngItem);
                }
            }
        } while (itemList.Count > 0);
    }
    #endregion

    /// <summary>
    /// Método para testes de raycast.
    /// </summary>
    /// <param name="origin">Ponto de origem do raycast </param>
    /// <param name="dir"> Direção do raycast </param>
    void rayTest(Vector2 origin, Vector2 dir) {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.right, 3f);
        foreach (RaycastHit2D hit in hits) {
            GameObject go = hit.collider.gameObject;

            Debug.Log("Dir: " + dir + " - Dist: " + Vector2.Distance(origin, gc.centerPosition(hit.point, dir)) +
                " - HitPoint: " + hit.point + " - Hit CellCenter: " + gc.centerPosition(hit.point, dir));
            Debug.Log(gameObject.tag);
        }
    }
}
