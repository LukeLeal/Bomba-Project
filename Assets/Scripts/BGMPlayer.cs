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
        // Loops BETA

        // Super Bomberman 5 - Battle Theme 1
        //if(source.timeSamples >= 4018865) {
        //    source.timeSamples = 817152;
        //}

        // Super Bomberman 4 - Battle Theme
        //if (source.timeSamples >= 2630216) {
        //    source.timeSamples = 114688;
        //}

        // Super Bomberman - Area 1
        if (source.timeSamples >= 3818409) {
            source.timeSamples = 802816;
        }

    }
}
