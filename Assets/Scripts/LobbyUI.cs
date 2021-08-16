using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VivoxUnity;
using System.ComponentModel;
using System;
using UnityEngine.UI;
using TMPro;
public class LobbyUI : MonoBehaviour
{
    public static LobbyUI instance;

    public GameObject UserNameUI;
    public GameObject ChannelUI;
    public GameObject ChatUI;



    public VivoxManager credentials;

    public TMP_Text txt_UserName;
    [SerializeField] TMP_Text txt_ChannelName;
    public TMP_Text txt_Message_Prefab;

    [SerializeField] TMP_InputField tmp_Input_UserName;
    [SerializeField] TMP_InputField tmp_Input_ChannelName;
    [SerializeField] TMP_InputField tmp_Input_SendMessages;

    public Image container;
    public TMP_Dropdown tmp_Dropdown_LoggedInUsers;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }


    void Start()
    {
        
    }

    #region UI control functions

    public void UserNameScreen() 
    {
        UserNameUI.SetActive(true);
        ChannelUI.SetActive(false);
        ChatUI.SetActive(false);
    }
    public void ChannelScreen() 
    {
        UserNameUI.SetActive(false);
        ChannelUI.SetActive(true);
        ChatUI.SetActive(false);
    }

    public void ChatScreen()
    {
        UserNameUI.SetActive(false);
        ChannelUI.SetActive(false);
        ChatUI.SetActive(true);
    }




    #endregion






    public void Btn_Join_Channel()
    {
        credentials.JoinChannel(tmp_Input_ChannelName.text, true, true, true, ChannelType.NonPositional);
    }


    public void Leave_Channel(IChannelSession channelToDiconnect, string channelName)
    {
        channelToDiconnect.Disconnect();
        credentials.vivox.loginSession.DeleteChannelSession(new ChannelId(credentials.vivox.issuer, channelName, credentials.vivox.domain));
    }

    public void Btn_Leave_Channel_Clicked()
    {
        Leave_Channel(credentials.vivox.channelSession, tmp_Input_ChannelName.text);
    }

    public void LoginUser()
    {
        credentials.Login(tmp_Input_UserName.text, SubscriptionMode.Accept);
    }

    public void Logout()
    {
        credentials.vivox.loginSession.Logout();
        credentials.Bind_Login_Callback_Listeners(false, credentials.vivox.loginSession);
    }

    public void Send_Group_Message()
    {
        credentials.Send_Group_Message(tmp_Input_SendMessages.text);
    }

    public void Send_Event_Message()
    {
        credentials.Send_Event_Message(tmp_Input_SendMessages.text, "Test", "blue");
    }

    public void Send_Direct_Message()
    {
        credentials.Send_Direct_Message(tmp_Dropdown_LoggedInUsers.Get_Selected(), tmp_Input_SendMessages.text);
    }

    public void MuteToggle()
    {
        //credentials.vivox.client client;
        if (credentials.vivox.client.AudioInputDevices.Muted)
        {
            credentials.vivox.client.AudioInputDevices.Muted = false;
        }
        else
        {
            credentials.vivox.client.AudioInputDevices.Muted = true;
        }
    }

}
