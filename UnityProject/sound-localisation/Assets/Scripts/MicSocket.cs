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
    public float distanceProxy {get; private set; } = 0.3f;
    public float realAngle {get; private set;}

    public bool isConnected { get; private set; } = false;
    public string classification { get; private set; }

    private List<string> serverIPs = new List<string>
    {
        "ws://172.20.10.2:8765",
        "ws://172.30.203.69:8765",
        "ws://192.168.0.19:8765",
    };
    

    void Start()
    {
        StartCoroutine(TryConnectToServer());
    }

    IEnumerator TryConnectToServer()
    {
        foreach (string ip in serverIPs){
            Debug.Log($"Connecting to websocket {ip}");
            WebSocket candidate = new WebSocket(ip); //laptop IP

            bool connectionAttemptFinished = false;
            bool connectionSucceeded = false;

            candidate.OnOpen += (sender,e) =>
            {
                isConnected = true;
                connectionSucceeded = true;
                connectionAttemptFinished = true;
                Debug.Log("Websocket Connected!");
            };
            candidate.OnError += (sender, e) =>
            {
                connectionAttemptFinished = true;
                Debug.Log($"Websocket error: {e.Message}");
            };
            candidate.OnClose += (sender,e) =>
            {
                isConnected = false;
                connectionAttemptFinished = true;
                Debug.Log($"Websocket disconnected! Code: {e.Code}, Reason: {e.Reason}");
            };
            candidate.OnMessage += (sender, e) => 
            {   
                //Debug.Log("message received" + e.Data);
                JObject json = JObject.Parse(e.Data);
                angle = (float)json["angle"];
                realAngle = angle;
                vad = (int)json["vad"];
                classification = (string)json["classification"];
            };

            candidate.ConnectAsync();

            float timer = 0f;
            float timeout = 2f;
            
            while (!connectionAttemptFinished && timer < timeout)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            if (connectionSucceeded)
            {
                ws = candidate;
                isConnected = true;
                yield break;
            }
            candidate.Close();
            yield return new WaitForSeconds(0.2f); // let close settle
        }
         Debug.LogError("Could not connect to any WebSocket server.");
    }


    void OnDestroy(){
        if (ws != null){
            ws.Close();
        }
        
    }

    
}
