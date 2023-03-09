using UnityEngine;

namespace Icarus.UI {
    public class BaseControlAuthoring : MonoBehaviour {
        [Tooltip("Identifier of the Datum this control will write to. If DatumID begins with a period (.): it will search up the GameObject hierarchy for a ControlDatumPrefixAuthoring to use as a datum prefix. If DatumID is blank: it will also search down for a ControlLabelAuthoring to use as a suffix")]
        public string _DatumID;

        public string DatumID {
            get {
                if (_DatumID == "") {
                    return Prefix() + "." + Suffix();
                } else if(_DatumID.StartsWith(".")) {
                    return Prefix() + _DatumID;
                } else {
                    return _DatumID;
                }
            }
        }

        private string Prefix() {
            var comp = GetComponentInParent<ControlDatumPrefixAuthoring>();
            return (comp is null) ? "" : comp.Prefix;
        }

        private string Suffix() {
            var comp = GetComponentInChildren<ControlLabelAuthoring>();
            return (comp is null) ? "" : comp.DatumID;
        }
    }
}
