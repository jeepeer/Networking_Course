using System.Collections;
using System.Collections.Generic;
using Alteruna;
using UnityEngine.UI;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    [SerializeField] private TeamManagerSync sync;
    [SerializeField] private TeamManagerUI teamUI;
    [SerializeField] private Button joinRedTeam;
    [SerializeField] private Button joinBlueTeam;

    [SerializeField] private Alteruna.Avatar avatar; // Alteruna Multiplayer SDK
    [SerializeField] private RocketLauncherGun gun; 
    private int currentTeam;

    private Multiplayer multiplayer; // Alteruna Multiplayer SDK

    private void Start()
    {
        teamUI.DisableUI();
        currentTeam = -1;

        if (avatar.IsMe)
        {
            teamUI.EnableUI();
            StartCoroutine(SetUpUI());
        }

        if (multiplayer == null)
        {
            multiplayer = FindObjectOfType<Multiplayer>();
            if (multiplayer == null)
            {
                Debug.LogError("Unable to find a active object of type Multiplayer.");
            }
        }

        // Multiplayer setup
        if (multiplayer)
        {
            // Events for other players joining and leaving
            multiplayer.OtherUserJoined.AddListener(HandleJoined);
            multiplayer.OtherUserLeft.AddListener(OtherPlayerLeft); 
        }

        // Button Setup
        joinRedTeam.onClick.AddListener(() => { JoinTeam((int)Team.red); });
        joinBlueTeam.onClick.AddListener(() => { JoinTeam((int)Team.blue); });
    }

    public void JoinTeam(int team)
    {
        switch (team)
        {
            case (int)Team.red:
                sync.AssignTeam((int)Team.red);
                sync.UpdateTeamSize();
                sync.HandleColorChange();
                currentTeam = (int)Team.red;
                break;

            case (int)Team.blue:
                sync.AssignTeam((int)Team.blue);
                sync.UpdateTeamSize();
                sync.HandleColorChange();
                currentTeam = (int)Team.blue;
                break;
        }

        // Remove the team UI
        teamUI.DisableUI();

        // Re-enable the mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        gun.bJoinedTeam = true;

        // Remove buttons
        joinRedTeam.gameObject.SetActive(false);
        joinBlueTeam.gameObject.SetActive(false);
    }

    IEnumerator SetUpUI()
    {
        // 1f to make sure UpdateNewPlayer() has the time to run
        yield return new WaitForSeconds(1f);
        sync.UpdateTeamSize();

        int redTeamSize = sync.GetRedTeamSize();
        int blueTeamSize = sync.GetBlueTeamSize();

        teamUI.UpdateTeamUI(redTeamSize, blueTeamSize);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HandleJoined(Multiplayer multiplayer, User user)
    {
        StartCoroutine(UpdateNewPlayer());
    }

    IEnumerator UpdateNewPlayer()
    {
        // 0.5f so it doesn't overlap with OtherUserLeft 
        yield return new WaitForSeconds(0.5f);
        sync.AssignTeam(currentTeam);
        sync.UpdateTeamSize();
        sync.HandleColorChange();
    }

    private void OtherPlayerLeft(Multiplayer multiplayer, User user)
    {
        StartCoroutine(UpdatePlayerLeft());
    }

    IEnumerator UpdatePlayerLeft()
    {
        yield return new WaitForSeconds(0.5f);
        sync.UpdateTeamSize();
    }
}