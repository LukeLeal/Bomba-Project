﻿using System.Collections;
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
    * o —————> X+
    * 
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
        Debug.Log("Grid info \nCell Size: " + grid.cellSize
            + "\nCell gap: " + grid.cellGap
            //+ "\nCell "+grid.
            );
        if (randomBlocks) {
            generateBlocks();
        }

        Debug.Log("x");
        Debug.Log("x 10.5: " + centerPosition(new Vector2(10.5f, 0f)));
        Debug.Log("x 10.501: " + centerPosition(new Vector2(10.501f, 0)));
        Debug.Log("x 10.499: " + centerPosition(new Vector2(10.499f, 0)));
        Debug.Log("y");
        Debug.Log("y 10.5: " + centerPosition(new Vector2(0, 10.5f)));
        Debug.Log("y 10.501: " + centerPosition(new Vector2(0, 10.501f)));
        Debug.Log("y 10.499: " + centerPosition(new Vector2(0, 10.499f)));

        Debug.Log(tileMainContent(new Vector2(0, 11.0f)));
        Debug.Log(tileMainContent(new Vector2(0, 10.0f)));

        GameObject boneco = GameObject.FindWithTag("Player");
        boneco.transform.position = centerPosition(boneco.transform.position); // Ajusta o boneco pro centro da tile.
    }
	
	// Update is called once per frame
	void Update () {
		
	}

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
        } while (y < 100); // Limite pra impedir loop infinito :P

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

    // Centraliza o objeto na célula atual.
    public Vector2 centerPosition(Vector2 v) {
        // WorldToCell pega a posição global e converte pra da célula
        // GetCellCenterWorld pega a posição da célula e retorna a posição global do centro dela.
        return grid.GetCellCenterWorld(grid.WorldToCell(v));
    }

    /// <summary>
    /// Usado apenas no tratamento de certos raycasts e colisões.
    /// Em caso de impasse de dupla tile, 
    /// </summary>
    /// <param name="pos">Posição</param>
    /// <param name="dir">Direção (e.g. Vector2.up</param>
    /// <returns></returns>
    /// Quando tá no range 3 e indo pro negativo, da ruim. APENAS NESSE CASO WTF
    public Vector2 centerPosition(Vector2 pos, Vector2 dir) {
        pos = new Vector2((float)Math.Round(pos.x, 3), (float)Math.Round(pos.y, 3));
        // Ver se o resto da divisão de ??? por 1 é 0.5 .
        // Se for, Pega o tile no sentido da dir.
        if (dir == Vector2.left || dir == Vector2.right) {
            if(Mathf.Abs(pos.x) % 1 == 0.5) {
                pos = new Vector2(pos.x + dir.x * 0.1f, pos.y);
                //Debug.Log("New pos: " + pos);
            }
        } else if (dir == Vector2.up || dir == Vector2.down) {
            if (Mathf.Abs(pos.y) % 1 == 0.5) {
                pos = new Vector2(pos.x, pos.y + dir.y * 0.1f);
                //Debug.Log("New pos: " + pos);
            }
        } else {
            Debug.Log("PutaVida.exception: Impossible direction");
        }
        
        // Talvez apenas isso não seja o suficiente e um doubleCheck seja necessário
        return grid.GetCellCenterWorld(grid.WorldToCell(pos));
    }

    // Retorna o GO presente na camada de objects da tile.
    public IZOrder tileMainContent(Vector2 v) {
        Vector2 center = centerPosition(v); // Garante centralização

        // ATENÇÃO (26/01/2018): Se bugar, voltar distância pra 0.4
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

    void rayTest(Vector2 origin, Vector2 dir) {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.right, 3f);
        foreach (RaycastHit2D hit in hits) {
            IZOrder zo = hit.collider.gameObject.GetComponent<MonoBehaviour>() as IZOrder;

            Debug.Log("Dir: " + dir + " - Dist: " + Vector2.Distance(origin, centerPosition(hit.point, dir)) +
                " - HitPoint: " + hit.point + " - Hit CellCenter: " + centerPosition(hit.point, dir));
            Debug.Log(zo.gameObject.tag);

        }

    }

    /// <summary>
    /// Define aleatoriamente quais blocos terão items
    /// </summary>
    /// <param name="rbs">Lista de blocos</param>
    void randomizeItems(List<RegularBlock> rbs) {
        int itemsLeft = rbs.Count / 4;
        Debug.Log(itemsLeft);

        do {
            int rng = UnityEngine.Random.Range(0, rbs.Count);
            if(rbs[rng].ItemName == "") {
                rbs[rng].ItemName = "FireUp"; // Beta. Tem que pegar um item aleatorizado de uma itemPool.
                itemsLeft--;
            }
        } while (itemsLeft > 0);
        // Atenção (23/01/2018): Otimizar o loop pra não correr risco de rng infinita
    }

}
