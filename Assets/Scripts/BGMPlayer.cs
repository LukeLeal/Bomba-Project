using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMPlayer : MonoBehaviour {

    AudioSource source; // AudioSource do objeto. Responsável por tocar os sons.

    // Use this for initialization
    void Start () {
        if (source == null) {
            source = gameObject.GetComponent<AudioSource>();
        }
    }
	
	// Update is called once per frame
	void Update () {
        // Loop BETA - BattleTheme 1
        if(source.timeSamples >= 4018865) {
            source.timeSamples = 817152;
        }
		
	}
}
