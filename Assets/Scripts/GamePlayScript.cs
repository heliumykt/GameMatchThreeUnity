using UnityEngine;
using UnityEngine.UI;

public class GamePlayScript : MonoBehaviour {

	public GameObject prefabFigure;

	public int matrixHeight = 15;
	public int matrixWidth = 15;
	public int noLessPR = 20;
	public int numFigures = 5;
	public int chancePR = 3;
	public int afterSecondsHint = 10;
	public int target = 50;

	private int[, ] fieldMatrix;
	private int[, ] removalMatrix;
	private GameObject[, ] objsFigures;

	private float sizeFigure = 0.3f;
	private float timeCount;

	private Vector3 startPositionMouse;
	private Vector3 positionDeviationMouse;

	private Ray ray;
	private RaycastHit hit;

	private int i_Button;
	private int j_Button;

	public Text scoreText;
	private int scoreGame;

	public GameObject finishGame;
	private bool endGame = true;
	public GameObject pauseButton;

	// Use this for initialization
	void Start () {
		fieldMatrix = new int[matrixHeight, matrixWidth];
		removalMatrix = new int[matrixHeight, matrixWidth];
		objsFigures = new GameObject[matrixHeight, matrixWidth];

		mixField ();
		TransferMatrixToScreen ();
	}

	// Update is called once per frame
	void Update () {
		timeCount += Time.deltaTime;
		if (timeCount > afterSecondsHint) {
			ShowHint ();
			timeCount = 0f;
		}

		if (DestroyMatchThree (fieldMatrix, removalMatrix, matrixHeight, matrixWidth)) {
			RaiseZeros (fieldMatrix, matrixHeight, matrixWidth);
			scoreGame += AddFigures (fieldMatrix, matrixHeight, matrixWidth, numFigures, chancePR);
			scoreText.text = "Score: " + scoreGame.ToString ();
			TransferMatrixToScreen ();
		}
		LeftClick ();

		if (scoreGame >= target) {
			{
				if (endGame) {
					endGame = false;
					SoundManagerScript.PlaySound ("applause");
					finishGame.SetActive (true);
					Time.timeScale = 0;
					pauseButton.GetComponent<Button> ().enabled = false;
					gameObject.GetComponent<GamePlayScript> ().enabled = false;
				}
			}
		}
	}

	int CreateRandomNumWithException (int frm, int to, params int[] exc) {
		int amountNums = 0;
		int[] arrayRandom = new int[to - frm + 1];
		int tempFrm = frm;
		bool stopCycle;

		for (int i = 0; i < to - frm + 1; i++) {
			stopCycle = false;
			foreach (int j in exc) {
				if (tempFrm == j) {
					tempFrm++;
					stopCycle = true;
					break;
				}
			}

			if (stopCycle) continue;

			arrayRandom[amountNums] = tempFrm;
			tempFrm++;
			amountNums++;

		}

		return arrayRandom[Random.Range (0, amountNums)];
	}

	bool CheckIndexRange (int[, ] array, int i, int j) {
		if (i >= array.GetLength (0) || i < 0 || j >= array.GetLength (1) || j < 0) {
			return false;
		} else {
			return true;
		}
	}

	bool ItemMatching (int[, ] array, int i, int j, int toI, int toJ) {
		if (CheckIndexRange (array, i, j) && CheckIndexRange (array, toI, toJ)) {
			if (array[i, j] == array[toI, toJ]) {
				return true;
			}

			return false;
		} else {
			return false;
		}

	}

	int?[] CheckPossibleRow (int[, ] array, int i, int j) { // возвращает отклонение i,j {stepI1, stepJ1, steI2, stepJ2}

		if (ItemMatching (array, i, j, i, j + 2)) { // возможные ряды совпадений | через одну | 101 по i
			if (ItemMatching (array, i, j, i + 1, j + 1)) return new int?[] { 0, 2, 1, 1 };
			if (ItemMatching (array, i, j, i - 1, j + 1)) return new int?[] { 0, 2, -1, 1 };
		}

		if (ItemMatching (array, i, j, i + 2, j)) { // возможные ряды совпадений | через одну | 101 по j
			if (ItemMatching (array, i, j, i + 1, j + 1)) return new int?[] { 2, 0, 1, 1 };
			if (ItemMatching (array, i, j, i + 1, j - 1)) return new int?[] { 2, 0, 1, -1 };
		}

		if (ItemMatching (array, i, j, i, j + 1)) { //возможные ряды совпадений | две рядом | 1101 и т.д.по i
			if (ItemMatching (array, i, j, i, j + 3)) return new int?[] { 0, 1, 0, 3 };
			if (ItemMatching (array, i, j, i, j - 2)) return new int?[] { 0, 1, 0, -2 };

			if (ItemMatching (array, i, j, i - 1, j + 2)) return new int?[] { 0, 1, -1, 2 };
			if (ItemMatching (array, i, j, i + 1, j + 2)) return new int?[] { 0, 1, 1, 2 };
			if (ItemMatching (array, i, j, i - 1, j - 1)) return new int?[] { 0, 1, -1, -1 };
			if (ItemMatching (array, i, j, i + 1, j - 1)) return new int?[] { 0, 1, 1, -1 };
		}
		if (ItemMatching (array, i, j, i + 1, j)) { //возможные ряды совпадений | две рядом | 1101 и т.д.по j
			if (ItemMatching (array, i, j, i + 3, j)) return new int?[] { 1, 0, 3, 0 };
			if (ItemMatching (array, i, j, i - 2, j)) return new int?[] { 1, 0, -2, 0 };

			if (ItemMatching (array, i, j, i + 2, j - 1)) return new int?[] { 1, 0, 2, -1 };
			if (ItemMatching (array, i, j, i + 2, j + 1)) return new int?[] { 1, 0, 2, 1 };
			if (ItemMatching (array, i, j, i - 1, j - 1)) return new int?[] { 1, 0, -1, -1 };
			if (ItemMatching (array, i, j, i - 1, j + 1)) return new int?[] { 1, 0, -1, 1 };
		}
		return null;
	}

	int[, ] GenerationField (int[, ] array, int n, int m, int numnumFigures) {
		for (int i = 0; i < n; i++) {
			for (int j = 0; j < m; j++) {
				if (ItemMatching (array, i - 1, j, i - 2, j) && ItemMatching (array, i, j - 1, i, j - 2))
					array[i, j] = CreateRandomNumWithException (1, numFigures, array[i - 1, j], array[i, j - 1]);
				else if (ItemMatching (array, i, j - 1, i, j - 2))
					array[i, j] = CreateRandomNumWithException (1, numFigures, array[i, j - 1]);
				else if (ItemMatching (array, i - 1, j, i - 2, j))
					array[i, j] = CreateRandomNumWithException (1, numFigures, array[i - 1, j]);
				else
					array[i, j] = CreateRandomNumWithException (1, numFigures);
			}
		}
		return array;
	}

	bool DestroyMatchThree (int[, ] array, int[, ] removalMatrix, int n, int m) {
		bool consilience = false;

		for (int i = 0; i < n; i++) {
			for (int j = 0; j < m; j++) {
				if (ItemMatching (array, i, j, i, j - 1) && ItemMatching (array, i, j, i, j - 2)) {
					removalMatrix[i, j] = 1;
					removalMatrix[i, j - 1] = 1;
					removalMatrix[i, j - 2] = 1;
					consilience = true;
				}
				if (ItemMatching (array, i, j, i - 1, j) && ItemMatching (array, i, j, i - 2, j)) {
					removalMatrix[i, j] = 1;
					removalMatrix[i - 1, j] = 1;
					removalMatrix[i - 2, j] = 1;
					consilience = true;
				}
			}
		}

		if (consilience) {
			for (int i = 0; i < n; i++) {
				for (int j = 0; j < m; j++) {
					if (removalMatrix[i, j] == 1) {
						removalMatrix[i, j] = 0;
						array[i, j] = 0;
					}
				}
			}
		}

		return consilience;
	}

	int[, ] RaiseZeros (int[, ] array, int n, int m) {
		for (int i = 0; i < n; i++) {
			for (int j = 0; j < m; j++) {
				if (array[i, j] == 0) {
					for (int row = 0; row < i; row++) {
						array[i - row, j] = array[i - row - 1, j];
						array[i - row - 1, j] = 0;
					}
				}
			}
		}
		return array;
	}
	int AddFigures (int[, ] array, int n, int m, int numFigures, int chancePR) {
		int scoreFigures = 0;

		for (int i = 0; i < n; i++) {
			for (int j = 0; j < m; j++) {
				if (array[i, j] == 0) {
					scoreFigures++;

					for (int f = 1; f <= numFigures; f++) {
						array[i, j] = f;
						if (CheckPossibleRow (array, i, j) != null) {
							if (CreateRandomNumWithException (0, chancePR) == 0) {
								break;
							}
						}
						if (f == numFigures) {
							array[i, j] = CreateRandomNumWithException (1, numFigures);
						}
					}

				}
			}
		}
		return scoreFigures;
	}

	private void mixField () {
		int scorePR = 0;

		while (true) {
			scorePR = 0;
			GenerationField (fieldMatrix, matrixHeight, matrixWidth, numFigures);

			for (int i = 0; i < matrixHeight; i++) {
				for (int j = 0; j < matrixWidth; j++) {
					if (CheckPossibleRow (fieldMatrix, i, j) != null) {
						scorePR++;
					}
				}
			}
			if (scorePR > noLessPR) break;
		}
	}
	private void TransferMatrixToScreen () {
		for (int i = 0; i < matrixHeight; i++) {
			for (int j = 0; j < matrixWidth; j++) {
				Destroy (objsFigures[i, j]);
				objsFigures[i, j] = Instantiate (prefabFigure);

				objsFigures[i, j].GetComponent<Transform> ().localScale = new Vector2 (sizeFigure, sizeFigure);
				objsFigures[i, j].GetComponent<Transform> ().localPosition = new Vector2 (j - matrixWidth / 2, -i + matrixHeight / 2);

				byte rColor = (byte) (fieldMatrix[i, j] * 98 % 255);
				byte gColor = (byte) (fieldMatrix[i, j] * 52 % 255);
				byte bColor = (byte) (fieldMatrix[i, j] * 9 % 255);
				objsFigures[i, j].GetComponent<SpriteRenderer> ().color = new Color32 (rColor, gColor, bColor, 255);

				objsFigures[i, j].GetComponent<PressedButtonScript> ().i = i;
				objsFigures[i, j].GetComponent<PressedButtonScript> ().j = j;
			}
		}
	}
	private void ShowHint () {
		int?[] checkPR;
		bool stopCycles = false;
		while (true) {
			for (int i = 0; i < matrixHeight; i++) {
				for (int j = 0; j < matrixWidth; j++) {
					checkPR = CheckPossibleRow (fieldMatrix, i, j);

					if (checkPR != null) {
						if (CreateRandomNumWithException (0, 4) == 0) {
							objsFigures[i, j].GetComponent<Animation> ().Play ("HintAnim");
							objsFigures[i + checkPR[0].Value, j + checkPR[1].Value].GetComponent<Animation> ().Play ("HintAnim");
							objsFigures[i + checkPR[2].Value, j + checkPR[3].Value].GetComponent<Animation> ().Play ("HintAnim");
							stopCycles = true;
							break;
						}
					}
				}
				if (stopCycles) break;
			}
			if (stopCycles) break;
		}

	}

	private void LeftClick () {
		if (Input.GetMouseButtonDown (0)) {
			startPositionMouse = Input.mousePosition;
		}
		if (Input.GetMouseButtonUp (0)) {

			ray = Camera.main.ScreenPointToRay (startPositionMouse);

			if (Physics.Raycast (ray, out hit)) { //получим нажатый объект и ее индексы на матрице i j
				i_Button = hit.collider.gameObject.GetComponent<PressedButtonScript> ().i;
				j_Button = hit.collider.gameObject.GetComponent<PressedButtonScript> ().j;
			}

			positionDeviationMouse = startPositionMouse - Input.mousePosition; //выясним куда свайпанули

			if (Mathf.Abs (positionDeviationMouse[0]) > Mathf.Abs (positionDeviationMouse[1])) {
				if (positionDeviationMouse[0] < 0) {
					ChangeShapes (i_Button, j_Button + 1); //свайп вправо
				} else {
					ChangeShapes (i_Button, j_Button - 1); //свайп влево
				}
			} else {
				if (positionDeviationMouse[1] < 0) {
					ChangeShapes (i_Button - 1, j_Button); //свайпп вверх
				} else {
					ChangeShapes (i_Button + 1, j_Button); //свайп вниз
				}
			}

		}

		i_Button = matrixHeight;
		j_Button = matrixWidth;
	}

	private void ChangeShapes (int i, int j) {
		if (CheckIndexRange (fieldMatrix, i_Button, j_Button) && CheckIndexRange (fieldMatrix, i, j)) {

			int tempInt = fieldMatrix[i, j];
			fieldMatrix[i, j] = fieldMatrix[i_Button, j_Button];
			fieldMatrix[i_Button, j_Button] = tempInt;

			if (!DestroyMatchThree (fieldMatrix, removalMatrix, matrixHeight, matrixWidth)) {
				fieldMatrix[i_Button, j_Button] = fieldMatrix[i, j];
				fieldMatrix[i, j] = tempInt;
			}
		}
	}



}