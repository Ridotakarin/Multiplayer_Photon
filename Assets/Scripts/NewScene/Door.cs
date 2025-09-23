using Fusion;
using UnityEngine;

public class Door : NetworkBehaviour
{
    public Material defaultMaterial;
    public Material goalMaterial;
    public GameObject portalPrefab;
    [SerializeField]
    private GameObject _minimapIndicator;

    private MeshRenderer meshRenderer;
    public bool IsGoal => isGoal;
    private bool isGoal;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = defaultMaterial;
    }

    public void SetAsGoal()
    {
        isGoal = true;
        meshRenderer.material = goalMaterial;

        Vector3 localPos = new Vector3(-3, 1.3f, 0.2f);

        // Instantiate as child without worldPositionStays
        GameObject portal = Instantiate(portalPrefab, transform);

        // Assign local position & rotation
        portal.transform.localPosition = localPos;
        _minimapIndicator.SetActive(true);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!isGoal) return;

        var player = other.GetComponent<PlayerNetworkController>();
        if (player != null && GameManager.Instance.Runner.IsServer)
        {
            GameManager.Instance.SetWinner(player.Object.InputAuthority, player.PlayerName);
        }
    }
}