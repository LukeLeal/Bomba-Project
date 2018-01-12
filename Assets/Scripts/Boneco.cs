﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Classe responsável pelos métodos e dados do boneco
/// </summary>
public class Boneco : MonoBehaviour, IZOrder {

    GridController gc;

    //  shortcut transform.position #sdds

    int firePower = 6; // Tiles além do centro ocupado pela explosão da bomba (min = 1)
    int bombsMax = 10; // Quantidade de bombas do boneco (min = 1)
    int bombsUsed = 0; // Quantidade de bombas em uso (max = bombsMax)
    int speed = 6; // Velocidade de movimento do boneco
    //bool kick;
    //bool punch;
    //bool hold;
    bool dead = false;
    int zOrder;

    Vector2Int curDir = new Vector2Int();

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
        set { firePower = value; }
    }

    public int BombsUsed {
        get { return bombsUsed; }
        set { bombsUsed = value; }
    }

    public bool Dead {
        get { return dead; }
        set { dead = value; }
    }

    #endregion

    // Use this for initialization
    void Start () {
        gc = GridController.instance;
	}
	
	// Update is called once per frame
	void Update () {

        #region Movement

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

        if (Input.GetKeyDown(KeyCode.Z)){// || Input.GetKeyDown(KeyCode.A)) {
            placeBomb();
        }

        if (Input.GetKeyDown(KeyCode.C)) {
            Debug.Log(GridController.instance.grid.WorldToLocal(transform.position));
        }
    } 

    // Verifica se há alguma colisão que impede o movimento pretendido pelo boneco.
    bool possibleMove(Vector2 dir, out bool obstacle) {
        obstacle = false;

        IZOrder zo = gc.tileMainContent((Vector2)transform.position + dir);
        if (zo != null) {

            if (zo.ZOrder == GridController.ZObjects) {

                // Se já tiver o mais próximo possível do obstáculo, movimento impossível
                if (dir == Vector2.right || dir == Vector2.left) {
                    if(transform.position.x - gc.centerPosition(transform.position).x == 0) {
                        return false; 
                    }
                } else if(dir == Vector2.up || dir == Vector2.down) {
                    if (transform.position.y - gc.centerPosition(transform.position).y == 0) {
                        return false;
                    }
                } else {
                    Debug.Log("PutaVida.exception: Impossible direction");
                }

                obstacle = true;
            }
        }
        return true;
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
        } 
        //else {
        //    // Atm não chega nesse else por causa do resto do código
        //    curDir = new Vector2Int(0, 0);
        //}
    }

    // Define qual será o movimento realizado pelo boneco de acordo com o input e a posição atual.
    // A ideia do movimento no jogo é sempre se manter no centro de um dos eixos da tile (ou ir a ele nas "curvas").
    void calculateMovement(string dir, bool obstacle) {

        float moveConst = Time.deltaTime * speed; // BETA. 

        if (dir == "Vertical") {
            if (obstacle) {
                // Com obstáculo à frente, pode apenas ir até o meio do eixo corrente
                if (Mathf.Abs (transform.position.y - gc.centerPosition(transform.position).y) > 0.1) {
                    transform.Translate(0, Mathf.Sign(Input.GetAxis(dir)) * moveConst, 0);
                } else {
                    transform.position = new Vector2(transform.position.x, gc.centerPosition(transform.position).y);
                }

            } else { // Vertical sem obstáculo
                float dif = transform.position.x - gc.centerPosition(transform.position).x;

                // Dependendo da distância do boneco ao centro do eixo horizontal da tile
                if (Mathf.Abs(dif) > 0.1) {
                    // Move diagonalmente até se aproximar
                    transform.Translate(Mathf.Sign(dif) * -1 * moveConst, Mathf.Sign(Input.GetAxis(dir)) * moveConst, 0);

                } else if (Mathf.Abs(dif) > 0) {
                    // Coloca no centro horizontal e segue o movimento vertical
                    transform.position = new Vector2(gc.centerPosition(transform.position).x, transform.position.y);
                    transform.Translate(0, Mathf.Sign(Input.GetAxis(dir)) * moveConst, 0);

                } else {
                    // Apenas move verticalmente
                    transform.Translate(0, Mathf.Sign(Input.GetAxis(dir)) * moveConst, 0);
                }
            }

        } else if (dir == "Horizontal") {
            if (obstacle) {
                // Com obstáculo à frente, pode apenas ir até o meio do eixo corrente
                if (Mathf.Abs (transform.position.x - gc.centerPosition(transform.position).x) > 0.1) {
                    transform.Translate(Mathf.Sign(Input.GetAxis(dir)) * moveConst, 0, 0);
                } else {
                    transform.position = new Vector2(gc.centerPosition(transform.position).x, transform.position.y);
                }

            } else { // Horizontal sem obstáculo
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
        }
    }

    // Cria bomba no tile atual se possível
    void placeBomb() {       
        if (!dead && BombsUsed < bombsMax && 
            GridController.instance.tileMainContent(transform.position) == null) {
            BombsUsed++;
            Bomb b = Instantiate(Resources.Load<Bomb>("prefabs/Bomb"));
            b.setup(this);
        }
    }

    // BETA
    IEnumerator die() {
        dead = true;
        yield return new WaitForSeconds(2.5f);
        GetComponent<SpriteRenderer>().color = Color.white;
        dead = false;
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.GetComponent<Collider2D>().tag == "Explosion") {
            GetComponent<SpriteRenderer>().color = Color.red;
            StartCoroutine(die());
        }
    }

}
