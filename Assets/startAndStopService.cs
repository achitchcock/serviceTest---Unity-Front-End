using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class startAndStopService : MonoBehaviour {

    public Button start;
    public Button stop;
    public Button send;
    public Button loadSDK;
    public Button register;
    public Button connectDrone;
    public Button updateStat;
    public Button takeOffBtn;
    public Button landBtn;
    public Button fcstatusbutn;
    public Button videoBtn;
    public Button save;
    public Text input;
    public Text status;
    public Text product;
    public Text connectionStatus;
    public Text flightControllerStatus;
    public Text buffStatus;
    public RawImage rimg;
    private AndroidJavaObject sc;
    private AndroidJavaClass unityPlayer;
    private AndroidJavaObject unityActivity;
    private Material mat;
    private bool enableVid;
    private bool checkStatus;
    private int opCount;
    private Texture2D tex2d;
    private bool write;


    // Use this for initialization
    void Start () {
        Debug.Log("Starting...");
        unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

        unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        object[] param = new object[1];
        param[0] = unityActivity;

        sc = new AndroidJavaObject("uwb.aaron.com.servicestarterlib.serviceConnection", param);
        enableVid = false;
        checkStatus = false;
        opCount = 0;
        start.onClick.AddListener(startServ);
        stop.onClick.AddListener(stopServ);
        send.onClick.AddListener(sendMsg);
        loadSDK.onClick.AddListener(load_SDK);
        register.onClick.AddListener(registerApp);
        connectDrone.onClick.AddListener(connect);
        updateStat.onClick.AddListener(updateStatus);
        landBtn.onClick.AddListener(land);
        takeOffBtn.onClick.AddListener(takeOff);
        fcstatusbutn.onClick.AddListener(fcStatus);
        videoBtn.onClick.AddListener(videoState);
        save.onClick.AddListener(saveByte);
        //mat = surface.GetComponent<Renderer>().material;
        write = false;
        tex2d = new Texture2D(960, 720);
        //mat.mainTexture = tex2d;
    }
	
    void startServ()
    {
        sc.Call("startDroneService");
    }

    void stopServ()
    {
        sc.Call("stopDroneService");
        status.text = "Service stopped.";
        product.text = "none";
        connectionStatus.text = "Not connected";
    }

    void sendMsg()
    {
        object[] args = new object[1];
        args[0] = input.text.ToString();
        sc.Call("sendCommand",args);
    }

    void load_SDK()
    {
        object[] args = new object[1];
        args[0] = "LOAD_SDK";
        sc.Call("sendCommand", args);
    }

    void registerApp()
    {
        object[] args = new object[1];
        args[0] = "REGISTER";
        sc.Call("sendCommand", args);
    }

    void connect()
    {
        object[] args = new object[1];
        args[0] = "CONNECT_DRONE";
        sc.Call("sendCommand", args);
    }

    void updateStatus()
    {
        object[] args = new object[1];
        args[0] = "REFRESH_DJI";
        sc.Call("sendCommand", args);
    }

    void takeOff()
    {
        object[] args = new object[1];
        args[0] = "TAKE_OFF";
        sc.Call("sendCommand", args);
    }

    void land()
    {
        object[] args = new object[1];
        args[0] = "LAND";
        sc.Call("sendCommand", args);
    }

    void fcStatus()
    {
        object[] args = new object[1];
        args[0] = "FC_STATUS";
        sc.Call("sendCommand", args);
        checkStatus = true;
    }

    void videoState()
    {
        if(enableVid == false)
        {
            enableVid = true;
            sc.Call("sendCommand", new object[] { "START_VIDEO" });
            videoBtn.GetComponentInChildren<Text>().text = "6. Stop Video";
        }
        else
        {
            enableVid = false;
            sc.Call("sendCommand", new object[] { "STOP_VIDEO" });
            videoBtn.GetComponentInChildren<Text>().text = "6. Start Video";

        }
    }

    void saveByte()
    {
        write = true;
        Debug.Log("write set to true");
    }
    void Update()
    {
        opCount += 1;
        if(checkStatus == true && opCount>0  && opCount%40==0)
        {
            opCount = 0;
            sc.Call("sendCommand", new object[] { "FC_STATUS" });
            sc.Call("sendCommand", new object[] { "REFRESH_DJI" });
            status.text = sc.Call<string>("get", new object[] { "DATA" });
            product.text = sc.Call<string>("get", new object[] { "PRODUCT" });
            connectionStatus.text = sc.Call<string>("get", new object[] { "CONNECTION_STATUS" });
            flightControllerStatus.text = sc.Call<string>("get", new object[] { "FC_STATUS" });
        }
        
        if(enableVid == true)
        {
            byte[] t = sc.Call<byte[]>("getTex");
            buffStatus.text = "Video received: "+ sc.Call<int>("getBuffSize")+ " A:" + t.Length;
            tex2d.LoadImage(t);
            tex2d.Apply();
            if (write == true)
            {
                Debug.Log("Writing file...");
                byte[] bytes = tex2d.EncodeToPNG();
                File.WriteAllBytes(Application.persistentDataPath + "/../SavedScreen.png", bytes);
                write = false;
                Debug.Log("File written");
            }
            rimg.texture = tex2d;
            
            //tex2d.LoadRawTextureData(t);
            //
            //img.sprite.texture.LoadImage(t);
            //img.sprite.texture.Apply();
            //mat.mainTexture = tex2d;
            //surface.GetComponent<Renderer>().material.mainTexture = tex2d;

            //img.GetComponent<Renderer>().material.mainTexture = new Texture2D(20, 20);
            
            
            //mat.SetTexture()
        }
    }

}
