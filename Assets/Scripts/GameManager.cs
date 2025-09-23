using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

//public enum GameEvents
//{
//    TrailGuide,
//    SuperSpeed,
//    MiniMap
//}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Networked] public NetworkString<_32> WinnerName { get; set; }

    [SerializeField]
    private float timeBetweenEvents = 10f;
    [SerializeField]
    private GameObject guiderPrefab;
    private void Awake()
    {
        Instance = this;
    }
    public override void Spawned()
    {
        if (Runner.IsServer)
        {
            //StartCoroutine(EventsPicker());
        }
    }
    //private IEnumerator EventsPicker()
    //{
    //    var events = System.Enum.GetValues(typeof(GameEvents));

    //    while (true)
    //    {
    //        yield return new WaitForSeconds(timeBetweenEvents);

    //        // Step 1: Tell all clients to show "Choosing..." message
    //        Rpc_ShowChoosingMessage();

    //        // Step 2: Wait 2 seconds before picking
    //        yield return new WaitForSeconds(2f);

    //        // Step 3: Pick random event
    //        var randomEvent = (GameEvents)events.GetValue(Random.Range(0, events.Length));
    //        Debug.Log($"Event Triggered: {randomEvent}");

    //        // Step 4: Tell all clients to update panel with event name
    //        Rpc_ShowEventName(randomEvent.ToString());

    //        // Step 5: Execute the event
    //        Rpc_ExecuteEvent(randomEvent);
    //    }
    //}


    //[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    //public void Rpc_ExecuteEvent(GameEvents value)
    //{
    //    switch (value)
    //    {
    //        case GameEvents.TrailGuide:
    //            var localPlayer = FindLocalPlayer();
    //            var exitDoor = FindExitDoor();
    //            if (localPlayer != null && exitDoor != null)
    //            {
    //                var guiderObj = Instantiate(guiderPrefab, localPlayer.transform.position, Quaternion.identity);
    //                var guider = guiderObj.GetComponent<Guider>();
    //                var exitPos = exitDoor.GetComponentInChildren<ParticleSystem>().transform;
    //                guider.Initialize(localPlayer.transform, exitPos);
    //            }

    //            break;
    //        case GameEvents.SuperSpeed:
    //            foreach (var netPlayer in Runner.ActivePlayers)
    //            {
    //                var playerObj = Runner.GetPlayerObject(netPlayer);
    //                if (playerObj != null)
    //                {
    //                    var movement = playerObj.GetComponent<PlayerMovement>();
    //                    if (movement != null)
    //                    {
    //                        movement.ApplySpeedBoost(10f, 5f); // speed = 10, duration = 5 seconds
    //                    }
    //                }
    //            }
    //            break;
    //        case GameEvents.MiniMap:
    //            MinimapUI.Instance.ShowMinimap();
    //            break;
    //    }
    //}
    //[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    //private void Rpc_ShowChoosingMessage()
    //{
    //    EventPanelUI.Instance.ShowChoosing("Choosing a random Event...");
    //}

    //[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    //private void Rpc_ShowEventName(string eventName)
    //{
    //    EventPanelUI.Instance.ShowEvent($"You've Got: {eventName}");
    //}

    public PlayerNetworkController FindLocalPlayer()
    {
        var players = FindObjectsByType<PlayerNetworkController>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.GetComponent<NetworkObject>().HasInputAuthority)
                return player;

        }
        return null;
    }
    public Transform FindExitDoor()
    {
        var doors = FindObjectsByType<Door>(FindObjectsSortMode.None);
        foreach (var door in doors)
        {
            if (door != null && door.IsGoal)
                return door.transform;
        }
        return null;
    }
    public void SetWinner(PlayerRef player, NetworkString<_32> playerName)
    {
        if (Runner.IsServer && string.IsNullOrEmpty(WinnerName.ToString()))
        {
            WinnerName = playerName;

            Debug.Log($"Winner is {playerName}!");

            // Disable player movement for all
            foreach (var netPlayer in Runner.ActivePlayers)
            {
                var playerObj = Runner.GetPlayerObject(netPlayer);
                if (playerObj != null)
                {
                    var movement = playerObj.GetComponent<PlayerMovement>();
                    if (movement != null) movement.enabled = false;
                }
            }

            // Tell everyone to show the panel
            RpcShowWinnerPanel(playerName.ToString(), 3f);

            // Delay reload on server
            StartCoroutine(ResetSceneAfterDelay(3f));
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcShowWinnerPanel(string winnerName, float countdownTime)
    {
        //WinnerPanelUI.Instance.ShowWinner(winnerName, countdownTime);
    }

    private IEnumerator ResetSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReloadGameplayScene();
    }

    private void ReloadGameplayScene()
    {
        if (Runner.IsServer)
        {
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;
            Runner.LoadScene(SceneRef.FromIndex(sceneIndex), LoadSceneMode.Single);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_ChooseExitDoor(NetworkObject doorObject)
    {
        Door selectedDoor = doorObject.GetComponent<Door>();
        selectedDoor.SetAsGoal();
    }

}