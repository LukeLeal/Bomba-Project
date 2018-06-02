using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Classe responsável por realizar cálculos e raycasts relacionados às tiles.
/// </summary>
public class GridCalculator : Singleton<GridCalculator> { 

    /* Singleton pois:
     * - Qualquer um pode usar.
     * - Precisa de apenas uma instância.
     * 
     * Tentei como static class, mas não ficou legal. Precisava pegar o grid e achei que isso poderia ser problemático na troca de cena.
     * Ficar passando entre os objetos que a usam não seria prático.
     * "FindObjectOfType" é lento e a própria documentação recomenda usar singleton pra maioria dos casos em que ele poderia ser usado =P .
     * 
     * Talvez eu faça umas modificações pra garantir que GridCalculator só possa ser chamado por objetos em cenas que possuam uma grid. Hmm...
     * ---
     * Separei esses esquemas do GridController pq achei que eles poderiam ser reutilizados em outros projetos tile-based.
     * O problema é que alguns dos métodos estão muito "atrelados" a forma como eles funcionam no Bomba. 
     * Espero eventualmente encontrar um jeito de generalizá-los , mas ainda atender às necessidades deste projeto.
     * */

    /// <summary>
    /// Grid de jogo atual. Setado pelo editor atm.
    /// </summary>
    public Grid grid;

    // Constantes Layers
    public const int Background = 8;
    public const int Surface = 9;
    public const int Objects = 10;
    public const int Bonecos = 11;
    public const int Above = 12;
    public const int Flying = 13;
    public const int Top = 14;

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
            if (Mathf.Abs(pos.x) % 1 == 0.5) {
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
    /// </summary>
    /// Há o caso especial pra GridBlocks e Border no if pois, por serem feitos pelo tilemap brush, a posição real deles não é na tile.
    ///     Porém, por ocuparem apenas uma tile fixa e tile.size > overlapBox.size, não é necessário fazer aquela garantia de posição.
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

        return null; // Tile vazia na camada de objects.
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

    //public void updateGridObject() {
    //    grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid>();
    //}
}
