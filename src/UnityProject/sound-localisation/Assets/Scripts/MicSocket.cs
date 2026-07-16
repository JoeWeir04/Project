using System.Collections;
using System.Collections.Generic;
using WebSocketSharp; 
using Newtonsoft.Json.Linq;
using UnityEngine;
using TMPro;

public class MicSocket : MonoBehaviour, IMicSocket
{
    WebSocket ws;

    public float angle { get; private set; }
    public int vad { get; private set; }
    public float realDistance {get; private set; } 
    public float distanceProxy {get; private set; } 
    public float realAngle {get; private set;}

    public bool isConnected { get; private set; } = false;
    public string classification { get; private set; }
    public bool isClose {get; private set; }

    private const string serverIP = "ws://172.20.10.2:8765";
    public float retryDelay = 2f;
    public float connectTimeout = 3f;
    private bool shouldReconnect = true; // set false in OnDestroy to stop retry loop
    private Coroutine connectRoutine;
    public TMP_Text debug_text;
    

    void Start()
    {
        
        connectRoutine = StartCoroutine(ConnectLoop());
    }

    IEnumerator ConnectLoop()
    {
        while (shouldReconnect)
        {
            yield return StartCoroutine(TryConnectToServer());
            if (shouldReconnect)
            {
                Debug.Log($"Retrying connection in {retryDelay} seconds...");
                yield return new WaitForSeconds(retryDelay);
            }
        }
    }

    IEnumerator TryConnectToServer()
    {
        
        Debug.Log($"Connecting to websocket {serverIP}");
        if (debug_text != null)
        {
            debug_text.text = "Trying to connect to laptop";
        }
        WebSocket candidate = new WebSocket(serverIP); //laptop IP

        bool connectionAttemptFinished = false;
        bool connectionSucceeded = false;
        bool disconnected = false;

        candidate.OnOpen += (sender,e) =>
        {
            isConnected = true;
            connectionSucceeded = true;
            connectionAttemptFinished = true;
            if(debug_text != null)
            {
                debug_text.text = "";
            }
           
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
            disconnected = true;
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
            realDistance = (float)json["distance"];
            distanceProxy = (float)json["distance"];
            isClose = (bool)json["isClose"];
        };

        candidate.ConnectAsync();

        float timer = 0f;
        
        while (!connectionAttemptFinished && timer < connectTimeout)
        {
            if (debug_text != null)
            {
                debug_text.text = "Trying to connect to laptop";
            }
            timer += Time.deltaTime;
            yield return null;
        }

        if (!connectionSucceeded)
        {
            try { candidate.Close(); }
            catch (System.Exception ex) { Debug.LogWarning($"Error closing failed candidate: {ex.Message}"); }
            yield break;
        }
        ws = candidate;
        isConnected = true;
        while (!disconnected && shouldReconnect)
        {
            if (debug_text != null)
            {
                debug_text.text = "Trying to connect to laptop";
            }
            
            yield return null;
        }

        isConnected = false;
    }


    void OnDestroy(){
        shouldReconnect = false;
        if (connectRoutine != null)
        {
            StopCoroutine(connectRoutine);
        }
        if (ws != null){
            ws.Close();
        }
        
    }
}
