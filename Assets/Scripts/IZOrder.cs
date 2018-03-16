using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IZOrder {

    /* Interface a ser usada por todos os objetos que estão presentes fisicamente no tabuleiro.
     * ZOrder é usado no tratamento de certas colisões. Deve se manter igual ao "SortingOrder" do renderer.
     * Constantes estão listadas no GridController.
     * */

    int ZOrder {
        get;
    }

    GameObject gameObject {
        get;
    }

}
