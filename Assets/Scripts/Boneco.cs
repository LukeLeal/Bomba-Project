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
    int speed; // Velocidade de movimento do boneco (inutilizado atm)
    bool kick;
    bool punch;
    bool hold;

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

        if (Input.GetKeyDown(KeyCode.UpArrow)) {

            // Verifica se o move é possível. Se for, faz. Se não, fica de boa.
            if (possibleMove(Vector2.up)) {
                transform.position = new Vector2(transform.position.x, transform.position.y + 1);
                transform.position = gc.centerPosition(transform.position);
                //Debug.Log("cima");
            }

        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {

            if (possibleMove(Vector2.down)) {
                transform.position = new Vector2(transform.position.x, transform.position.y - 1);
                transform.position = gc.centerPosition(transform.position);
                //Debug.Log("baixo");
            }

        } else if (Input.GetKeyDown(KeyCode.RightArrow)) {

            if (possibleMove(Vector2.right)){
                transform.position = new Vector2(transform.position.x + 1, transform.position.y);
                transform.position = gc.centerPosition(transform.position);
                //Debug.Log("direita");
            }

        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {

            if (possibleMove(Vector2.left)) {
                transform.position = new Vector2(transform.position.x - 1, transform.position.y);
                transform.position = gc.centerPosition(transform.position);
                //Debug.Log("esquerda");
            }
        }
        #endregion

        if (Input.GetKeyDown(KeyCode.Z)) {
            placeBomb();
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
    bool possibleMove(Vector2 dir) {
        // Cria raycast a partir de (x,y), na direção dir com distância de uma tile
        float x = transform.position.x;
        float y = transform.position.y;
        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(x, y), dir, 1);

        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.gameObject == gameObject || // Ignora o raycasthit do próprio collider. 
                hit.collider.gameObject.tag == "Explosion" || // Pode ir onde tem explosão. Só que morre nisso. Hue
                (hit.collider.gameObject.tag == "Bomb" && gc.centerPosition(hit.point) == new Vector2(x, y))) {
                // Atravessa colisão apenas se for uma bomba e estiver "dentro" dela. 

                // ATENÇÃO (05/12/17): O da bomba vai dar ruim quando o movimento do boneco ficar dinâmico.
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
}
