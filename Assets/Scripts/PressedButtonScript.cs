using UnityEngine;

public class PressedButtonScript : MonoBehaviour {

	public int i;
	public int j;

	void OnMouseDown () {
		SoundManagerScript.PlaySound ("pressButton");
		gameObject.GetComponent<Animation> ().Play ();
	}
}