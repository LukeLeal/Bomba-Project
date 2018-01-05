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
    int speed = 10; // Velocidade de movimento do boneco
    bool kick;
    bool punch;
    bool hold;

    Vector2Int curMove = new Vector2Int();

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

        #region ortogonal movement inputs

        bool[] border = { false, false }; 
        bool xInput = false, yInput = false; 

        if (Input.GetAxis("Horizontal") > 0) {
            if (possibleMove(Vector2.right, out border[0])) {
                xInput = true;
            }
        } else if (Input.GetAxis("Horizontal") < 0) {
            if (possibleMove(Vector2.left, out border[0])) {
                xInput = true;
            }
        }

        if (Input.GetAxis("Vertical") > 0) {

            // Verifica se o move é possível. Se for, faz. Se não, fica de boa.
            if (possibleMove(Vector2.up, out border[1])) {
                yInput = true;
            }
        } else if (Input.GetAxis("Vertical") < 0) {
            if (possibleMove(Vector2.down, out border[1])) {
                yInput = true;
            }

        } 

        automatum(xInput, yInput);

        if(curMove[0] == 1) {
            calculateMovement("Horizontal", border[0]);
        } else if (curMove[1] == 1) {
            calculateMovement("Vertical", border[1]);
        }

        #endregion

        if (Input.GetKeyDown(KeyCode.Z)){// || Input.GetKeyDown(KeyCode.A)) {
            placeBomb();
        }

        if (Input.GetKeyDown(KeyCode.C)) {
            Debug.Log(GridController.instance.grid.WorldToLocal(transform.position));
        }

    }

    // nome beta
    void automatum(bool x, bool y) {

        if (x && y) {
            if (curMove == new Vector2Int(0, 0)) { // Estado 0
                curMove = new Vector2Int(1, 2);

            } else if (curMove == new Vector2Int(1, 0)) { // Estado 1
                curMove = new Vector2Int(2, 1);

            } else if (curMove == new Vector2Int(0, 1)) { // Estado 2
                curMove = new Vector2Int(1, 2);
            } // Nos estados 3 e 4, curMove fica igual

        } else if (x) {
            curMove = new Vector2Int(1, 0);
        } else if (y) {
            curMove = new Vector2Int(0, 1);
        } else {
            curMove = new Vector2Int(0, 0);
        }
    }

    // Define qual será o movimento realizado pelo boneco de acordo com o input e a posição atual
    void calculateMovement(string dir, bool closeToBorder) {
        Debug.Log(curMove);
        float moveConst = Time.deltaTime * 0.5f * speed; // BETA. 

        if (!closeToBorder) {
            if (dir == "Vertical") {
                float dif = transform.position.x - gc.centerPosition(transform.position).x; // Vê o quão diferente tá o x

                // Vê se o range exige ajuste no movimento, ou set ou nada -q
                if(Mathf.Abs(dif) > 0.1) {
                    transform.Translate(Mathf.Sign(dif) * -1 * moveConst, Mathf.Sign(Input.GetAxis(dir)) * moveConst , 0);

                } else if (Mathf.Abs(dif) > 0) {
                    transform.position = new Vector2(gc.centerPosition(transform.position).x, transform.position.y);
                    transform.Translate(0, Mathf.Sign(Input.GetAxis(dir)) * moveConst, 0);
                } else {
                    transform.Translate(0, Mathf.Sign(Input.GetAxis(dir)) * moveConst, 0);
                }

            } else if (dir == "Horizontal") {
                // Right // Left
                float dif = transform.position.y - gc.centerPosition(transform.position).y; // Vê o quão diferente tá o y

                if (Mathf.Abs(dif) > 0.1) {
                    transform.Translate(Mathf.Sign(Input.GetAxis(dir)) * moveConst, Mathf.Sign(dif) * -1 * moveConst, 0);

                } else if (Mathf.Abs(dif) > 0) {
                    transform.position = new Vector2(transform.position.x, gc.centerPosition(transform.position).y);
                    transform.Translate(Mathf.Sign(Input.GetAxis(dir)) * moveConst, 0, 0);
                } else {
                    transform.Translate(Mathf.Sign(Input.GetAxis(dir)) * moveConst, 0, 0);
                }         
            }

        } else {
            
        }
    }

    // Criação de bombas
    void placeBomb() {       
        if (BombsUsed < bombsMax && GridController.instance.tileMainContent(transform.position) == null) {
            BombsUsed++;
            Bomb b = Instantiate(Resources.Load<Bomb>("prefabs/Bomb"));
            b.setup(this);
        }
    }

    // Verifica se há alguma colisão que impede o movimento pretendido pelo boneco.
    bool possibleMove(Vector2 dir, out bool border) {
        // Cria raycast a partir de (x,y), na direção dir com distância de uma tile
        float x = transform.position.x;
        float y = transform.position.y;
        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(x, y), dir, 1);
        border = false; // BETA

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
