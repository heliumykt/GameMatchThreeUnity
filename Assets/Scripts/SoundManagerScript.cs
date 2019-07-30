using UnityEngine;

public class SoundManagerScript : MonoBehaviour {

	public static AudioClip pressSound;
	public static AudioClip applauseSound;
	static AudioSource audioScr;
	// Use this for initialization
	void Start () {
		pressSound = Resources.Load<AudioClip> ("pressButton");
		applauseSound = Resources.Load<AudioClip> ("applause");
		audioScr = GetComponent<AudioSource> ();
	}

	public static void PlaySound (string clip) {
		if (clip == "pressButton") {
			audioScr.PlayOneShot (pressSound);
		}
		if(clip == "applause"){
			audioScr.PlayOneShot (applauseSound);
		}
	}
}