using System.Collections;
using System.Collections.Generic;
using WebSocketSharp; 
using Newtonsoft.Json.Linq;
using UnityEngine;

public class MicSocket : MonoBehaviour, IMicSocket
{
    WebSocket ws;

    public float angle { get; private set; }
    public int vad { get; private set; }
    public float realDistance {get; private set; } = 1;
    public float distanceProxy {get; private set; } = 1;
    public float realAngle {get; private set;}

    public bool isConnected { get; private set; } = false;
    public string classification { get; private set; }
    

    void Start()
    {
    Debug.Log("Connecting to websocet");
     ws = new WebSocket("ws://192.168.0.19:8765"); //laptop IP

    ws.OnOpen += (sender,e) =>
    {
        isConnected = true;
        Debug.Log("Websocket Connected!");
    };

    ws.OnClose += (sender,e) =>
    {
        isConnected = false;
        Debug.Log($"Websocket disconnected! Code: {e.Code}, Reason: {e.Reason}");
    };

     ws.OnMessage += (sender, e) => 
     {
        //Debug.Log("message received" + e.Data);
        JObject json = JObject.Parse(e.Data);
        angle = (float)json["angle"];
        realAngle = angle;
        vad = (int)json["vad"];
        classification = (string)json["classification"];
     };

     ws.ConnectAsync();
    }


    void OnDestroy(){
        if (ws != null){
            ws.Close();
        }
        
    }
}
