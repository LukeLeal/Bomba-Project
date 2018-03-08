using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Classe responsável pelos métodos e dados do boneco
/// </summary>
public class Boneco : MonoBehaviour, IZOrder {

    GridController gc;

    //  shortcut transform.position #sdds

    int firePower = 2; // Tiles além do centro ocupado pela explosão da bomba (min = 1)
    int bombsMax = 1; // Quantidade de bombas do boneco
    int bombsUsed = 0; // Quantidade de bombas em uso (max = bombsMax)
    int speed = 6; // Velocidade de movimento do boneco
    bool hasKick = false;
    //bool hasPunch;
    //bool hasHold;
    bool dead = false;
    int zOrder;

    Vector2Int curDir = new Vector2Int();

    public const int MinFirePower = 2;
    public const int MaxFirePower = 10;

    #region gets & sets

    public int ZOrder {
        get { return zOrder; }
        set {
            GetComponent<Renderer>().sortingOrder = value;
            zOrder = value;
        }
    }

    public int BombsMax {
        get { return bombsMax; }
        set { bombsMax = value; }
    }

    public int FirePower {
        get { return firePower; }
        set {
            if (value < MinFirePower) {
                firePower = MinFirePower; 
            } else if (value > MaxFirePower) {
                firePower = MaxFirePower; 
            } else { 
                firePower = value;
            }
        }
    }

    public int BombsUsed {
        get { return bombsUsed; }
        set { bombsUsed = value; }
    }

    public bool Dead {
        get { return dead; }
        set { dead = value; }
    }

    public Vector2 curTile() {
        return gc.centerPosition(transform.position);
    }

    #endregion

    // Use this for initialization
    void Start () {
        gc = GridController.instance;
        zOrder = 2;

        if (!gc.randomBlocks) {
            FirePower = 6;
            BombsMax = 20;
            hasKick = true;
        }
	}
	
	// Update is called once per frame
	void Update () {

        #region Update movement stuff

        // (12/01/18): xInput e yInput não utilizados atm
        bool xInput = false, yInput = false, xMove = false, yMove = false, xObstacle = false, yObstacle = false;

        // Análise dos inputs de movimento
        if (Input.GetAxis("Horizontal") > 0) {
            xInput = true;
            xMove = possibleMove(Vector2.right, out xObstacle);
        } else if (Input.GetAxis("Horizontal") < 0) {
            xInput = true;
            xMove = possibleMove(Vector2.left, out xObstacle);
        }

        if (Input.GetAxis("Vertical") > 0) {
            yInput = true;
            yMove = possibleMove(Vector2.up, out yObstacle);
        } else if (Input.GetAxis("Vertical") < 0) {
            yInput = true;
            yMove = possibleMove(Vector2.down, out yObstacle);
        }

        if(xMove || yMove) {
            trueDirection(xMove, yMove);

            // Definição e realização do movimento
            if (curDir[0] == 1) {
                calculateMovement("Horizontal", xObstacle);
            } else if (curDir[1] == 1) {
                calculateMovement("Vertical", yObstacle);
            }
        } else {
            curDir = new Vector2Int(0, 0);
            // Apenas esquemas de rotações e animações SE houver input. Soon tm
        }

        #endregion

        if (hasKick) {
            if (!xMove && !yMove) {
                if (xInput) {
                    Vector2 dir = new Vector2(Mathf.Sign(Input.GetAxis("Horizontal")), 0);
                    Vector2 nextTile = curTile() + dir;
                    IZOrder content = gc.tileMainContent(nextTile);
                    if (content != null && content.gameObject.CompareTag("Bomb")) {
                        content.gameObject.GetComponent<Bomb>().wasKicked(dir);
                    }
                } else if (yInput) {
                    Vector2 dir = new Vector2(0, Mathf.Sign(Input.GetAxis("Vertical")));
                    Vector2 nextTile = curTile() + dir;
                    IZOrder content = gc.tileMainContent(nextTile);
                    if (content != null && content.gameObject.CompareTag("Bomb")) {
                        content.gameObject.GetComponent<Bomb>().wasKicked(dir);
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Z)){// || Input.GetKeyDown(KeyCode.A)) {
            placeBomb();
        }

        if (Input.GetKeyDown(KeyCode.C)) {
            Debug.Log(GridController.instance.grid.WorldToLocal(transform.position));
        }
    }

#region Movement functions
    /// <summary>
    /// Verifica se há alguma colisão que impede o movimento pretendido pelo boneco.
    /// </summary>
    /// <param name="dir"> Direção (e.g. Vector2.up) </param>
    /// <param name="obstacle"> Se true, indica movimento limitado. False whatever </param>
    /// <returns> Movimento possível ou não </returns>
    bool possibleMove(Vector2 dir, out bool obstacle) {
        obstacle = false;

        IZOrder zo = gc.tileMainContent((Vector2)transform.position + dir);
        if (zo != null) {

            if (zo.ZOrder == GridController.ZObjects) {

                // Se já tiver o mais próximo possível do obstáculo, movimento impossível
                if (dir == Vector2.right || dir == Vector2.left) {

                    // New - Baseado de fato na distância entre o boneco e o centro do objeto
                    if (Mathf.Abs(transform.position.x - gc.centerPosition((Vector2)transform.position + dir).x) <= 1) {
                        return false;
                    }

                    //if (transform.position.x - gc.centerPosition(transform.position).x == 0) {
                    //    return false;
                    //}

                } else if(dir == Vector2.up || dir == Vector2.down) {

                    // New
                    if (Mathf.Abs(transform.position.y - gc.centerPosition((Vector2)transform.position + dir).y) <= 1) {
                        return false;
                    }

                    //if (transform.position.y - gc.centerPosition(transform.position).y == 0) {
                    //    return false;
                    //}
                } else {
                    Debug.Log("PutaVida.exception: Impossible direction");
                }
                obstacle = true;
            }
        }
        return true;
    }

    /// <summary>
    /// Baseado na direção antiga e nos novos inputs, define qual será a nova direção do boneco
    /// Desenho dos estados no "automato" na pasta "design & etc"
    /// </summary>
    /// <param name="x, y"> Se há inputs de movimento horizontal / vertical </param>
    void trueDirection(bool x, bool y) {

        if (x && y) {
            if (curDir == new Vector2Int(0, 0)) { 
                curDir = new Vector2Int(1, 2);

            } else if (curDir == new Vector2Int(1, 0)) {
                curDir = new Vector2Int(2, 1);

            } else if (curDir == new Vector2Int(0, 1)) { 
                curDir = new Vector2Int(1, 2);
            } 
        
        } else if (x) {
            curDir = new Vector2Int(1, 0);
        } else if (y) {
            curDir = new Vector2Int(0, 1);
        } 

    }

    /// <summary>
    /// Define qual será o movimento realizado pelo boneco de acordo com o input e a posição atual.
    /// A ideia do movimento no jogo é sempre se manter no centro de um dos eixos da tile (ou ir a ele nas "curvas").
    /// - ATENÇÃO (Janeiro/2018): O translate pra fazer o movimento quebra o galho, mas não acho ideal. 
    ///     O boneco fica com um "Efeito Luigi". Vai um pouco mais à frente do que deve nos movimentos.
    /// </summary>
    /// <param name="dir"> Direção (e.g. Vector2.up) </param>
    /// <param name="obstacle"> Se há algum elemento à frente que limita o movimento </param>
    void calculateMovement(string dir, bool obstacle) {

        float moveConst = Time.deltaTime * speed; // BETA. 

        if (dir == "Vertical") {
            if (obstacle) {
                // Com obstáculo à frente, pode apenas ir até o meio do eixo corrente
                if (Mathf.Abs (transform.position.y - curTile().y) > 0.1) {
                    transform.Translate(0, Mathf.Sign(Input.GetAxis(dir)) * moveConst, 0);
                } else {
                    transform.position = new Vector2(transform.position.x, curTile().y);
                }

            } else { // Vertical sem obstáculo
                float dif = transform.position.x - curTile().x;

                // Dependendo da distância do boneco ao centro do eixo horizontal da tile
                if (Mathf.Abs(dif) > 0.1) {
                    // Move diagonalmente até se aproximar
                    transform.Translate(Mathf.Sign(dif) * -1 * moveConst, Mathf.Sign(Input.GetAxis(dir)) * moveConst, 0);

                } else if (Mathf.Abs(dif) > 0) {
                    // Coloca no centro horizontal e segue o movimento vertical
                    transform.position = new Vector2(curTile().x, transform.position.y);
                    transform.Translate(0, Mathf.Sign(Input.GetAxis(dir)) * moveConst, 0);

                } else {
                    // Apenas move verticalmente
                    transform.Translate(0, Mathf.Sign(Input.GetAxis(dir)) * moveConst, 0);
                }
            }

        } else if (dir == "Horizontal") {
            if (obstacle) {
                // Com obstáculo à frente, pode apenas ir até o meio do eixo corrente
                if (Mathf.Abs (transform.position.x - curTile().x) > 0.1) {
                    transform.Translate(Mathf.Sign(Input.GetAxis(dir)) * moveConst, 0, 0);
                } else {
                    transform.position = new Vector2(curTile().x, transform.position.y);
                }

            } else { // Horizontal sem obstáculo
                float dif = transform.position.y - curTile().y;

                // Dependendo da distância do boneco ao centro do eixo vertical da tile
                if (Mathf.Abs(dif) > 0.1) {
                    // Move diagonalmente até se aproximar
                    transform.Translate(Mathf.Sign(Input.GetAxis(dir)) * moveConst, Mathf.Sign(dif) * -1 * moveConst, 0);

                } else if (Mathf.Abs(dif) > 0) {
                    // Coloca no centro vertical e segue o movimento horizontal
                    transform.position = new Vector2(transform.position.x, curTile().y);
                    transform.Translate(Mathf.Sign(Input.GetAxis(dir)) * moveConst, 0, 0);

                } else {
                    // Apenas move horizontalmente
                    transform.Translate(Mathf.Sign(Input.GetAxis(dir)) * moveConst, 0, 0);
                }
            }
        }
    }
#endregion

    /// <summary>
    /// Cria bomba no tile atual se possível
    /// </summary>
    void placeBomb() {       
        if (!dead && BombsUsed < bombsMax && GridController.instance.tileMainContent(transform.position) == null) {
            BombsUsed++;
            Bomb b = Instantiate(Resources.Load<Bomb>("prefabs/Bomb"));
            b.setup(this);
        }
    }

    /// <summary>
    /// Realiza as ações e alterações de acordo com o item adquirido.
    /// </summary>
    void gotItem(Item item) {
        if(item == null) {
            Debug.Log("PutaVida.Exception: GO com tag Item não é Item");
            return;
        }

        GetComponent<AudioSource>().Play();
        switch (item.name) {
            case "FireUp": 
                FirePower++;
                break;

            case "BombUp":
                BombsMax++;
                break;

            case "Kick":
                hasKick = true;
                break;

            default:
                Debug.Log("PutaVida.Exception: ItemNotFound");
                break;
        }
        Destroy(item.gameObject);
    }

    // BETA
    IEnumerator die() {
        dead = true;
        yield return new WaitForSeconds(2.5f);
        GetComponent<SpriteRenderer>().color = Color.white;
        dead = false;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.CompareTag("Explosion")) {
            GetComponent<SpriteRenderer>().color = Color.red;
            StartCoroutine(die());
        } else if (collider.CompareTag("Item")) {
            gotItem(collider.gameObject.GetComponent<Item>());
        }
    }

}
