using Peque;
using Peque.Machines;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildPanel : MonoBehaviour
{
    public static BuildPanel Instance;
    public Transform spacePreviewer;

    public Material canBuildMat;
    public Material cannotBuildMat;

    private Vector3 previousMousePosition = Vector3.zero;
    private List<Vector3> currentBelt = new List<Vector3>();
    private MachineInfo selectedMachine;
    private bool isPlacing = false;

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        if (selectedMachine == null) {
            return;
        }
        RaycastHit hitInfo;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool gotHit = Physics.Raycast(ray, out hitInfo);

        if (gotHit && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
            moveSpacePreviewer(hitInfo.point);
        }
        if (Input.GetMouseButton(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() && gotHit) {
            isPlacing = true;
            // it's on the same place, not worth doing anything
            if (previousMousePosition == hitInfo.point) {
                return;
            }
            if (previousMousePosition == Vector3.zero) {
                previousMousePosition = hitInfo.point;
            }

            if (selectedMachine.type == Machine.Type.Belt) {
                placeBelt(hitInfo.point);
            } else {
                placeMachine(hitInfo.point);
            }
        } else if (Input.GetMouseButton(1) && gotHit) {
            removeMachine(hitInfo.point);
        }

        // on mouse release reset some vars
        if (Input.GetMouseButtonUp(0) && isPlacing) {
            validateBelt();
            isPlacing = false;
        }
    }

    public void showMachineInfo (Machine.Type type) {
        InfoPanel.Instance.setSelectedMachine(GameGrid.Instance.getMachineInfo(type));
    }

    private void moveSpacePreviewer (Vector3 position) {
        Vector3 gridPosition = GameGrid.Instance.GetNearestPointOnGrid(position);
        bool canPlaceIt = true;

        // machines may take multiple blocks, so we need to make sure all are available
        foreach (Vector3 blockPosition in GameGrid.Instance.getMachineBlocks(gridPosition, selectedMachine.type)) {
            if (!GameGrid.Instance.isAvailable(blockPosition)) {
                canPlaceIt = false;
                break;
            }
        }

        spacePreviewer.GetComponent<MeshRenderer>().material = canPlaceIt ? canBuildMat : cannotBuildMat;
        spacePreviewer.transform.position = getPlacingPosition(gridPosition);
    }

    private Vector3 getPlacingPosition (Vector3 gridPosition) {
        // to fit blocks bigger than 1x1 in a 1x1 grid, we make this hack
        return gridPosition + new Vector3((selectedMachine.x - 1) * 0.5f, 0, (selectedMachine.y - 1) * 0.5f);
    }

    private void validateBelt() {
        previousMousePosition = Vector3.zero;
        currentBelt = new List<Vector3>();
    }

    private void removeMachine(Vector3 clickPoint) {
        Vector3 gridPosition = GameGrid.Instance.GetNearestPointOnGrid(clickPoint);

        // avoid overlapping
        if (GameGrid.Instance.isAvailable(gridPosition)) {
            return;
        }

        GameGrid.Instance.getMachineAt(gridPosition).delete();
    }

    private void placeMachine (Vector3 clickPoint) {
        Vector3 gridPosition = GameGrid.Instance.GetNearestPointOnGrid(clickPoint);

        // machines may take multiple blocks, so we need to make sure all are available
        foreach (Vector3 blockPosition in GameGrid.Instance.getMachineBlocks(gridPosition, selectedMachine.type)) {
            // avoid overlapping
            if (gridPosition.y > 0.5f || !GameGrid.Instance.isAvailable(blockPosition)) {
                previousMousePosition = gridPosition;
                return;
            }
        }

        GameObject obj = Instantiate(selectedMachine.prefab, getPlacingPosition(gridPosition), Quaternion.identity);
        obj.name = gridPosition.ToString();
        Machine machine = new Machine(obj, selectedMachine.type, gridPosition);
        GameGrid.Instance.place(machine);
    }

    private void placeBelt(Vector3 clickPoint) {
        Vector3 finalPosition = GameGrid.Instance.GetNearestPointOnGrid(clickPoint);

        // avoid overlapping
        if (finalPosition.y > 0.5f || !GameGrid.Instance.isAvailable(finalPosition)) {
            // seems like he's trying to link existing belts
            if (previousMousePosition != finalPosition &&
                !GameGrid.Instance.isAvailable(previousMousePosition) &&
                GameGrid.Instance.getMachineAt(finalPosition) != GameGrid.Instance.getMachineAt(previousMousePosition)
                //GameGrid.Instance.getMachineAt(previousMousePosition).type == Machine.Type.Belt &&
                //GameGrid.Instance.getMachineAt(finalPosition).type == Machine.Type.Belt
                ) {
                GameGrid.Instance.getMachineAt(finalPosition).addConnection(previousMousePosition, Machine.ConnectionType.Input);
                GameGrid.Instance.getMachineAt(previousMousePosition).addConnection(finalPosition, Machine.ConnectionType.Output);
            }
            previousMousePosition = finalPosition;
            return;
        }

        Vector3 mouseDirection = finalPosition - previousMousePosition;
        Vector3 itemDirection = Vector3.zero;
        Machine.Direction direction = Machine.Direction.Right;

        if (mouseDirection.x < 0) {
            itemDirection.y = (float)Machine.Direction.Left;
            direction = Machine.Direction.Left;
        } else if (mouseDirection.z > 0) {
            itemDirection.y = (float)Machine.Direction.Up;
            direction = Machine.Direction.Up;
        } else if (mouseDirection.z < 0) {
            itemDirection.y = (float)Machine.Direction.Down;
            direction = Machine.Direction.Down;
        }

        GameObject obj = Instantiate(selectedMachine.prefab, finalPosition, Quaternion.Euler(itemDirection));
        obj.name = finalPosition.ToString();
        Belt machine = new Belt(obj);
        machine.direction = direction;

        // if player started dragging from an existing conveyor belt, link it to the new one
        if (previousMousePosition != Vector3.zero && 
            !currentBelt.Contains(previousMousePosition) && 
            !GameGrid.Instance.isAvailable(previousMousePosition) && 
            GameGrid.Instance.getMachineAt(previousMousePosition).type == Machine.Type.Belt) {
            if (currentBelt.Count > 0) {
                validatePreviousBlockDirection(GameGrid.Instance.getMachineAt(currentBelt.Last()), GameGrid.Instance.getMachineAt(previousMousePosition));

                GameGrid.Instance.getMachineAt(currentBelt.Last()).addConnection(previousMousePosition, Machine.ConnectionType.Output);
                GameGrid.Instance.getMachineAt(previousMousePosition).addConnection(currentBelt.Last(), Machine.ConnectionType.Input);
            }
            currentBelt.Add(previousMousePosition);
        }

        GameGrid.Instance.place(machine);

        Belt lastMachine = null;

        if (currentBelt.Count > 0) {
            lastMachine = (Belt)GameGrid.Instance.machines[currentBelt.Last()];

            // new item and latest one are not neighbors
            if (!GameGrid.Instance.getNeighbors(finalPosition).Contains(lastMachine.position)) {
                var lastItemNeighbors = GameGrid.Instance.getNeighbors(lastMachine.position);

                // find a common available neighbor to create the union
                foreach (Vector3 pos in GameGrid.Instance.getNeighbors(finalPosition)) {
                    if (lastItemNeighbors.Contains(pos) && GameGrid.Instance.isAvailable(pos)) {
                        placeBelt(pos);

                        // refresh last item
                        lastMachine = (Belt)GameGrid.Instance.machines[currentBelt.Last()];
                        break;
                    }
                }
            }
        }

        currentBelt.Add(finalPosition);

        if (lastMachine != null) {
            validatePreviousBlockDirection(lastMachine, machine);

            // connect them
            GameGrid.Instance.getMachineAt(lastMachine.position).addConnection(finalPosition, Machine.ConnectionType.Output);
            GameGrid.Instance.getMachineAt(finalPosition).addConnection(lastMachine.position, Machine.ConnectionType.Input);
        }

        previousMousePosition = finalPosition;
    }

    void validatePreviousBlockDirection(Machine previousMachine, Machine nextMachine) {
        // change previous block direction if necessary
        if (previousMachine.position.z == nextMachine.position.z) { // horizontal relation --
            Vector3 newRotation = Vector3.zero;
            Machine.Direction newDirection = (previousMachine.position.x > nextMachine.position.x) ? Machine.Direction.Left : Machine.Direction.Right;

            newRotation.y = (float)newDirection;
            GameGrid.Instance.getMachineAt(previousMachine.position).gameObject.transform.rotation = Quaternion.Euler(newRotation);
            GameGrid.Instance.getMachineAt(previousMachine.position).direction = newDirection;

        } else if (previousMachine.position.x == nextMachine.position.x && previousMachine.direction != nextMachine.direction) { // vertical relation :
            GameGrid.Instance.getMachineAt(previousMachine.position).gameObject.transform.rotation = nextMachine.gameObject.transform.rotation;
            GameGrid.Instance.getMachineAt(previousMachine.position).direction = nextMachine.direction;
        }
    }

    public void select(Machine.Type type) {
        selectedMachine = GameGrid.Instance.getMachineInfo(type);
        showSpacePreviewer();
    }

    public void stopBuilding () {
        selectedMachine = null;
        spacePreviewer.gameObject.SetActive(false);
    }

    private void showSpacePreviewer () {
        spacePreviewer.GetComponent<MeshRenderer>().material = cannotBuildMat;
        spacePreviewer.gameObject.SetActive(true);
        spacePreviewer.localScale = new Vector3(selectedMachine.x, 1, selectedMachine.y);
    }
}
