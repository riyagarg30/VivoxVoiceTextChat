using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
using VivoxUnity;
using System.ComponentModel;
using System;


public class Base_Credentials
{
    public VivoxUnity.Client client;
    public Uri server = new Uri("https://mt1s.www.vivox.com/api2");
    public string issuer = "riyaga8715-le40-dev";
    public string domain = "mt1s.vivox.com";
    public string tokenKey = "bonk431";
    public TimeSpan timeSpan = TimeSpan.FromSeconds(90);


    public ILoginSession loginSession;
    public IChannelSession channelSession;

    public List<IFailedDirectedTextMessage> failedMessages = new List<IFailedDirectedTextMessage>();
}
