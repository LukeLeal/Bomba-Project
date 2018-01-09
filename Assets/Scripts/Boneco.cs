using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Classe responsável pelos métodos e dados do boneco
/// </summary>
public class Boneco : MonoBehaviour {

    GridController gc;

    //  shortcut transform.position #sdds

    int firePower = 6; // Tiles além do centro ocupado pela explosão da bomba (min = 1)
    int bombsMax = 10; // Quantidade de bombas do boneco (min = 1)
    int bombsUsed = 0; // Quantidade de bombas em uso (max = bombsMax)
    int speed = 6; // Velocidade de movimento do boneco
    bool kick;
    bool punch;
    bool hold;

    Vector2Int curDir = new Vector2Int();

    #region gets & sets
    public int BombsMax {
        get { return bombsMax; }
        set { bombsMax = value; }
    }

    public int FirePower {
        get { return firePower; }
        set { firePower = value; }
    }

    public int BombsUsed {
        get {
            return bombsUsed;
        }
        set {
            bombsUsed = value;
        }
    }
    #endregion

    // Use this for initialization
    void Start () {
        gc = GridController.instance;
	}
	
	// Update is called once per frame
	void Update () {

        #region Movement

        bool[] obstacle = { false, false }; // [0] = Obstáculo na horizontal / [1] = Obstáculo na vertical
        bool xInput = false, yInput = false; 

        // Análise dos inputs
        if (Input.GetAxis("Horizontal") > 0) {
            if (possibleMove(Vector2.right, out obstacle[0])) {
                xInput = true;
            }
        } else if (Input.GetAxis("Horizontal") < 0) {
            if (possibleMove(Vector2.left, out obstacle[0])) {
                xInput = true;
            }
        }

        if (Input.GetAxis("Vertical") > 0) {

            if (possibleMove(Vector2.up, out obstacle[1])) {
                yInput = true;
            }
        } else if (Input.GetAxis("Vertical") < 0) {
            if (possibleMove(Vector2.down, out obstacle[1])) {
                yInput = true;
            }

        }

        trueDirection(xInput, yInput);

        // Definição e realização do movimento
        if(curDir[0] == 1) {
            calculateMovement("Horizontal", obstacle[0]);
        } else if (curDir[1] == 1) {
            calculateMovement("Vertical", obstacle[1]);
        }

        #endregion

        if (Input.GetKeyDown(KeyCode.Z)){// || Input.GetKeyDown(KeyCode.A)) {
            placeBomb();
        }

        if (Input.GetKeyDown(KeyCode.C)) {
            Debug.Log(GridController.instance.grid.WorldToLocal(transform.position));
        }

    }

    // Baseado na direção antiga e nos novos inputs, define qual será a nova direção do boneco
    // Desenho dos estados no "automato" na pasta 'design & etc'
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
        } else {
            curDir = new Vector2Int(0, 0);
        }
    }

    // Define qual será o movimento realizado pelo boneco de acordo com o input e a posição atual.
    // A ideia do movimento no jogo é sempre se manter no centro de um dos eixos da tile (ou próximo dele).
    void calculateMovement(string dir, bool obstacle) {

        float moveConst = Time.deltaTime * speed; // BETA. 

        if (!obstacle) {
            if (dir == "Vertical") {
                float dif = transform.position.x - gc.centerPosition(transform.position).x; 

                // Dependendo da distância do boneco ao centro do eixo horizontal da tile
                if(Mathf.Abs(dif) > 0.1) {
                    // Move diagonalmente até se aproximar
                    transform.Translate(Mathf.Sign(dif) * -1 * moveConst, Mathf.Sign(Input.GetAxis(dir)) * moveConst , 0);

                } else if (Mathf.Abs(dif) > 0) {
                    // Coloca no centro horizontal e segue o movimento vertical
                    transform.position = new Vector2(gc.centerPosition(transform.position).x, transform.position.y);
                    transform.Translate(0, Mathf.Sign(Input.GetAxis(dir)) * moveConst, 0);

                } else {
                    // Apenas move verticalmente
                    transform.Translate(0, Mathf.Sign(Input.GetAxis(dir)) * moveConst, 0);
                }

            } else if (dir == "Horizontal") {
                float dif = transform.position.y - gc.centerPosition(transform.position).y;

                // Dependendo da distância do boneco ao centro do eixo vertical da tile
                if (Mathf.Abs(dif) > 0.1) {
                    // Move diagonalmente até se aproximar
                    transform.Translate(Mathf.Sign(Input.GetAxis(dir)) * moveConst, Mathf.Sign(dif) * -1 * moveConst, 0);

                } else if (Mathf.Abs(dif) > 0) {
                    // Coloca no centro vertical e segue o movimento horizontal
                    transform.position = new Vector2(transform.position.x, gc.centerPosition(transform.position).y);
                    transform.Translate(Mathf.Sign(Input.GetAxis(dir)) * moveConst, 0, 0);

                } else {
                    // Apenas move horizontalmente
                    transform.Translate(Mathf.Sign(Input.GetAxis(dir)) * moveConst, 0, 0);
                }         
            }

        } else {
            
        }
    }

    // Cria bomba no tile atual
    void placeBomb() {       
        if (BombsUsed < bombsMax && GridController.instance.tileMainContent(transform.position) == null) {
            BombsUsed++;
            Bomb b = Instantiate(Resources.Load<Bomb>("prefabs/Bomb"));
            b.setup(this);
        }
    }

    // Verifica se há alguma colisão que impede o movimento pretendido pelo boneco.
    bool possibleMove(Vector2 dir, out bool obstacle) {
        // Cria raycast a partir de (x,y), na direção dir com distância de uma tile
        float x = transform.position.x;
        float y = transform.position.y;
        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(x, y), dir, 1);
        obstacle = false; // BETA

        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.gameObject == gameObject || // Ignora o raycasthit do próprio collider. 
                hit.collider.gameObject.tag == "Explosion" || // Pode ir onde tem explosão. Só que morre nisso. Hue
                (hit.collider.gameObject.tag == "Bomb" && gc.centerPosition(hit.point) == gc.centerPosition(new Vector2(x, y)))) {
                // Atravessa colisão apenas se for uma bomba e estiver "dentro" dela. 

                // ATENÇÃO (05/12/17): O da bomba vai dar ruim quando o movimento do boneco ficar dinâmico.
                // ATENÇÃO (03/01/18): Ficou rui mesmo LMAO. 05/01/18: Ajuste temp (?) só mandando centralizar
                continue; 
            }  else {
                return false; // Qualquer outra colisão, não pode.
            }
        }
        return true;
    }

    // BETA
    IEnumerator die() {
        yield return new WaitForSeconds(2.5f);
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.GetComponent<Collider2D>().tag == "Explosion") {
            GetComponent<SpriteRenderer>().color = Color.red;
            StartCoroutine(die());
        }
    }

    //void 
}
