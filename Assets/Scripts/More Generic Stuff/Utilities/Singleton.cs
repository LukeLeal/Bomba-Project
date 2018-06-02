using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe que implementa o padrão singleton em quem herdá-la.
/// </summary>
/// <typeparam name="T">Tipo genérico</typeparam>
public class Singleton<T> : MonoBehaviour where T : Component {
    private static T instance;

    /// <summary>
    /// Se o singleton deve ser preservado entre as cenas (aka DontDestroyOnLoad)
    /// </summary>
    public bool preserved;

    public static T Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<T>();
                if (instance == null) {
                    // Cria um objeto com T se ele não estiver na cena
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    instance = obj.AddComponent<T>();
                }
            }
            return instance;
        }
    }

    public virtual void Awake() {
        if (instance == null) {
            instance = this as T;
            if (preserved) {
                DontDestroyOnLoad(this.gameObject);
            }
        } else {
            Destroy(gameObject);
        }
    }
}

// Baseado em: http://www.unitygeek.com/unity_c_singleton/ 
