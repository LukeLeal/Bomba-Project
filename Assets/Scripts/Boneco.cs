using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Classe responsável pelos métodos e dados do boneco
/// </summary>
public class Boneco : MonoBehaviour {

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
    string sfxPath = "Sounds/SFX/Boneco/";

    Vector2Int curDir = new Vector2Int();

    public const int MinFirePower = 2;
    public const int MaxFirePower = 10;

    #region gets & sets

    public int Layer {
        get { return gameObject.layer; }
        set {
            GetComponentsInChildren<Renderer>()[1].sortingOrder = value;
            gameObject.layer = value;
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
        GetComponentsInChildren<Renderer>()[1].sortingOrder = Layer;

        if (!gc.randomBlocks) {
            FirePower = 6;
            BombsMax = 20;
            hasKick = true;
        }
	}
	
	// Update is called once per frame
	void Update () {

        #region Update movement stuff

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
            // Provavelmente dá pra limpar um pouco essa parte
            if (!xMove && !yMove) {
                if (xInput) {
                    Vector2 dir = new Vector2(Mathf.Sign(Input.GetAxis("Horizontal")), 0);
                    Vector2 nextTile = curTile() + dir;

                    GameObject content = gc.tileContent(nextTile);
                    if (content != null && content.CompareTag("Bomb")) {
                        content.GetComponent<Bomb>().wasKicked(dir);
                    }
                } else if (yInput) {
                    Vector2 dir = new Vector2(0, Mathf.Sign(Input.GetAxis("Vertical")));
                    Vector2 nextTile = curTile() + dir;

                    GameObject content = gc.tileContent(nextTile);
                    if (content != null && content.CompareTag("Bomb")) {
                        content.GetComponent<Bomb>().wasKicked(dir);
                    }
                }
            }
        }

        // ATENÇÃO (16/03/18): Sincronizar joystick com jogador quando tiver multiplayer. Só tá assim agora pq os ports ficam de brincation.
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.JoystickButton1)) {
            placeBomb();
        }

        #region other inputs
        // http://wiki.unity3d.com/index.php?title=Xbox360Controller
        /* PC Mode WiiU Adapter:
            0: X
            1: A
            2: B
            3: Y
            4: L
            5: R
            6: ???
            7: Z
            8: ???
        */

        // Literal Pause. Ayy lmao
        if (Input.GetKeyDown(KeyCode.JoystickButton7)) {
            Debug.Break();
        }

        if (Input.GetKeyDown(KeyCode.C)) {
            //Debug.Log(GridController.instance.grid.WorldToLocal(transform.position));
            //Debug.Log(gc.tileContent((Vector2)transform.position + Vector2.up).name);
        }
        #endregion
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

        GameObject go = gc.tileContent((Vector2)transform.position + dir);
        if (go != null) {

            // Se já tiver o mais próximo possível do obstáculo, movimento impossível
            if (dir == Vector2.right || dir == Vector2.left) {

                // Baseado de fato na distância entre o boneco e o centro do objeto
                if (Mathf.Abs(transform.position.x - gc.centerPosition((Vector2)transform.position + dir).x) <= 1) {
                    return false;
                }

            } else if (dir == Vector2.up || dir == Vector2.down) {

                if (Mathf.Abs(transform.position.y - gc.centerPosition((Vector2)transform.position + dir).y) <= 1) {
                    return false;
                }

            } else {
                Debug.Log("PutaVida.exception: Impossible direction");
            }
            obstacle = true;
            
        }
        return true;
    }

    /// <summary>
    /// Baseado na direção antiga e nos novos inputs, define qual será a nova direção do boneco
    /// </summary>
    /// <param name="x, y"> Se há inputs de movimento horizontal / vertical </param>
    /// Desenho dos estados no "automato" na pasta "design & etc"
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
    /// </summary>
    /// <param name="dir"> Direção (e.g. Vector2.up) </param>
    /// <param name="obstacle"> Se há algum elemento à frente que limita o movimento </param>
    void calculateMovement(string dir, bool obstacle) {
        // - ATENÇÃO (Janeiro/2018): O translate pra fazer o movimento quebra o galho, mas não acho ideal. 
        //     O boneco fica com um "Efeito Luigi". Vai um pouco mais à frente do que deve nos movimentos.
        // - Update (16/03/2018): O "Efeito Luigi" aparentemente só ocorre quando usa teclado como input.    
        //     Até onde testei, o movimento usando controle de GameCube tá 10/10. A diferença deve ser por causa do analógico,
        //     mas não deixa de ser estranho... Devo ter feito alguma bosta nas fórmulas ai, ou o teclado é RUI mesmo hue.

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
    /// Cria bomba na tile atual se possível
    /// </summary>
    void placeBomb() {       
        if (!dead && BombsUsed < bombsMax && gc.tileContent(transform.position) == null) {
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

        GetComponent<AudioSource>().clip = (AudioClip)Resources.Load(sfxPath + "GotItem");
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

    /// <summary>
    /// BETA. Controla o tempo em que o boneco fica no estado dead (sem bombas e vermelho)
    /// </summary>
    IEnumerator die() {
        yield return new WaitForSeconds(1.5f);
        gameObject.GetComponentsInChildren<SpriteRenderer>()[1].color = Color.white;
        dead = false;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.CompareTag("Explosion") && !dead) {
            dead = true;
            gameObject.GetComponentsInChildren<SpriteRenderer>()[1].color = Color.red; // Beta
            GetComponent<AudioSource>().clip = (AudioClip)Resources.Load(sfxPath + "Death");
            GetComponent<AudioSource>().Play();
            StartCoroutine(die());
        } else if (collider.CompareTag("Item")) {
            gotItem(collider.gameObject.GetComponent<Item>());
        }
    }

}