using UnityEngine;
using Mirror;
using Mirror.SimpleWeb;
using System.Web;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor; // Required for Editor-specific code
#endif

public class BootLoader: MonoBehaviour
{
    [Header("Server Side")]
    [SerializeField] private WagrAuthenticator Authenticator;
    [SerializeField] private ServerSharedInfo ServerInfo;
#if UNITY_SERVER

    void Start() {
        ushort port = 7777; // Default port
        string player1Token = null;
        string player2Token = null;
        string username1 = null;
        string username2 = null;
        string[] args;


#if UNITY_EDITOR
        // Use simulated arguments in the Editor
        string simulatedArgsString = CommandLineArgsSimulator.SimulatedArguments;
        if (!string.IsNullOrEmpty(simulatedArgsString)) {
            args = ParseSimulatedArguments(simulatedArgsString); // Parse the string into an array
            Debug.Log("Using Simulated Command Line Arguments in Editor.");
        }
        else {
            args = System.Environment.GetCommandLineArgs(); // Fallback to actual command line if simulation is empty
            Debug.Log("Using Actual Command Line Arguments (if any).");
        }
#else
        // Use actual command line arguments in builds
        args = System.Environment.GetCommandLineArgs();
#endif



        for (int i = 0; i < args.Length; i++) {
            switch (args[i]) {
                case "-port":
                    if (i + 1 < args.Length && ushort.TryParse(args[i + 1], out ushort parsedPort)) {
                        port = parsedPort;
                    }
                    break;
                case "-player1token":
                    if (i + 1 < args.Length) {
                        player1Token = args[i + 1];
                    }
                    break;
                case "-player2token":
                    if (i + 1 < args.Length) {
                        player2Token = args[i + 1];
                    }
                    break;
                case "-username1":
                    if (i + 1 < args.Length) {
                        username1 = args[i + 1];
                    }
                    break;
                case "-username2":
                    if (i + 1 < args.Length) {
                        username2 = args[i + 1];
                    }
                    break;
            }
        }

        SetupAuthenticator(player1Token, player2Token);

        if (string.IsNullOrEmpty(username1) || string.IsNullOrEmpty(username2)) {
            Debug.LogError($"One or both usernames not given via console arguments.");
            Application.Quit(999);
            return;
        }
        Global.Player1Name = username1;
        Global.Player2Name = username2;

        NetworkManager.singleton.GetComponent<SimpleWebTransport>().Port = port;
        NetworkManager.singleton.StartServer();
    }

    private void SetupAuthenticator(string player1Token, string player2Token) {
        if (Authenticator == null) {
            throw new System.Exception("Authenticator not set!!!");
        }
        Authenticator.Player1Token = player1Token;
        Authenticator.Player2Token = player2Token;

        if (string.IsNullOrEmpty(Authenticator.Player1Token)) {
            throw new System.Exception("Player 1 Token not provided via command line.");
        }
        if (string.IsNullOrEmpty(Authenticator.Player2Token)) {
            throw new System.Exception("Player 2 Token not provided via command line.");
        }
    }
#endif

#if !UNITY_SERVER

    private void Start() {
        ushort port = 7777; // Default port
        string playerToken = null;
        string hostname = "localhost";
        string[] args;


#if UNITY_WEBGL
        // In WebGL, retrieve parameters from the URL
        args = GetUrlArguments();
        Debug.Log("Using URL parameters in WebGL build.");
#else
#if UNITY_EDITOR
        // Use simulated arguments in the Editor
        string simulatedArgsString = CommandLineArgsSimulator.SimulatedArguments;
        if (!string.IsNullOrEmpty(simulatedArgsString)) {
            args = ParseSimulatedArguments(simulatedArgsString);
            Debug.Log("Using Simulated Command Line Arguments in Editor.");
        }
        else {
            args = System.Environment.GetCommandLineArgs();
            Debug.Log("Using Actual Command Line Arguments (if any).");
        }
#else
        // Use actual command line arguments in standalone builds
        args = System.Environment.GetCommandLineArgs();
#endif
#endif



        for (int i = 0; i < args.Length; i++) {
            switch (args[i]) {
                case "-port":
                    if (i + 1 < args.Length && ushort.TryParse(args[i + 1], out ushort parsedPort)) {
                        port = parsedPort;
                    }
                    break;
                case "-playerToken":
                    if (i + 1 < args.Length) {
                        playerToken = args[i + 1];
                    }
                    break;
                case "-hostname":
                    if (i + 1 < args.Length) {
                        hostname = args[i + 1];
                    }
                    break;
            }
        }

        SetupAuthenticator(playerToken);

        Debug.Log("Starting Client...");
        
        NetworkManager.singleton.GetComponent<SimpleWebTransport>().Port = port;
        NetworkManager.singleton.networkAddress = hostname;
        NetworkManager.singleton.StartClient();
    }

    private void SetupAuthenticator(string playerToken) {
        if (Authenticator == null) {
            throw new System.Exception("Authenticator not set!!!");
        }
        Authenticator.LocalToken = playerToken;

        if (string.IsNullOrEmpty(Authenticator.LocalToken)) {
            throw new System.Exception("Local Player Token not provided via command line.");
        }
    }

#endif





#if UNITY_WEBGL
    // Function to parse URL parameters
    private string[] GetUrlArguments() {
        string queryString = Application.absoluteURL.Split('?').Skip(1).FirstOrDefault();
        if (string.IsNullOrEmpty(queryString)) return new string[0];

        var queryParams = HttpUtility.ParseQueryString(queryString);
        return queryParams.AllKeys.SelectMany(key => new string[] { "-" + key, queryParams[key] }).ToArray();
    }
#endif

#if UNITY_EDITOR
    // Helper function to parse simulated arguments string into an array
    private string[] ParseSimulatedArguments(string argsString) {
        return argsString.Split(' '); // Simple split by space, you can improve parsing if needed
    }
#endif
}
