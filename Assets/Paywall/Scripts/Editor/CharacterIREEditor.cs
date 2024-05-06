using UnityEditor;

namespace Paywall.Editors {

    [CustomEditor(typeof(CharacterIRE), true)]
    public class CharacterIREEditor : Editor {
        private CharacterIRE Target {
            get {
                return (CharacterIRE)target;
            }
        }

        public override void OnInspectorGUI() {
            if (Target != null) {
                if (Target.MovementState != null && Target.ConditionState != null) {
                    EditorGUILayout.LabelField("Movement State", Target.MovementState.CurrentState.ToString());
                    EditorGUILayout.LabelField("Condition State", Target.ConditionState.CurrentState.ToString());
                }
            }
            base.OnInspectorGUI();
        }
    }
}
