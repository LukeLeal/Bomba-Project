using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMPlayer : MonoBehaviour {

    string basePath = "Sounds/BGM/";
    AudioSource source; // AudioSource do objeto. Responsável por tocar os sons.
    public int forceMusic; // Musica a ser tocada. -1 se random. Valor definido no editor.

    Tuple<string, int, int> curMusicInfo; // <Name, LoopStart, LoopEnd>
    Tuple<string, int, int>[] musicsInfo = {
        new Tuple<string, int, int>("Super Bomberman - Area 1", 802816, 3818409),
        new Tuple<string, int, int>("Super Bomberman 5 - Battle Theme 1", 817152, 4018865),
        new Tuple<string, int, int>("Super Bomberman 4 - Battle Theme", 0, int.MaxValue), // sdds loop
        new Tuple<string, int, int>("Super Bomberman 4 - Level 1", 0, int.MaxValue),
        new Tuple<string, int, int>("Super Bomberman 5 - Zone 1", 0, int.MaxValue),
        new Tuple<string, int, int>("Super Bomberman 5 - Battle Theme 2", 0, int.MaxValue)
        // new Tuple<string, int, int>("", 0, int.MaxValue),
    };

    void Start () {
        if (source == null) {
            source = gameObject.GetComponent<AudioSource>();
        }
        
        if(forceMusic > 0 && forceMusic < musicsInfo.Length) {
            curMusicInfo = musicsInfo[forceMusic];
        } else {
            curMusicInfo = musicsInfo[Random.Range(0, musicsInfo.Length)];
        }
        
        source.clip = (AudioClip)Resources.Load(basePath + curMusicInfo.item1);
        source.Play();
    }
	
	// Update is called once per frame
	void Update () {

        // Loop BETA
        if (source.timeSamples >= curMusicInfo.item3) {
            source.timeSamples = curMusicInfo.item2;
        }
    }
}
