using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<Instance> : MonoBehaviour where Instance : Singleton<Instance> {
    public static Instance instance;
    public bool isPersistant;

    /// <summary>
    /// Awake this instance.
    /// </summary>
    public virtual void Awake() {
        if (isPersistant) {
            // Singleton persistente entre as sessões de jogo

            if (!instance) {
                instance = this as Instance;
            } else {
                DestroyObject(gameObject);
            }
            DontDestroyOnLoad(gameObject);

        } else {
            // Singleton simples
            instance = this as Instance;
        }
    }
}
