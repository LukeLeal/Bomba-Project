using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    * 0 —————> X+
    */

    // Constantes Layers
    public const int Background = 8;
    public const int Surface = 9;
    public const int Objects = 10;
    public const int Bonecos = 11;
    public const int Above = 12;
    public const int Flying = 13;
    public const int Top = 14;

    // Use this for initialization
    void Start() {
        //Debug.Log("Grid info \nCell Size: " + grid.cellSize
        //    + "\nCell gap: " + grid.cellGap
        //    //+ "\nCell "+grid.
        //    );

        if (randomBlocks) {
            generateBlocks();
        } 

        GameObject boneco = GameObject.FindWithTag("Player");
        boneco.transform.position = centerPosition(boneco.transform.position); // Ajusta o boneco pro centro da tile.
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.O)) {
            tileContent(new Vector2(2, 2), new int[] { Objects, Bonecos });
            //Debug.Log(tileContentOnLayers(new Vector2(2, 2), new int[] { Objects, Bonecos }));
        }
    }

    #region Grid Setup
    /// <summary>
    /// Cria e posiciona os blocos destrutíveis no mapa. 
    /// </summary>
    void generateBlocks() {
        // Verifica se o mapa está corretamente posicionado
        Vector2 curPos = centerPosition(new Vector2(0, 0));
        if (curPos != new Vector2(0, 0) || !tileContent(new Vector2(-1, 0)).CompareTag("Border") ||
            !tileContent(new Vector2(0, -1)).CompareTag("Border")) {
            Debug.Log("Deu ruim. Tabuleiro mal formado"); // PutaVida.exception
        }

        // Pegando tamanho da parte jogável do mapa
        int x = 1, y = 1;
        do {
            GameObject content = tileContent(curPos + Vector2.up);
            if (content != null && content.CompareTag("Border")) {
                break;
            }
            curPos += Vector2.up;
            y++;
        } while (y < 100); // Limite arbitrário pra impedir loop infinito :P

        do {
            GameObject content = tileContent(curPos + Vector2.right);
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
                gridInfo[i, j] = new TileInfo(centerPosition(new Vector2(i, j)));
                GameObject content = tileContent(new Vector2(i, j));
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
            new Tuple<string, int>("BombUp", 8),
            new Tuple<string, int>("FireUp", 5),
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

    #region Utilidades
    /// <summary>
    /// Acha a posição central da célula
    /// </summary>
    public Vector2 centerPosition(Vector2 pos) {
        // WorldToCell pega a posição global e converte pra da célula
        // GetCellCenterWorld pega a posição da célula e retorna a posição global do centro dela.
        return grid.GetCellCenterWorld(grid.WorldToCell(pos));
    }

    /// <summary>
    /// Acha a posição central da célula. Usado apenas no tratamento de certos raycasts e colisões.
    /// </summary>
    /// Em casos bem específicos, float ficava de brincation. Por isso os arredondamentos
    /// - "Assets/Design & etc/Float perdendo precisão wtf.png"
    /// <param name="pos"> Posição </param>
    /// <param name="dir"> Direção (e.g. Vector2.up) </param>
    /// <returns> Posição centralizada na devida célula </returns>
    public Vector2 centerPosition(Vector2 pos, Vector2 dir) {
        pos = new Vector2((float)Math.Round(pos.x, 2), (float)Math.Round(pos.y, 2)); // Pra garantir precisão

        if (dir == Vector2.left || dir == Vector2.right) {
            if(Mathf.Abs(pos.x) % 1 == 0.5) {
                pos = new Vector2(pos.x + dir.x * 0.1f, pos.y);
            }
        } else if (dir == Vector2.up || dir == Vector2.down) {
            if (Mathf.Abs(pos.y) % 1 == 0.5) {
                pos = new Vector2(pos.x, pos.y + dir.y * 0.1f);
            }
        } else {
            Debug.Log("PutaVida.exception: Impossible direction");
        }
        
        return grid.GetCellCenterWorld(grid.WorldToCell(pos));
    }

    /// <summary>
    /// Retorna o gameObject na "Objects" layer da tile (deve haver no máximo gameObject).
    /// 
    /// Há o caso especial pra GridBlocks e Border no if pois, por serem feitos pelo tilemap brush, a posição real deles não é na tile.
    ///     Porém, por ocuparem apenas uma tile fixa e tile.size > overlapBox.size, não é necessário fazer aquela garantia de posição.
    /// </summary>
    /// <param name="pos"> Posição da tile </param>
    public GameObject tileContent(Vector2 pos) {
        Vector2 center = centerPosition(pos); // Garante centralização
        int layerMask = 1 << Objects;
        Collider2D[] collidersInTile = Physics2D.OverlapBoxAll(center, new Vector2(0.9f, 0.9f), 0, layerMask);

        foreach (Collider2D collider in collidersInTile) {
            if (centerPosition(collider.gameObject.transform.position) == center || collider.gameObject.CompareTag("GridBlocks") ||
                collider.gameObject.CompareTag("Border")) {
                return collider.gameObject;
            }
        }

        return null; // Tile vazia na cada de objects.
    }

    /// <summary>
    /// Retorna todos os objetos encontrados nas layers desejadas
    /// </summary>
    /// <param name="pos"> Posição da tile</param>
    /// <param name="layers"> Nº das layers a serem vasculhadas </param>
    public List<GameObject> tileContent(Vector2 pos, params int[] layers) {
        Vector2 center = centerPosition(pos); // Garante centralização
        List<GameObject> contents = new List<GameObject>();

        // LayerMask's BitMagic
        int layerMask = 0;
        foreach (int i in layers) {
            layerMask |= 1 << i;
        }

        Collider2D[] collidersInTile = Physics2D.OverlapBoxAll(center, new Vector2(0.9f, 0.9f), 0, layerMask);
        foreach (Collider2D collider in collidersInTile) {

            if (centerPosition(collider.gameObject.transform.position) == center || collider.gameObject.CompareTag("GridBlocks") ||
                collider.gameObject.CompareTag("Border")) {
                contents.Add(collider.gameObject);
            }

        }
        return contents; 
    }

    /// <summary>
    /// Retorna todos os objetos encontrados nas layers desejadas
    /// </summary>
    /// <param name="pos"> Posição da tile</param>
    /// <param name="layers"> Nome das layers a serem vasculhadas </param>
    public List<GameObject> tileContent(Vector2 pos, params string[] layers) {
        Vector2 center = centerPosition(pos); // Garante centralização
        List<GameObject> contents = new List<GameObject>();
        int layerMask = LayerMask.GetMask(layers);

        Collider2D[] collidersInTile = Physics2D.OverlapBoxAll(center, new Vector2(0.9f, 0.9f), 0, layerMask);
        foreach (Collider2D collider in collidersInTile) {

            if (centerPosition(collider.gameObject.transform.position) == center || collider.gameObject.CompareTag("GridBlocks") ||
                collider.gameObject.CompareTag("Border")) {
                contents.Add(collider.gameObject);
            }

        }
        return contents; 
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

            Debug.Log("Dir: " + dir + " - Dist: " + Vector2.Distance(origin, centerPosition(hit.point, dir)) +
                " - HitPoint: " + hit.point + " - Hit CellCenter: " + centerPosition(hit.point, dir));
            Debug.Log(gameObject.tag);
        }
    }
}
