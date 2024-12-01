using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchDestinationUI : MonoBehaviour
{
    [SerializeField] private Button selectDestinationButton;
    [SerializeField] private Transform buttonHolder;

    [SerializeField] private Transform selectPanel;
    [SerializeField] private Transform optionsButton;
    [SerializeField] private Toggle wheelchairToggle;

    public void OnEndEdit(string input)
    {
        for (int i = buttonHolder.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonHolder.GetChild(i).gameObject);
        }

        if(NavigationManager.Instance == null) return;
        if(NavigationManager.Instance.xmlParser == null) return;

        List<Node> nodeList = NavigationManager.Instance.xmlParser.NodeList;

        // Lista przechowuj�ca wszystkie pasuj�ce NodeConnection
        List<NodeConnection> matchingConnections = new List<NodeConnection>();

        // Przeszukaj ka�dy w�ze� i jego po��czenia wewn�trzne
        foreach (Node node in nodeList)
        {
            // Znajd� wszystkie dopasowane NodeConnection
            List<NodeConnection> connections = node.ConnectedNodes.FindAll(conn =>
                conn.To.Contains(input, StringComparison.OrdinalIgnoreCase)
            );

            // Dodaj pasuj�ce po��czenia do listy wynikowej
            matchingConnections.AddRange(connections);
        }

        // Filtrowanie, aby uzyska� tylko unikalne nazwy 'To'
        var uniqueConnections = new List<NodeConnection>();
        var seenNames = new HashSet<string>();

        foreach (var connection in matchingConnections)
        {
            if (seenNames.Add(connection.To)) // Add zwraca true, je�li element jest nowy
            {
                uniqueConnections.Add(connection);
            }
        }

        // Tw�rz przyciski dla ka�dego unikalnego dopasowanego po��czenia
        foreach (NodeConnection connection in uniqueConnections)
        {
            Button newButton = Instantiate(selectDestinationButton, buttonHolder);
            newButton.GetComponentInChildren<TextMeshProUGUI>().SetText($"{connection.To}"); // Wy�wietl nazw� w�z�a docelowego
            newButton.onClick.AddListener(() => OnDestinationSelected(connection.To));
        }
    }
    private void OnDestinationSelected(string destinationName)
    {
        if (NavigationManager.Instance == null) return;
        NavigationManager.Instance.NavigateTo(destinationName, wheelchairToggle.isOn);

        if(selectPanel) selectPanel.gameObject.SetActive(false);
        if(optionsButton) optionsButton.gameObject.SetActive(true);
    }
    public void OpenWebsite(string url)
    {
        Application.OpenURL(url);
    }
}
