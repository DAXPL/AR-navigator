using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchDestinationUI : MonoBehaviour
{
    [SerializeField] private Button selectDestinationButton;
    [SerializeField] private Transform buttonHolder;
    [SerializeField] private Toggle wheelchairToggle;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnEndEdit(string input)
    {
        for (int i = buttonHolder.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonHolder.GetChild(i).gameObject);
        }

        if(NavigationManager.Instance == null) return;
        if(NavigationManager.Instance.xmlParser == null) return;

        List<Node> nodeList = NavigationManager.Instance.xmlParser.NodeList;

        // Lista przechowuj¹ca wszystkie pasuj¹ce NodeConnection
        List<NodeConnection> matchingConnections = new List<NodeConnection>();

        // Przeszukaj ka¿dy wêze³ i jego po³¹czenia wewnêtrzne
        foreach (Node node in nodeList)
        {
            // ZnajdŸ wszystkie dopasowane NodeConnection
            List<NodeConnection> connections = node.ConnectedNodes.FindAll(conn =>
                conn.To.Contains(input, StringComparison.OrdinalIgnoreCase)
            );

            // Dodaj pasuj¹ce po³¹czenia do listy wynikowej
            matchingConnections.AddRange(connections);
        }

        // Filtrowanie, aby uzyskaæ tylko unikalne nazwy 'To'
        var uniqueConnections = new List<NodeConnection>();
        var seenNames = new HashSet<string>();

        foreach (var connection in matchingConnections)
        {
            if (seenNames.Add(connection.To)) // Add zwraca true, jeœli element jest nowy
            {
                uniqueConnections.Add(connection);
            }
        }

        // Twórz przyciski dla ka¿dego unikalnego dopasowanego po³¹czenia
        foreach (NodeConnection connection in uniqueConnections)
        {
            Button newButton = Instantiate(selectDestinationButton, buttonHolder);
            newButton.GetComponentInChildren<TextMeshProUGUI>().SetText($"{connection.To}"); // Wyœwietl nazwê wêz³a docelowego
            newButton.onClick.AddListener(() => OnDestinationSelected(connection.To));
        }
    }
    private void OnDestinationSelected(string destinationName)
    {
        if (NavigationManager.Instance == null) return;
        NavigationManager.Instance.NavigateTo(destinationName, wheelchairToggle.isOn);
    }
    public void OpenWebsite(string url)
    {
        Application.OpenURL(url);
    }

    public void ToggleAnimatorBoolean(string booleanName)
    {
        if(animator == null) return;
        bool curVal = animator.GetBool(booleanName);
        animator.SetBool(booleanName, !curVal);
    }

    public void SetAnimatorBoolean(string booleanName)
    {
        if (animator == null) return;
        animator.SetBool(booleanName, true);
    }

    public void ResetAnimatorBoolean(string booleanName)
    {
        if (animator == null) return;
        animator.SetBool(booleanName, false);
    }
    public void SetAnimatorBooleanValue(string booleanName, bool val)
    {
        if (animator == null) return;
        animator.SetBool(booleanName, val);
    }
}
