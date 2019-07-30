using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour {
	public GameObject gamePlay;
	private bool pauseGame = true;
	private bool musicGame = true;

	public void ResetGame () {
		SceneManager.LoadScene ("SampleScene");
	}
	public void PauseGame () {
		if (Time.timeScale != 0) {
			Time.timeScale = 0;
			gamePlay.GetComponent<GamePlayScript> ().enabled = false;
			AudioListener.volume = 0;
			pauseGame = false;
		} else {
			Time.timeScale = 1;
			gamePlay.GetComponent<GamePlayScript> ().enabled = true;
			pauseGame = true;
			if(musicGame) AudioListener.volume = 1;
		}
	}
	public void MusicOff () {
		if (pauseGame) {
			if (AudioListener.volume != 0) {
				AudioListener.volume = 0;
				musicGame = false;
			} else {
				AudioListener.volume = 1;
				musicGame = true;
			}
		}

	}
	public void Quit () {
		Application.Quit ();
	}
}