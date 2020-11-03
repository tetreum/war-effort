using Peque;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    public static InfoPanel Instance;
    public Text machineName;
    public Text description;
    public Text storedItems;
    public Text money;

    private Machine selectedMachine;
    private MachineInfo selectedMachineInfo;

    private bool updaterStarted = false;

    private void Awake() {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
            trySelectMachine();
        }
    }

    private void trySelectMachine() {
        RaycastHit hitInfo;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool gotHit = Physics.Raycast(ray, out hitInfo);

        if (!gotHit) {
            return;
        }

        Vector3 gridPosition = GameGrid.Instance.GetNearestPointOnGrid(hitInfo.point);

        if (GameGrid.Instance.isAvailable(gridPosition)) {
            return;
        }

        if (!updaterStarted) {
            updaterStarted = true;
            InvokeRepeating("updateShownInfo", 0, 1);
        }

        selectedMachine = GameGrid.Instance.getMachineAt(gridPosition);
        setSelectedMachine(selectedMachine.info);
    }

    public void setSelectedMachine (MachineInfo machineInfo) {
        selectedMachineInfo = machineInfo;

        machineName.text = machineInfo.name;
        description.text = machineInfo.description;

        if (machineInfo.executionType == Machine.ExecutionType.Converter) {
            description.text += " | Produces ";
        }
    }

    private void updateShownInfo () {
        string storedItemsSummary = "";

        foreach (KeyValuePair<Item.Type, int> storedItem in selectedMachine.storedItems) {
            storedItemsSummary += " " + storedItem.Key + ": " + storedItem.Value + "/" + selectedMachineInfo.storageLimits[storedItem.Key];
        }
        storedItems.text = storedItemsSummary;
        money.text = GameGrid.Instance.money + "$";
    }
}
