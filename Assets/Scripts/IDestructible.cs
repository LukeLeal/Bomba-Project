using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IDestructible é um gameObject cujo outro gameObject pode forçar sua destruição diretamente (sem uso de colliders)
/// </summary>
/// Por exemplo: 
///     Item I está no alcance de explosão da Bomba B; 
///     B nota que I é destrutível;
///     B manda I executar sua auto-destruição.
///     I começa o seu processo customizado de auto-destruição / explosão / whatever seja o nome 
///     
/// Nome de classe e método não finais :P
public interface IDestructible {

    /// <summary>
    /// Força a destruição do objeto.
    /// </summary>
    /// <param name="position"> Posição onde a destruição deve ocorrer (uso não obrigatório). </param>
    void forceDestruction(Vector2 position);

    GameObject gameObject {
        get;
    }
}
