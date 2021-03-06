﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System;
using Facebook.Unity;
using GameSparks.Core;
using System.Collections.Generic;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour {

	public GameObject loseModal;
	public GameObject victoryModal;
	public GameObject pauseModal;
	public GameObject pauseButton;
	public GameObject clockPanel;
    public GameObject loadingMini;

    //[HideInInspector]
    public GameObject gameSparksManager;

	public Text victoryClockText;
	public ClockTime clockTime;
	public Button loseCheckpointButton;
	public Button pauseCheckponintButton;

    public Text energyText;
    public Text stageText;
    public Toggle rankToggle;

	[Tooltip("Insira o numero de checkpoint e sete eles na posição e ordem correta")]
	public GameObject[] checkpoints;

    public GameObject[] portas;

    public AudioMixerSnapshot paused;
    public AudioMixerSnapshot unpaused;

    public AudioMixer masterMixer;

    private int perderVidaFlag;
    private int powercells;

	void Awake () {
        gameSparksManager = GameObject.Find("GameSparks Manager");

        if (gameSparksManager == null)
        {
            gameSparksManager = new GameObject();
            gameSparksManager.name = "GameSparks Manager";
            gameSparksManager.AddComponent<GameSparksUnity>();
            gameSparksManager.AddComponent<GameSparksManager>();
            gameSparksManager.AddComponent<UserManager>();
            gameSparksManager.AddComponent<EnergyTimeValues>();
            gameSparksManager.AddComponent<ModoOffline>();
            gameSparksManager.AddComponent<GooglePlayOrbe>();
        }

		startAll ();
		verifyCheckpoint ();

        PlayerPrefs.SetInt("MainMenuOff", 1);

        energyText.text = PlayerPrefs.GetInt("Vidas").ToString();
        StartCoroutine("CarregarVida");
	}

    void Start() {
        for (int i = 0; i < portas.Length; i++) {
            if(PlayerPrefs.HasKey("Porta" + portas[i].name)){
                Debug.Log("Porta " + portas[i].name + " deveria estar aberta");
                if (PlayerPrefs.GetInt("Porta" + portas[i].name) == 0) {
                    portas[i].GetComponent<Animator>().SetInteger("Cor", i);
                    StartCoroutine("EsperarSegundosAbrirPorta", i);
                    //portas[i].GetComponent<Animator>().SetBool("Aberta", true);
                }
            }
        }
    }

    IEnumerator EsperarSegundosAbrirPorta(int i)
    {
        yield return new WaitForSeconds(.1f);
        portas[i].GetComponent<Animator>().SetBool("Aberta", true);
        portas[i].GetComponent<BoxCollider2D>().enabled = false;
    }

    private void CarregarTutorial()
    {
        if (SceneManager.GetActiveScene().name.Equals("TutorialScene"))
        {
            stageText.text = "Tutorial";
        }
        else
        {
            stageText.text = "Fase " + SceneManager.GetActiveScene().name;
            //stageText.text = "Stage " + SceneManager.GetActiveScene().name;
        }

        masterMixer.SetFloat("sfxVol", PlayerPrefs.GetFloat("sfxVol"));
        masterMixer.SetFloat("musicVol", PlayerPrefs.GetFloat("musicVol"));

        GameSparks.Api.Messages.NewHighScoreMessage.Listener += HighScoreMessageHandler; // assign the New High Score message
    }

    private void BloquearBotoes()
    {
        //Debug.Log("Chamando BloquearBotoes " + energyText.text);
        if (int.Parse(energyText.text) <= 1)
        {
            //Debug.Log(pauseModal.transform.GetChild(0).GetChild(2).name);
            pauseModal.transform.GetChild(0).GetChild(2).gameObject.GetComponent<Button>().interactable = false;
        }
    }

    void HighScoreMessageHandler(GameSparks.Api.Messages.NewHighScoreMessage _message)
    {
        //Debug.Log("NEW TIME RECORD\n " + _message.LeaderboardName);
    }

	// Update is called once per frame
	void Update () {
        if (gameSparksManager.GetComponent<EnergyTimeValues>().getVidas() >= 5)
        {
            gameSparksManager.GetComponent<EnergyTimeValues>().PararCountdownTimer();
        }
	}

	public void loseGame () {
		stopAll ();
		showLoseModal ();

        perderVidaFlag = 0;
        StartCoroutine("PerderVida");
	}

	public void victoryGame () {
        if (SceneManager.GetActiveScene().name.Equals("TutorialScene"))
        {
            stopAll();
            verifyTutorialDone();
            loadingMini.SetActive(true);
            StartCoroutine("LoadNewScene", "StageSelect");
            //SceneManager.LoadScene("StageSelect");
        }
        else
        {
            stopAll();
            showVictoryModal();
            showClockTime();
            hidePauseButton();
            saveTime();
            verifyTutorialDone();
            resetCheckpoint();

            if (!SceneManager.GetActiveScene().name.Equals("8-10"))
            {
                SalvarFases();
            }

            verifyGoogleAchievements();
        }
	}

    private void verifyGoogleAchievements()
    {
        powercells = gameSparksManager.GetComponent<EnergyTimeValues>().getPowercells();

        if (PlayerPrefs.GetInt("Fases") >= 11)
        {
            gameSparksManager.GetComponent<GooglePlayOrbe>().UnlockAchievementClearWorld1();
            gameSparksManager.GetComponent<EnergyTimeValues>().SavePowercells(powercells + 5);
        }

        if (PlayerPrefs.GetInt("Fases") >= 21)
        {
            gameSparksManager.GetComponent<GooglePlayOrbe>().UnlockAchievementClearWorld2();
            gameSparksManager.GetComponent<EnergyTimeValues>().SavePowercells(powercells + 5);
        }

        if (PlayerPrefs.GetInt("Fases") >= 31)
        {
            gameSparksManager.GetComponent<GooglePlayOrbe>().UnlockAchievementClearWorld3();
            gameSparksManager.GetComponent<EnergyTimeValues>().SavePowercells(powercells + 5);
        }

        if (PlayerPrefs.GetInt("Fases") >= 41)
        {
            gameSparksManager.GetComponent<GooglePlayOrbe>().UnlockAchievementClearWorld4();
            gameSparksManager.GetComponent<EnergyTimeValues>().SavePowercells(powercells + 5);
        }

        if (PlayerPrefs.GetInt("Fases") >= 51)
        {
            gameSparksManager.GetComponent<GooglePlayOrbe>().UnlockAchievementClearWorld5();
            gameSparksManager.GetComponent<EnergyTimeValues>().SavePowercells(powercells + 10);
        }

        if (PlayerPrefs.GetInt("Fases") >= 61)
        {
            gameSparksManager.GetComponent<GooglePlayOrbe>().UnlockAchievementClearWorld6();
            gameSparksManager.GetComponent<EnergyTimeValues>().SavePowercells(powercells + 10);
        }

        if (PlayerPrefs.GetInt("Fases") >= 71)
        {
            gameSparksManager.GetComponent<GooglePlayOrbe>().UnlockAchievementClearWorld7();
            gameSparksManager.GetComponent<EnergyTimeValues>().SavePowercells(powercells + 15);
        }

        if (SceneManager.GetActiveScene().name.Equals("8-10"))
        {
            gameSparksManager.GetComponent<GooglePlayOrbe>().UnlockAchievementClearWorld8();
            gameSparksManager.GetComponent<EnergyTimeValues>().SavePowercells(powercells + 20);
        }

        if (SceneManager.GetActiveScene().name.Equals("1-1") && ClockTimeInt() <= 6589)
        {
            gameSparksManager.GetComponent<GooglePlayOrbe>().UnlockAchievementStage1_1Time();
            gameSparksManager.GetComponent<EnergyTimeValues>().SavePowercells(powercells + 5);
        }

        if (SceneManager.GetActiveScene().name.Equals("2-9") && ClockTimeInt() <= 14000)
        {
            gameSparksManager.GetComponent<GooglePlayOrbe>().UnlockAchievementStage2_9Time();
            gameSparksManager.GetComponent<EnergyTimeValues>().SavePowercells(powercells + 5);
        }

        if (SceneManager.GetActiveScene().name.Equals("3-5") && ClockTimeInt() <= 12799)
        {
            gameSparksManager.GetComponent<GooglePlayOrbe>().UnlockAchievementStage3_5Time();
            gameSparksManager.GetComponent<EnergyTimeValues>().SavePowercells(powercells + 5);
        }

        if (SceneManager.GetActiveScene().name.Equals("4-3") && ClockTimeInt() <= 16733)
        {
            gameSparksManager.GetComponent<GooglePlayOrbe>().UnlockAchievementStage4_3Time();
            gameSparksManager.GetComponent<EnergyTimeValues>().SavePowercells(powercells + 5);
        }

        if (SceneManager.GetActiveScene().name.Equals("5-4") && ClockTimeInt() <= 12000)
        {
            gameSparksManager.GetComponent<GooglePlayOrbe>().UnlockAchievementStage5_4Time();
            gameSparksManager.GetComponent<EnergyTimeValues>().SavePowercells(powercells + 5);
        }

        if (SceneManager.GetActiveScene().name.Equals("6-7") && ClockTimeInt() <= 210787)
        {
            gameSparksManager.GetComponent<GooglePlayOrbe>().UnlockAchievementStage6_7Time();
            gameSparksManager.GetComponent<EnergyTimeValues>().SavePowercells(powercells + 10);
        }

        if (SceneManager.GetActiveScene().name.Equals("7-9") && ClockTimeInt() <= 147830)
        {
            gameSparksManager.GetComponent<GooglePlayOrbe>().UnlockAchievementStage7_9Time();
            gameSparksManager.GetComponent<EnergyTimeValues>().SavePowercells(powercells + 20);
        }

        if (SceneManager.GetActiveScene().name.Equals("8-10") && ClockTimeInt() <= 935512)
        {
            gameSparksManager.GetComponent<GooglePlayOrbe>().UnlockAchievementStage8_10Time();
            gameSparksManager.GetComponent<EnergyTimeValues>().SavePowercells(powercells + 20);
        }

    }

    void SalvarFases()
    {
        string[] m = SceneManager.GetActiveScene().name.Split('-');
        string n = m[0];
        string o = m[1];
        int q = (int.Parse(n) - 1) * 10 + (int.Parse(o) + 1);
        string p = q.ToString();

        if (PlayerPrefs.GetInt("Fases") <= int.Parse(p))
        {
            PlayerPrefs.SetInt("Fases", int.Parse(p));
        }
    }

	void showClockTime () {
		victoryClockText.text = clockTime.timestring;
		clockPanel.SetActive (false);
	}

	void showVictoryModal () {
		if (victoryModal) {
			victoryModal.SetActive (true);
		}
	}

    // The coroutine runs on its own at the same time as Update() and takes an integer indicating which scene to load.
    IEnumerator LoadNewScene(string levelName)
    {
        clockTime.stopTime();
        Time.timeScale = 1;
        // This line waits for 1 seconds before executing the next line in the coroutine.
        // This line is only necessary for this demo. The scenes are so simple that they load too fast to read the "Loading..." text.
        yield return new WaitForSeconds(1);

        // Start an asynchronous operation to load the scene that was passed to the LoadNewScene coroutine.
        AsyncOperation async = SceneManager.LoadSceneAsync(levelName);

        // While the asynchronous operation to load the new scene is not yet complete, continue waiting until it's done.
        while (!async.isDone)
        {
            yield return null;
        }
    }

    IEnumerator LoadNewScene(int levelName)
    {
        clockTime.stopTime();
        Time.timeScale = 1;
        // This line waits for 1 seconds before executing the next line in the coroutine.
        // This line is only necessary for this demo. The scenes are so simple that they load too fast to read the "Loading..." text.
        yield return new WaitForSeconds(1);

        // Start an asynchronous operation to load the scene that was passed to the LoadNewScene coroutine.
        AsyncOperation async = SceneManager.LoadSceneAsync(levelName);

        // While the asynchronous operation to load the new scene is not yet complete, continue waiting until it's done.
        while (!async.isDone)
        {
            yield return null;
        }
    }

	public void goToStageSelectScene () {
		resetCheckpoint ();
        if (!clockTime.timestring.Equals("00:00:000") && !victoryModal.activeInHierarchy && !loseModal.activeInHierarchy)
        {
            perderVidaFlag = 1;
            StartCoroutine("PerderVida");
        }else{
            loadingMini.SetActive(true);
            StartCoroutine("LoadNewScene", "StageSelect");
            //SceneManager.LoadScene("StageSelect");
        }
	}

	public void goToNextScene () {
		try {
			resetCheckpoint ();
            loadingMini.SetActive(true);
            StartCoroutine("LoadNewScene", SceneManager.GetActiveScene().buildIndex + 1);
			//SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex + 1);
		}
		catch (System.Exception e) {
			//Debug.Log (e.ToString());
			goToStageSelectScene ();
		}
	}

	public void restart () {
        if (!clockTime.timestring.Equals("00:00:000") && !victoryModal.activeInHierarchy && !loseModal.activeInHierarchy)
        {
            perderVidaFlag = 2;
            StartCoroutine("PerderVida");
        }
        else
        {
            loadingMini.SetActive(true);
            StartCoroutine("LoadNewScene", SceneManager.GetActiveScene().name);
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
	}

	void saveTime () {
		//Debug.Log ("Venci - " + clockTime.timestring + " -- " + clockTime.timer);

        // Transforma o tempo em int
        // int teste2 = ClockTimeInt();
        string[] m1 = SceneManager.GetActiveScene().name.Split('-');
        PlayerPrefs.SetInt("LEADERBOARD_" + m1[0] + "_" + m1[1], ClockTimeInt());
        //Debug.Log(teste2);
        //Debug.Log("CT: " + clockTime.RetornaTempoString(teste2));

        StartCoroutine("SalvarTempo");
        /*if (!gameSparksManager.GetComponent<ModoOffline>().getModoOffline())
        {
            SalvarTempoGameSparks();
        }*/
	}

    private int ClockTimeInt() {
        return int.Parse(clockTime.timestring[0].ToString() +
                         clockTime.timestring[1].ToString() +
                         clockTime.timestring[3].ToString() +
                         clockTime.timestring[4].ToString() +
                         clockTime.timestring[6].ToString() +
                         clockTime.timestring[7].ToString() +
                         clockTime.timestring[8].ToString());
    }

    // Dado o nome da fase no formato "X-Y", retorna a posição no vetor GameSparksManager.records[]
    private int PosicaoVetorRecords(string mundo) {
        string[] m1 = mundo.Split('-'); 
        
        int m2 = int.Parse(m1[0]) - 1;
        int m3 = int.Parse(m1[1]) - 1;

        return int.Parse((m2.ToString() + m3.ToString()));
    }

    // Salva tempo no GS pro Leaderboard da fase que está jogando
    void SalvarTempoGameSparks()
    {
        // m1[0] = String da cena antes do '-' ; m1[1] = String da cena depois do '-' ; 
        string[] m1 = SceneManager.GetActiveScene().name.Split('-');
        rankToggle.isOn = true;

        // Se ClockTimeInt() < records[], salva
        if (ClockTimeInt() < GameSparksManager.records[PosicaoVetorRecords(SceneManager.GetActiveScene().name)])
        {
            GameSparksManager.records[PosicaoVetorRecords(SceneManager.GetActiveScene().name)] = ClockTimeInt();

            // mudar no gs para ficar 1-1 e assim usar o nome da cena concatenado
            new GameSparks.Api.Requests.LogEventRequest()
                .SetEventKey("SAVE_STAGE_" + m1[0] + "_" + m1[1])
                .SetEventAttribute("TIME_" + m1[0] + "_" + m1[1], ClockTimeInt())
                .SetDurable(true)
                .Send((response) =>
                {

                    if (!response.HasErrors)
                    {
                        //Debug.Log("Player Saved To GameSparks...");
                        //Debug.Log("Score Posted Sucessfully...");
                        PlayerPrefs.DeleteKey("LEADERBOARD_" + m1[0] + "_" + m1[1]);
                    }
                    else
                    {
                        //Debug.Log("Error Saving Player Data...");
                        rankToggle.isOn = false;
                    }
                });
        }
    }

	void showLoseModal () {
		if (loseModal) {
			loseModal.SetActive (true);
		}
	}

	public void pauseGame () {
		stopAll ();
		hidePauseButton ();
		showPauseModal ();

        Lowpass();

        if ((int.Parse(energyText.text)) <= 0)
        {
            pauseModal.transform.GetChild(0).GetChild(1).GetComponent<Button>().interactable = false;
        }
	}

	void hidePauseButton () {
		pauseButton.SetActive (false);
	}

	void showPauseButton () {
		pauseButton.SetActive (true);
	}
		
	public void unpauseGame () {
		startAll ();
		hidePauseModal ();
		showPauseButton ();
        Lowpass();
	}

	void showPauseModal () {
		pauseModal.SetActive (true);
	}

	void hidePauseModal () {
		pauseModal.SetActive (false);
	}
		
	void stopAll () {
		Time.timeScale = 0;
	}

	void startAll () {
		Time.timeScale = 1;
	}

	void verifyTutorialDone () {
		if (PlayerPrefs.GetInt ("tutorialDone") == 0) {
			PlayerPrefs.SetInt ("tutorialDone", 1);
		}
	}

	public void checkpoint (int order) {
		PlayerPrefs.SetInt ("checkpointOrder", order);
		PlayerPrefs.SetFloat ("checkpointTimer", clockTime.timer);
		PlayerPrefs.SetInt ("hasCheckpoint", 1);
		activeCheckpointButton ();
	}

	void verifyCheckpoint () {
		if (PlayerPrefs.HasKey("startInCheckpoint") 
			&& PlayerPrefs.GetInt ("startInCheckpoint") == 1
			&& PlayerPrefs.HasKey ("hasCheckpoint") 
			&& PlayerPrefs.GetInt ("hasCheckpoint") == 1) {
			setRobotToCheckpoint ();
			setCheckpointTimer ();
			activeCheckpointButton ();
		}
	}

	void activeCheckpointButton () {
		loseCheckpointButton.interactable = true;
		pauseCheckponintButton.interactable = true;
	}

	void setRobotToCheckpoint () {
		GameObject checkpointField = checkpoints[PlayerPrefs.GetInt ("checkpointOrder")] ;
		GameObject.Find ("Robot").SendMessage ("goToField", checkpointField);
	}

	void setCheckpointTimer () {
		clockTime.statTimeCheckpoint (PlayerPrefs.GetFloat ("checkpointTimer"));
	}

	public void restartCheckpoint () {
		PlayerPrefs.SetInt("startInCheckpoint", 1);
		restart ();
	}

	public void restartWithoutCheckpoint () {
		resetCheckpoint ();
		restart ();
	}

	void resetCheckpoint () {
		PlayerPrefs.SetInt("startInCheckpoint", 0); 

        // Reseta portas
        for (int i = 0; i < portas.Length; i++) {
            if (PlayerPrefs.HasKey("Porta" + portas[i].name)) { 
                PlayerPrefs.DeleteKey("Porta" + portas[i].name);
            }
        }
	}

	void OnApplicationQuit() {
		resetCheckpoint ();
	}

    void Lowpass()
    {
        if (Time.timeScale == 0)
        {
            paused.TransitionTo(.01f);
        }
        else
        {
            unpaused.TransitionTo(.01f);
        }
    }

    // Carrega o valor da vida
    private void LoadLife()
    {
        new GameSparks.Api.Requests.LogEventRequest()
            .SetEventKey("RETRIEVE_RECORDS")
                .Send((response) =>
                {

                    if (!response.HasErrors)
                    {
                        GSData data = response.ScriptData.GetGSData("player_Data");
                        energyText.text = data.GetInt("life").ToString();
                        BloquearBotoes();

                        //Debug.Log("Recieved Player Life Data From GameSparks...");
                    }
                    else
                    {
                        //Debug.Log("Error Loading Player Data...");
                    }
                });

    }

    private void LoseLife(int vida) {
        // Tira uma vida em jogo
        energyText.text = vida.ToString();
        PlayerPrefs.SetInt("Vidas", vida);
        gameSparksManager.GetComponent<EnergyTimeValues>().setVidas(vida);

        // Tira uma vida no GS
        new GameSparks.Api.Requests.LogEventRequest()
            .SetEventKey("SAVE_LIFES")
            .SetEventAttribute("LIFE", vida)
            .SetDurable(true)
            .Send((response) =>
            {

                if (!response.HasErrors)
                {
                    //Debug.Log("Perdeu uma vida...");

                    if ((int.Parse(energyText.text)) <= 0)
                    {
                        //Debug.Log("Vida <= 0, não pode jogar");
                    }
                    else if ((int.Parse(energyText.text)) == 4)
                    {
                        PlayerPrefs.SetString("DateTime", System.DateTime.Now.ToString());
                        gameSparksManager.GetComponent<EnergyTimeValues>().IniciarCountdownTimer();
                    }

                }
                else
                {
                    //Debug.Log("Error Saving Player Data...");
                }
            });
    }

    IEnumerator CarregarVida()
    {
        // Chama o teste de conexão em Modo Offline
        gameSparksManager.GetComponent<ModoOffline>().TestarConexao();

        // Aguarda até terminar o teste
        yield return new WaitUntil(() => gameSparksManager.GetComponent<ModoOffline>().getTestandoConexao() == false);

        // Age de acordo com o resultado, offline ou online
        if (gameSparksManager.GetComponent<ModoOffline>().getModoOffline())
        {
            //Debug.Log("Acao - Offline");
            energyText.text = PlayerPrefs.GetInt("Vidas").ToString();
        }
        else
        {
            //Debug.Log("Acao - Online");
            LoadLife();

            if(int.Parse(energyText.text) != PlayerPrefs.GetInt("Vidas"))
                energyText.text = PlayerPrefs.GetInt("Vidas").ToString();
        }


        CarregarTutorial();
        BloquearBotoes();
    }

    IEnumerator PerderVida()
    {
        energyText.text = (int.Parse(energyText.text) - 1).ToString();
        PlayerPrefs.SetInt("Vidas", int.Parse(energyText.text));
        gameSparksManager.GetComponent<EnergyTimeValues>().setVidas(int.Parse(energyText.text));

        // Chama o teste de conexão em ModoOffline
        gameSparksManager.GetComponent<ModoOffline>().TestarConexao();

        // Aguarda até terminar o teste
        yield return new WaitUntil(() => gameSparksManager.GetComponent<ModoOffline>().getTestandoConexao() == false);

        // Age de acordo com o resultado, offline ou online
        if (gameSparksManager.GetComponent<ModoOffline>().getModoOffline())
        {
            //Debug.Log("Acao - Offline - PerderVida");
            // PlayerPrefs.SetInt("Vidas", int.Parse(energyText.text));
        }
        else
        {
            //Debug.Log("Acao - Online");
            LoseLife(int.Parse(energyText.text));
        }

        if ((int.Parse(energyText.text)) <= 0)
        {
            loseModal.transform.GetChild(0).GetChild(1).GetComponent<Button>().interactable = false;
            loseModal.transform.GetChild(0).GetChild(2).GetComponent<Button>().interactable = false;
        }

        if (perderVidaFlag == 1)
        {
            //SceneManager.LoadScene("StageSelect");
            loadingMini.SetActive(true);
            StartCoroutine("LoadNewScene", "StageSelect");
        }
        else if (perderVidaFlag == 2)
        {
            loadingMini.SetActive(true);
            StartCoroutine("LoadNewScene", SceneManager.GetActiveScene().name);
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

    }

    IEnumerator SalvarTempo()
    {
        // Chama o teste de conexão em ModoOffline
        gameSparksManager.GetComponent<ModoOffline>().TestarConexao();

        // Aguarda até terminar o teste
        yield return new WaitUntil(() => gameSparksManager.GetComponent<ModoOffline>().getTestandoConexao() == false);

        // Age de acordo com o resultado, offline ou online
        if (gameSparksManager.GetComponent<ModoOffline>().getModoOffline())
        {
            //Debug.Log("Acao - Offline");
            rankToggle.isOn = false;
            // Avisar que está offline e não vai salvar o tempo?
        }
        else
        {
            //Debug.Log("Acao - Online");
            SalvarTempoGameSparks();
        }
    }

    public void CompartilharTempoFB()
    {
        FB.FeedShare(string.Empty, 
                      new Uri("https://play.google.com/apps/testing/com.SDGStudio.Orb_e"), 
                     "Record Orb-E", 
                     "Fase " + SceneManager.GetActiveScene().name, 
                     "Eu fiz o tempo de " + clockTime.timestring + "! Consegue superar?",
                     new Uri("https://scontent.fgig1-4.fna.fbcdn.net/v/t1.0-9/14089323_225201597881813_4306982641321836974_n.jpg?oh=315466b6073dd254269e0b908c6f2a84&oe=585E9E9D"),
                     string.Empty,
                     ShareCallBack);
    }

    void ShareCallBack(IResult result)
    {
        if (result.Cancelled)
        {
            //Debug.Log("Success cancelled!");
        }
        else if(!string.IsNullOrEmpty(result.Error))
        {
            //Debug.Log("Error on share!");
        }
        else if (!string.IsNullOrEmpty(result.RawResult))
        {
            //Debug.Log("Success on share!");
        }
    }
}
