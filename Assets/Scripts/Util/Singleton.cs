using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Atenção (19/04/2018): Essa classe e GridController (único singleton atm) provavelmente apreciariam uma revisão mais profunda. hue

public class Singleton<Instance> : MonoBehaviour where Instance : Singleton<Instance> {
    public static Instance instance;
    // public bool isPersistant;

    /// <summary>
    /// Awake this instance.
    /// </summary>
    public virtual void Awake() {
        //if (isPersistant) {
        //    // Singleton persistente entre as sessões de jogo

        //    if (!instance) {
        //        instance = this as Instance;
        //    } else {
        //        DestroyObject(gameObject);
        //    }
        //    DontDestroyOnLoad(gameObject);

        //} else {
        //    // Singleton simples
        //    instance = this as Instance;
        //}

        instance = this as Instance;
    }
}
