﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Classe responsável pelos métodos e dados do boneco
/// </summary>
public class Boneco : MonoBehaviour {

    GridController gc;

    //  shortcut transform.position #sdds

    
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

    }
}
