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
    public float realDistance {get; private set; } = 1f;
    public float distanceProxy {get; private set; } = 0.5f;
    public float realAngle {get; private set;}

    public bool isConnected { get; private set; } = false;
    public string classification { get; private set; }

    private List<string> serverIPs = new List<string>
    {
        "ws://192.168.0.19:8765",
        "ws://192.168.0.20:8765",
        "ws://172.20.10.2:8765",
    };
    

    void Start()
    {
        StartCoroutine(TryConnectToServer());
    }

    IEnumerator TryConnectToServer()
    {
        foreach (string ip in serverIPs){
            Debug.Log($"Connecting to websocket {ip}");
            ws = new WebSocket(ip); //laptop IP

            bool connectionAttemptFinished = false;
            bool connectionSucceeded = false;

            ws.OnOpen += (sender,e) =>
            {
                isConnected = true;
                connectionSucceeded = true;
                connectionAttemptFinished = true;
                Debug.Log("Websocket Connected!");
            };

            ws.OnClose += (sender,e) =>
            {
                isConnected = false;
                connectionAttemptFinished = true;
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

            float timeout = 2f;
            float timer = 0f;

            while (!connectionAttemptFinished && timer < timeout)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            if (connectionSucceeded)
                yield break;

            ws.Close();
            ws = null;
        }
         Debug.LogError("Could not connect to any WebSocket server.");
    }


    void OnDestroy(){
        if (ws != null){
            ws.Close();
        }
        
    }

    
}
