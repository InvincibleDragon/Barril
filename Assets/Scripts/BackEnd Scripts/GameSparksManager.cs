﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GameSparks.Core;
using System.Collections.Generic;
using System;
using GameSparks.Api.Responses;
using Facebook.Unity;

public class GameSparksManager : MonoBehaviour
{
    //singleton for the gamesparks manager so it can be called from anywhere
    private static GameSparksManager instance = null;

    //getter property for private backing field instance
    public static GameSparksManager Instance() { return instance; }

    public Canvas registerPlayerCanvas;

    // Use this for initialization
    void Awake()
    {
        //this will create a singleton for our gamesparks manager object
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
        }
        GS.GameSparksAvailable += GSAvailable;
        GameSparks.Api.Messages.AchievementEarnedMessage.Listener += AchievementEarnedListener;
    }

    void GSAvailable(bool _isAvalable)
    {
        //this method will be called only when the GS service is available or unavailable
        if (_isAvalable)
        {
            // Application.LoadLevel(1);
            Debug.Log(">>>>>>>>>GS Conected<<<<<<<<");

            // Tenta concetar a uma conta
            new GameSparks.Api.Requests.AccountDetailsRequest()
                .Send((response) =>
                {

                    if (!response.HasErrors)
                    {
                        // Conectou, prossegue mostrando o nome
                        Debug.Log("Account Details Found... - Olá, " + response.DisplayName);

                        //string playerName = response.DisplayName; // we can get the display name

                        //username.text = "Olá, " + playerName + " - ";

                    }
                    else
                    {
                        // Não conectou a nenhuma conta, mostra tela de registro
                        Debug.Log("Error Retrieving Account Details...");

                        registerPlayerCanvas.gameObject.SetActive(true);
                    }
                });

        }
        else
        {
            Debug.Log(">>>>>>>>>GS Disconnected<<<<<<<<");
        }
    }

    //Achievement message  listener
    private void AchievementEarnedListener(GameSparks.Api.Messages.AchievementEarnedMessage _message)
    {
        Debug.LogWarning("Message Recieved" + _message.AchievementName);

    }



    #region FaceBook Authentication
    /// <summary>
    /// Below we will login with facebook.
    /// When FB is ready we will call the method that allows GS to connect to GameSparks
    /// Chamar este método em um botão para conectar com o FB
    /// Também cria usuário
    /// </summary>
    public void ConnectWithFacebook()
    {
        if (!FB.IsInitialized)
        {
            Debug.Log("Initializing Facebook");
            FB.Init(FacebookLogin);
        }
        else
        {
            FacebookLogin();
        }
    }


    /// <summary>
    /// When Facebook is ready , this will connect the pleyer to Facebook
    /// After the Player is authenticated it will  call the GS connect
    /// </summary>
    void FacebookLogin()
    {
        if (!FB.IsLoggedIn)
        {
            Debug.Log("Logging into Facebook");
            FB.LogInWithReadPermissions(
                new List<string>() { "public_profile", "email", "user_friends" },
                GameSparksFBConnect
                );
        }
    }

    void GameSparksFBConnect(ILoginResult result)
    {

        if (FB.IsLoggedIn)
        {
            Debug.Log("Logging into gamesparks with facebook details");
            GSFacebookLogin(AfterFBLogin);
        }
        else
        {
            Debug.Log("Something wrong  with FB");
        }
    }

    //this is the callback that happens when gamesparks has been connected with FB
    private void AfterFBLogin(GameSparks.Api.Responses.AuthenticationResponse _resp)
    {
        Debug.Log(_resp.DisplayName);
    }

    //delegate for asynchronous callbacks
    public delegate void FacebookLoginCallback(AuthenticationResponse _resp);


    //This method will connect GS with FB
    public void GSFacebookLogin(FacebookLoginCallback _fbLoginCallback)
    {
        Debug.Log("");

        new GameSparks.Api.Requests.FacebookConnectRequest()
            .SetAccessToken(AccessToken.CurrentAccessToken.TokenString)
            .Send((response) =>
            {
                if (!response.HasErrors)
                {
                    Debug.Log("Logged into gamesparks with facebook");
                    _fbLoginCallback(response);
                }
                else
                {
                    Debug.Log("Error Logging into facebook");
                }

            });
    }
    #endregion

    /// <summary>
    /// If a player is registered this will log them in with GameSparks.
    /// </summary>
    public void LoginPlayer(string _userNameInput, string _passwordInput)
    {
        new GameSparks.Api.Requests.AuthenticationRequest()
            .SetUserName(_userNameInput)
            .SetPassword(_passwordInput)
            .Send((response) =>
            {
                if (!response.HasErrors)
                {
                    Debug.Log("Player Authenticated...");
                }
                else
                {
                    Debug.Log("Error Authenticating Player\n" + response.Errors.JSON.ToString());
                }
            });
    }

    /// <summary>
    /// this will register a new player and assign their email to their account.
    /// </summary>
    public void RegisterNewPlayer(string _userNameInput, string _emailInput, string _passwordInput)
    {
        new GameSparks.Api.Requests.RegistrationRequest()
            .SetDisplayName(_userNameInput)
            .SetUserName(_userNameInput)
            .SetPassword(_passwordInput)
            .SetScriptData(new GSRequestData().AddString("email", _emailInput))
            .Send((response) =>
            {
                if (!response.HasErrors)
                {
                    Debug.Log("Player registered");
                }
                else
                {
                    Debug.LogWarning("Failed to register player...\n" + response.Errors.JSON.ToString());
                }


            });
    }


}