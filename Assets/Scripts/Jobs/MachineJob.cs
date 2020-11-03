using Peque;
using Unity.Jobs;
using UnityEngine;

public struct MachineJob : IJob
{

    public void Execute() {
        foreach (Machine machine in GameGrid.Instance.machines.Values) {
            /*if (machine.type == Machine.Type.Belt) {
                continue;
            }*/

            machine.run();
        }
    }
}