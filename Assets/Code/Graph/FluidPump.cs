using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidPump : FluidPipeEdge {
    public MultiWaySwitch lever;

    /* note: pump flows from v1 towards v2 */

    protected new void FixedUpdate() {
        base.FixedUpdate();
        if (lever.State > 0) {
            foreach (FluidPipeEdge edge in v2.edges) {
                container.Equalize(edge.container, false);
            }
            foreach (FluidPipeEdge edge in v1.edges) {
                edge.container.Equalize(container, false);
            }
        }
    }
}
