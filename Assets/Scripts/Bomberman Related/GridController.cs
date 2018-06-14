using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Responsável por preparar / posicionar elementos do jogo na grid, como spawn dos bonecos, blocos e itens. Nome pode ser melhorado.
/// </summary>
public class GridController : MonoBehaviour {

    GridCalculator gc; 
    TileInfo[,] boardInfo; // Matrix que guarda os estados das tiles. ATM usado apenas pra geração dos blocos aleatórios

    /// <summary>
    /// Se verdadeiro, nenhum soft-block será criado e os bonecos já terão vários power-ups.
    /// </summary>
    public bool sandboxMode;

    /// <summary>
    /// Número de jogadores na partida (Max 2 atm). 
    /// </summary>
    public int playersAmount;

    /* Board 101:
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

        setupBoard();
    }
	
	// Update is called once per frame
	void Update () {
        
    }

    #region Board Setup
    /// <summary>
    /// Prepara o tabuleiro. Posiciona os blocos destrutíveis no mapa e os jogadores.
    /// </summary>
    void setupBoard() {

        // Verifica se o mapa está corretamente posicionado
        Vector2 curPos = gc.centerPosition(new Vector2(0, 0));
        if (curPos != new Vector2(0, 0) || !gc.tileContent(new Vector2(-1, 0)).CompareTag("Border") ||
            !gc.tileContent(new Vector2(0, -1)).CompareTag("Border")) {
            Debug.Log("Deu ruim. Tabuleiro mal formado"); // PutaVida.exception
            return;
        }

        // Pegando tamanho da parte jogável do mapa
        int xSize = 1, ySize = 1;
        do {
            GameObject content = gc.tileContent(curPos + Vector2.up);
            if (content != null && content.CompareTag("Border")) {
                break;
            }
            curPos += Vector2.up;
            ySize++;
        } while (ySize < 100); // Limite arbitrário pra impedir loop infinito :P

        do {
            GameObject content = gc.tileContent(curPos + Vector2.right);
            if (content != null && content.CompareTag("Border")) {
                break;
            }
            curPos += Vector2.right;
            xSize++;
        } while (xSize < 100);

        // População da boardInfo
        boardInfo = new TileInfo[xSize, ySize];
        for (int i = 0; i < xSize; i++) {
            for (int j = 0; j < ySize; j++) {
                boardInfo[i, j] = new TileInfo(gc.centerPosition(new Vector2(i, j)));
                GameObject content = gc.tileContent(new Vector2(i, j));
                if (content != null) {
                    boardInfo[i, j].Block = content.tag;
                }
            }
        }

        // Spawn dos jogadores
        boardInfo[0, ySize - 1].Spawn = true;
        boardInfo[1, ySize - 1].Spawn = true;
        boardInfo[0, ySize - 2].Spawn = true;
        if (playersAmount == 2) {
            boardInfo[xSize - 1, 0].Spawn = true;
            boardInfo[xSize - 2, 0].Spawn = true;
            boardInfo[xSize - 1, 1].Spawn = true;
        }

        Boneco player1 = Instantiate<Boneco>(Resources.Load<Boneco>("Prefabs/Boneco 1"));
        player1.transform.position = gc.centerPosition(new Vector2(0, ySize - 1)); // Ajusta o boneco pro centro da tile.
        player1.setup(sandboxMode);
        if (playersAmount == 2) {
            Boneco player2 = Instantiate<Boneco>(Resources.Load<Boneco>("Prefabs/Boneco 2"));
            player2.transform.position = gc.centerPosition(new Vector2(xSize - 1, 0)); // Ajusta o boneco pro centro da tile.
            player2.setup(sandboxMode);
        }

        // Criação dos blocos destrutíveis aleatórios e itens
        // Atenção (14/06/2018): Tem que gerar os blocos em locais "igualmente" aleatórios e garantindo que exatamente 80 blocos serão criados.
        if (!sandboxMode) {
            List<SoftBlock> rbs = new List<SoftBlock>();
            foreach (TileInfo t in boardInfo) {
                if (!t.Spawn && t.Block == "") {
                    if (UnityEngine.Random.Range(0, 100) < 70) {
                        t.Block = "SoftBlock";
                        rbs.Add(Instantiate(Resources.Load<SoftBlock>("Prefabs/SoftBlock"), t.Center, Quaternion.identity));
                    }
                }
            }
            randomizeItems(rbs);
        }
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
