using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Classe responsável pelos métodos e dados do boneco
/// </summary>
public class Boneco : MonoBehaviour {

    GridController gc;

    //  shortcut transform.position #sdds

    #region stats
    int firePower; // Tiles além do centro ocupado pela explosão da bomba (min = 1)
    int bombsMax = 10; // Quantidade de bombas do boneco (min = 1)
    int bombsUsed = 0; // Quantidade de bombas em uso (max = bombsMax)
    int speed; 
    bool kick;
    bool punch;
    bool hold;
    
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
            if (gc.possibleMove(transform.position.x, transform.position.y, Vector2.up)) {
                transform.position = new Vector3(transform.position.x, transform.position.y + 1);
                transform.position = gc.centerPosition(transform.position);
                Debug.Log("cima");
            }

        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {

            if (gc.possibleMove(transform.position.x, transform.position.y, Vector2.down)) {
                transform.position = new Vector3(transform.position.x, transform.position.y - 1);
                transform.position = gc.centerPosition(transform.position);
                Debug.Log("baixo");
            }

        } else if (Input.GetKeyDown(KeyCode.RightArrow)) {

            if (gc.possibleMove(transform.position.x, transform.position.y, Vector2.right)){
                transform.position = new Vector3(transform.position.x + 1, transform.position.y);
                transform.position = gc.centerPosition(transform.position);
                Debug.Log("direita");
            }

        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {

            if (gc.possibleMove(transform.position.x, transform.position.y, Vector2.left)) {
                transform.position = new Vector3(transform.position.x - 1, transform.position.y);
                transform.position = gc.centerPosition(transform.position);
                Debug.Log("esquerda");
            }
        }
        #endregion

        if (Input.GetKeyDown(KeyCode.Z)) {
            placeBomb();
        }



    }

    // 
    void placeBomb() {
        // Só pode colocar bomba se tiver alguma disponível
        if(bombsUsed < bombsMax) {
            // Cria a bomba na posição atual
            Bomb b = Instantiate(Resources.Load<Bomb>("prefabs/Bomb"));
            b.setup(gc.centerPosition(transform.position));
        }
    }
}
