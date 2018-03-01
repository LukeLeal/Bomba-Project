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

    // Constantes ZOrder
    public const int ZBackground = -1;
    public const int ZSurface = 0;
    public const int ZObjects = 1;
    public const int ZBonecos = 2;
    public const int ZAbove = 3;
    public const int ZFlying = 4;
    public const int ZTop = 5;

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
        //newtileMainContent(new Vector2(2, 1));
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.O)) {
            Debug.Log(tileContentOnZOrders(new Vector2(2, 2), new int[] { 1, 2 }).Count);
        }
    }

    /// <summary>
    /// Cria e posiciona os blocos destrutíveis no mapa. Também define se eles terão items.
    /// </summary>
    void generateBlocks() {
        // Verifica se o mapa está corretamente posicionado
        Vector2 curPos = centerPosition(new Vector2(0, 0));
        if(curPos != new Vector2(0, 0) || tileMainContent(new Vector2(-1, 0)).gameObject.tag != "Border" ||
            tileMainContent(new Vector2(0, -1)).gameObject.tag != "Border") {
            Debug.Log("Deu ruim. Tabuleiro mal formado"); // PutaVida.exception
        }

        // Pegando tamanho da parte jogável do mapa
        int x = 1, y = 1;
        do {
            IZOrder zo = tileMainContent(curPos + Vector2.up);
            if(zo != null) {
                if(zo.gameObject.CompareTag("Border")) {
                    break;
                }
            }
            curPos += Vector2.up;
            y++;
        } while (y < 100); // Limite arbitrário pra impedir loop infinito :P

        do {
            IZOrder zo = tileMainContent(curPos + Vector2.right);
            if (zo != null) {
                if (zo.gameObject.CompareTag("Border")) {
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
                IZOrder zo = tileMainContent(new Vector2(i, j));
                if (zo != null) {
                    gridInfo[i, j].Block = zo.gameObject.tag;
                }
            }
        }

        // Setagem de spawn beta. 
        gridInfo[0, 0].Spawn = true;
        gridInfo[1, 0].Spawn = true;
        gridInfo[0, 1].Spawn = true;

        // Criação dos blocos destrutíveis aleatórios
        List<RegularBlock> rbs = new List<RegularBlock>();
        foreach (TileInfo t in gridInfo) {
            if (!t.Spawn && t.Block == "") {
                if (UnityEngine.Random.Range(0, 100) < 70) {
                    t.Block = "RegularBlock";
                    rbs.Add( Instantiate(Resources.Load<RegularBlock>("Prefabs/RegularBlock"), t.Center, Quaternion.identity) );
                }
            }
        }
        randomizeItems(rbs);
    }

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
    /// - Em casos bem específicos, float ficava de brincation. "Float perdendo precisão wtf.png"
    /// </summary>
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
    /// Retorna o ZO (gameObject com ZOrder) presente na camada de objects da tile.
    /// </summary>
    public IZOrder oldtileMainContent(Vector2 pos) {
        Vector2 center = centerPosition(pos); // Garante centralização

        RaycastHit2D[] hits = Physics2D.RaycastAll(center, Vector2.up, 0.05f);
        foreach (RaycastHit2D hit in hits) {
            IZOrder zo = hit.collider.gameObject.GetComponent<MonoBehaviour>() as IZOrder;
            if (zo != null) {
                if (zo.ZOrder == GridController.ZObjects && centerPosition(hit.point) == center) {
                    return zo;
                }
            } else {
                Debug.Log("PutaVida.exception: Objeto sem ZOrder no tabuleiro - " + centerPosition(hit.point));
            }
        }
        return null; // Tile vazia.
    }

    /// <summary>
    /// Retorna o ZO (gameObject com ZOrder) presente na camada de objects da tile.
    /// 
    /// Há o caso especial pra GridBlocks e Border no if pois, por serem feitos pelo tilemap brush, a posição real deles não é na tile.
    ///     Porém, por ocuparem apenas uma tile fixa e tile.size > overlapBox.size, não é necessário fazer aquela garantia de posição.
    /// </summary>
    public IZOrder tileMainContent(Vector2 pos) {
        Vector2 center = centerPosition(pos); // Garante centralização

        Collider2D[] collidersInTile = Physics2D.OverlapBoxAll(center, new Vector2(0.9f, 0.9f), 0);
        foreach (Collider2D collider in collidersInTile) {
            IZOrder zo = collider.gameObject.GetComponent<MonoBehaviour>() as IZOrder;
            if (zo != null) {
                if (zo.ZOrder == GridController.ZObjects && 
                    (centerPosition(zo.gameObject.transform.position) == center || zo.gameObject.CompareTag("GridBlocks") ||
                    zo.gameObject.CompareTag("Border"))) {

                    return zo;
                }
            } else {
                Debug.Log("PutaVida.exception: Objeto sem ZOrder no tabuleiro - " + centerPosition(collider.transform.position));
            }
        }
        
        return null; // Tile vazia na cada de objects.
    }

    /// <summary>
    /// Retorna todos os zobjetos nas zorders desejada (eu tenho que melhorar essa nomenclatura lol)
    /// 
    /// Talvez eu faça um overload com o tileMainContent.
    /// </summary>
    /// <param name="pos">Posição da tile</param>
    /// <param name="zorders">Camadas ZOrder desejadas</param>
    /// <returns></returns>
    public List<IZOrder> tileContentOnZOrders(Vector2 pos, int[] zorders) {
        Vector2 center = centerPosition(pos); // Garante centralização
        List<IZOrder> zobjs = new List<IZOrder>();

        Collider2D[] collidersInTile = Physics2D.OverlapBoxAll(center, new Vector2(0.9f, 0.9f), 0);
        foreach (Collider2D collider in collidersInTile) {
            IZOrder zo = collider.gameObject.GetComponent<MonoBehaviour>() as IZOrder;
            if (zo != null) {
                if (UnityEditor.ArrayUtility.Contains(zorders, zo.ZOrder) &&
                    (centerPosition(zo.gameObject.transform.position) == center || zo.gameObject.CompareTag("GridBlocks") ||
                    zo.gameObject.CompareTag("Border"))) {

                    zobjs.Add(zo);
                }
            } else {
                Debug.Log("PutaVida.exception: Objeto sem ZOrder no tabuleiro - " + centerPosition(collider.transform.position));
            }
        }
        return zobjs; // Tile vazia na cada de objects.
    }

    /// <summary>
    /// Define aleatoriamente quais blocos terão items
    /// - Atenção (23/01/2018): Otimizar o loop pra não correr risco de rng infinita
    /// </summary>
    /// <param name="rbs"> Lista de blocos </param>
    void randomizeItems(List<RegularBlock> rbs) {
        int itemsLeft = rbs.Count / 4;
        Debug.Log(itemsLeft);

        do {
            int rng = UnityEngine.Random.Range(0, rbs.Count);
            if(rbs[rng].ItemName == "") {
                if (itemsLeft % 2 == 0) {
                    rbs[rng].ItemName = "BombUp"; 
                } else {
                    rbs[rng].ItemName = "FireUp"; 
                }
                itemsLeft--;
            }
        } while (itemsLeft > 0);
    }

    /// <summary>
    /// Método para testes de raycast.
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="dir"></param>
    void rayTest(Vector2 origin, Vector2 dir) {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.right, 3f);
        foreach (RaycastHit2D hit in hits) {
            IZOrder zo = hit.collider.gameObject.GetComponent<MonoBehaviour>() as IZOrder;

            Debug.Log("Dir: " + dir + " - Dist: " + Vector2.Distance(origin, centerPosition(hit.point, dir)) +
                " - HitPoint: " + hit.point + " - Hit CellCenter: " + centerPosition(hit.point, dir));
            Debug.Log(zo.gameObject.tag);
        }
    }
}
