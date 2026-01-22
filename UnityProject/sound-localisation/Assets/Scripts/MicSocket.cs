using System.Collections;
using System.Collections.Generic;
using WebSocketSharp; 
using Newtonsoft.Json.Linq;
using UnityEngine;

public class MicSocket : MonoBehaviour, IMicSocket
{
    WebSocket ws;

    public float angle ;
    public int vad;
    public string classification;
    public string transcript;

    public bool isConnected = false;

    // Start is called before the first frame update
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
        vad = (int)json["vad"];
        classification = (string)json["classification"];
        transcript = (string)json["transcript"];
     };

     ws.ConnectAsync();
    }

    void OnDestroy(){
        if (ws != null){
            ws.Close();
        }
        
    }
}
