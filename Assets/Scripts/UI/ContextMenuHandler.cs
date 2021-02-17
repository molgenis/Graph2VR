using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContextMenuHandler : MonoBehaviour
{
    public GameObject ContentPanel;
    Button buttonPrefab;
    InputField inputPrefab;

    // Start is called before the first frame update
    List<Button> buttons = new List<Button>();
    public delegate void OnItemIsSelected(string button);
    public event OnItemIsSelected itemSelected;

    void Start()
    {
        inputPrefab = Resources.Load<InputField>("UI/InputField");
        InputField inputField = Instantiate<InputField>(inputPrefab);
        inputField.transform.SetParent(ContentPanel.transform, false);

        buttonPrefab = Resources.Load<Button>("UI/Button");
        List<string> subjects = Graph.instance.GetSubjects();
        foreach (string subject in subjects)
        {
            AddButton(subject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddButton(string label)
    {
        Button button = Instantiate<Button>(buttonPrefab);
        button.transform.SetParent(ContentPanel.transform, false);
        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = label;

        button.onClick.AddListener(
            delegate
            {
                GetComponentInChildren<TMPro.TextMeshProUGUI>(true).text = label;
                itemSelected(label);
            });

        buttons.Add(button);
    }

    private void OnDisable()
    {
        foreach(Button button in buttons)
        {
            button.onClick.RemoveAllListeners();
        }
    }
}
