using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetwrokUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButten;

    private void Awake()
    {
        // clickable host 

        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();

        });

        // clickable client
        clientButten.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();

        });
    }
}
