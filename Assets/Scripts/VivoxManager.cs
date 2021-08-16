using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VivoxUnity;
using System.ComponentModel;
using System;
using UnityEngine.UI;
using TMPro;

#if PLATFORM_ANDROID

using UnityEngine.Android;

#endif

public class VivoxManager : MonoBehaviour
{
    public Base_Credentials vivox = new Base_Credentials();
    public LobbyUI lobbyUI;

    private void Awake()
    {
        vivox.client = new VivoxUnity.Client();
        vivox.client.Uninitialize();
        vivox.client.Initialize();
        DontDestroyOnLoad(this);
    }

    private void OnApplicationQuit()
    {
        vivox.client.Uninitialize();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }



#region Binding Callbacks

    public void Bind_Login_Callback_Listeners(bool bind, ILoginSession loginSesh)
    {
        if (bind)
        {
            loginSesh.PropertyChanged += Login_Status;
        }
        else
        {
            loginSesh.PropertyChanged -= Login_Status;
        }

    }

    public void Bind_Channel_Callback_Listeners(bool bind, IChannelSession channelSesh)
    {
        if (bind)
        {
            channelSesh.PropertyChanged += On_Channel_Status_Changed;
        }
        else
        {
            channelSesh.PropertyChanged -= On_Channel_Status_Changed;
        }
    }

    public void Bind_User_Callbacks(bool bind, IChannelSession channelSesh)
    {
        if (bind)
        {
            channelSesh.Participants.AfterKeyAdded += On_Participant_Added;
            channelSesh.Participants.BeforeKeyRemoved += On_Participant_Removed;
            channelSesh.Participants.AfterValueUpdated += On_Participant_Updated;
        }
        else
        {
            channelSesh.Participants.AfterKeyAdded -= On_Participant_Added;
            channelSesh.Participants.BeforeKeyRemoved -= On_Participant_Removed;
            channelSesh.Participants.AfterValueUpdated -= On_Participant_Updated;
        }
    }

    public void Bind_Group_Message_Callbacks(bool bind, IChannelSession channelSesh)
    {
        if (bind)
        {
            channelSesh.MessageLog.AfterItemAdded += On_Message_Recieved;
        }
        else
        {
            channelSesh.MessageLog.AfterItemAdded -= On_Message_Recieved;
        }
    }

    public void Bind_Directed_Message_Callbacks(bool bind, ILoginSession loginSesh)
    {
        if (bind)
        {
            loginSesh.DirectedMessages.AfterItemAdded += On_Direct_Message_Recieved;
            loginSesh.FailedDirectedMessages.AfterItemAdded += On_Direct_Message_Failed;
        }
        else
        {
            loginSesh.DirectedMessages.AfterItemAdded -= On_Direct_Message_Recieved;
            loginSesh.FailedDirectedMessages.AfterItemAdded -= On_Direct_Message_Failed;
        }
    }


    #endregion


    #region Login Methods



    public void Login(string userName)
    {
        AccountId accountId = new AccountId(vivox.issuer, userName, vivox.domain);
        vivox.loginSession = vivox.client.GetLoginSession(accountId);

        Bind_Login_Callback_Listeners(true, vivox.loginSession);
        
        vivox.loginSession.BeginLogin(vivox.server, vivox.loginSession.GetLoginToken(vivox.tokenKey, vivox.timeSpan), ar =>
        {
            try
            {
                vivox.loginSession.EndLogin(ar);
            }
            catch (Exception e)
            {
                Bind_Login_Callback_Listeners(false, vivox.loginSession);
                
                Debug.Log(e.Message);
            }
        });
    }


    public void Login(string userName, SubscriptionMode subMode)
    {
        AccountId accountId = new AccountId(vivox.issuer, userName, vivox.domain);
        vivox.loginSession = vivox.client.GetLoginSession(accountId);

        Bind_Login_Callback_Listeners(true, vivox.loginSession);
        Bind_Directed_Message_Callbacks(true, vivox.loginSession);

        vivox.loginSession.BeginLogin(vivox.server, vivox.loginSession.GetLoginToken(vivox.tokenKey, vivox.timeSpan), subMode, null, null, null, ar =>
        {
            try
            {
                vivox.loginSession.EndLogin(ar);
            }
            catch (Exception e)
            {
                Bind_Login_Callback_Listeners(false, vivox.loginSession);
                Bind_Directed_Message_Callbacks(false, vivox.loginSession);

                Debug.Log(e.Message);
            }
        });
    }





    public void Login_Status(object sender, PropertyChangedEventArgs loginArgs)
    {
        var source = (ILoginSession)sender;

        switch (source.State)
        {
            case LoginState.LoggingIn:
                Debug.Log("Logging In");
                break;
            case LoginState.LoggedIn:
                Debug.Log($"Logged In {vivox.loginSession.LoginSessionId.Name}");
                
                break;
        }
    }

#endregion

#region Join Channel Methods

    public void JoinChannel(string channelName, bool IsAudio, bool IsText, bool switchTransmission, ChannelType channelType)
    {
        
        ChannelId channelId = new ChannelId(vivox.issuer, channelName, vivox.domain, channelType);
        vivox.channelSession = vivox.loginSession.GetChannelSession(channelId);
        Bind_Channel_Callback_Listeners(true, vivox.channelSession);
        Bind_User_Callbacks(true, vivox.channelSession);
        Bind_Group_Message_Callbacks(true, vivox.channelSession);

        if (IsAudio)
        {
            vivox.channelSession.PropertyChanged += On_Audio_State_Changed;
        }
        if (IsText)
        {
            vivox.channelSession.PropertyChanged += On_Text_State_Changed;
        }

        vivox.channelSession.BeginConnect(IsAudio, IsText, switchTransmission, vivox.channelSession.GetConnectToken(vivox.tokenKey, vivox.timeSpan), ar =>
        {
            try
            {
                vivox.channelSession.EndConnect(ar);
            }
            catch (Exception e)
            {
                Bind_Channel_Callback_Listeners(false, vivox.channelSession);
                Bind_User_Callbacks(false, vivox.channelSession);
                Bind_Group_Message_Callbacks(false, vivox.channelSession);
                if (IsAudio)
                {
                    vivox.channelSession.PropertyChanged -= On_Audio_State_Changed;
                }
                if (IsText)
                {
                    vivox.channelSession.PropertyChanged -= On_Text_State_Changed;
                }

                Debug.Log(e.Message);
            }
        });
    }

    

    public void On_Channel_Status_Changed(object sender, PropertyChangedEventArgs channelArgs)
    {
        IChannelSession source = (IChannelSession)sender;

        if(channelArgs.PropertyName == "ChannelState")
        {
            switch (source.ChannelState)
            {
                case ConnectionState.Connecting:
                    Debug.Log("Channel Connecting");
                    break;
                case ConnectionState.Connected:
                    Debug.Log($"{source.Channel.Name} Connected");
                    
                
                    break;
                case ConnectionState.Disconnecting:
                    Debug.Log($"{source.Channel.Name} disconnecting");
                    break;
                case ConnectionState.Disconnected:
                    Debug.Log($"{source.Channel.Name} disconnected");
                    Bind_Channel_Callback_Listeners(false, vivox.channelSession);
                    Bind_User_Callbacks(false, vivox.channelSession);
                    Bind_Group_Message_Callbacks(false, vivox.channelSession);
                    Bind_Directed_Message_Callbacks(false, vivox.loginSession);
                    break;
            }
        }

        
    }

    public void On_Audio_State_Changed(object sender, PropertyChangedEventArgs audioArgs)
    {
        IChannelSession source = (IChannelSession)sender;

        if (audioArgs.PropertyName == "AudioState")
        {
            switch (source.AudioState)
            {
                case ConnectionState.Connecting:
                    Debug.Log($"Audio Channel Connecting");
                    break;
                case ConnectionState.Connected:
                    Debug.Log($"Audio Channel Connected");
                    break;
                case ConnectionState.Disconnecting:
                    Debug.Log($"Audio Channel Disconnecting");
                    break;
                case ConnectionState.Disconnected:
                    Debug.Log($"Audio Channel Disconnected");
                    vivox.channelSession.PropertyChanged -= On_Audio_State_Changed;
                    break;
            }
        }

            
    }

    public void On_Text_State_Changed(object sender, PropertyChangedEventArgs textArgs)
    {
        IChannelSession source = (IChannelSession)sender;

        if (textArgs.PropertyName == "TextState")
        {
            switch (source.TextState)
            {
                case ConnectionState.Connecting:
                    Debug.Log($"Text Channel Connecting");
                    break;
                case ConnectionState.Connected:
                    Debug.Log($"Text Channel Connected");
                    break;
                case ConnectionState.Disconnecting:
                    Debug.Log($"Text Channel Disconnecting");
                    break;
                case ConnectionState.Disconnected:
                    Debug.Log($"Text Channel Disconnected");
                    vivox.channelSession.PropertyChanged -= On_Text_State_Changed;
                    break;
            }
        }

            

    }


#endregion

#region User Callbacks

    public void On_Participant_Added(object sender, KeyEventArg<string> participantArgs)
    {
        var source = (VivoxUnity.IReadOnlyDictionary<string, IParticipant>)sender;

        IParticipant user = source[participantArgs.Key];
        Debug.Log($"{user.Account.Name} has joined the channel");

        var temp = Instantiate(lobbyUI.txt_Message_Prefab, lobbyUI.container.transform);
        temp.text = $"{user.Account.Name} has joined the channel";

        if (!user.IsSelf)
        {
            lobbyUI.tmp_Dropdown_LoggedInUsers.Add_Value(user.Account.Name);
        }

    }

    public void On_Participant_Removed(object sender, KeyEventArg<string> participantArgs)
    {
        var source = (VivoxUnity.IReadOnlyDictionary<string, IParticipant>)sender;

        IParticipant user = source[participantArgs.Key];
        Debug.Log($"{user.Account.Name} has left the channel");

        var temp = Instantiate(lobbyUI.txt_Message_Prefab, lobbyUI.container.transform);
        temp.text = $"{user.Account.Name} has left the channel";
    }

    public void On_Participant_Updated(object sender, ValueEventArg<string, IParticipant> participantArgs)
    {
        var source = (VivoxUnity.IReadOnlyDictionary<string, IParticipant>)sender;

        IParticipant user = source[participantArgs.Key];
    }




#endregion

#region Message Methods
        
    public void Send_Group_Message(string message)
    {
        vivox.channelSession.BeginSendText(message, ar =>
        {
            try
            {
                vivox.channelSession.EndSendText(ar);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        });
    }


    public void Send_Event_Message(string message, string stanzaNameSpace, string stanzaBody)
    {
        vivox.channelSession.BeginSendText(null, message, stanzaNameSpace, stanzaBody, ar =>
        {
            try
            {
                vivox.channelSession.EndSendText(ar);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        });
    }


    public void On_Message_Recieved(object sender, QueueItemAddedEventArgs<IChannelTextMessage> msgArgs)
    {
        var messenger = (VivoxUnity.IReadOnlyQueue<IChannelTextMessage>)sender;

        Debug.Log($"From {msgArgs.Value.Sender} : Message - {msgArgs.Value.Message}");

        Check_Message_Args(msgArgs.Value);

        var temp = Instantiate(lobbyUI.txt_Message_Prefab, lobbyUI.container.transform);
        temp.text = $"From {msgArgs.Value.Sender.DisplayName} : Message - {msgArgs.Value.Message}";
    }

    public void Check_Message_Args(IChannelTextMessage message)
    {
        if (message.ApplicationStanzaNamespace == "Test")
        {
            Debug.Log("This is a test");
            if (message.ApplicationStanzaBody == "blue")
            {
                Debug.Log("this player is blue");
            }
        }
        if (message.ApplicationStanzaBody == "Helloe Body")
        {
            Debug.Log("This a hidden message");
        }
    }


    #endregion


    #region Send Direct Messages


    public void Send_Direct_Message(string userToSend, string message)
    {
        var accountID = new AccountId(vivox.issuer, userToSend, vivox.domain);

        vivox.loginSession.BeginSendDirectedMessage(accountID, message, ar =>
        {
            try
            {
                vivox.loginSession.EndSendDirectedMessage(ar);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        });
    }

    public void On_Direct_Message_Recieved(object sender, QueueItemAddedEventArgs<IDirectedTextMessage> txtMsgArgs)
    {
        var msgSender = (IReadOnlyQueue<IDirectedTextMessage>)sender;

        while (msgSender.Count > 0)
        {
            var msg = msgSender.Dequeue().Message;
            var temp = Instantiate(lobbyUI.txt_Message_Prefab, lobbyUI.container.transform);
            temp.text = msg;
            // Debug.Log(txtMsgArgs.Value.Message);
        }
    }

    public void On_Direct_Message_Failed(object sender, QueueItemAddedEventArgs<IFailedDirectedTextMessage> txtMsgArgs)
    {
        var msgSender = (IReadOnlyQueue<IFailedDirectedTextMessage>)sender;

        Debug.Log(txtMsgArgs.Value.Sender);
        vivox.failedMessages.Add(txtMsgArgs.Value);
    }

    #endregion

}
