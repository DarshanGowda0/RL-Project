#if UNITY_EDITOR
using UnityEditor;

namespace MyApp.BowlingKit.Editors
{
    [CustomEditor(typeof(BowlingPin))]
    public class BowlingPinEditor : Editor
    {
        #region variable
        protected BowlingPin Target;
        #endregion
        private void OnEnable()
        {
            Target = (BowlingPin)target;
        }
        #region Inspector
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Enable();
            if (Target.enable)
            {
                Parameter();
                Force();
                Info();
            }
            serializedObject.ApplyModifiedProperties();
        }
        private void Enable()
        {
            EditorTools.Box_Open();
            EditorTools.PropertyField(serializedObject, "enable", "Enable");
            EditorTools.Box_Close();
        }
        private void Parameter()
        {
            if (!EditorTools.Foldout(ref Target.showParametersParts, "Parameter(s)")) return;
            EditorTools.Box_Open();
            EditorTools.PropertyField(serializedObject, "maxDistance", "Max movement distance", "Max distance that physics will be calculated.");
            EditorTools.PropertyField(serializedObject, "bottomVector", "Bottom vector", "The place that pin is touching ground.");
            EditorTools.PropertyField(serializedObject, "bottomConnectorRadius", "Bottom Vector size", "The radius of touching ground place.");
            EditorTools.PropertyField(serializedObject, "groundMask", "Ground mask");
            EditorTools.Box_Close();
        }
        private void Force()
        {
            if (!EditorTools.Foldout(ref Target.showForceParts, "Force")) return;
            EditorTools.Box_Open();
            EditorTools.PropertyField(serializedObject, "forceToStop", "Force to stop");
            if (Target.forceToStop)
            {
                EditorTools.PropertyField(serializedObject, "velocityThreshold", "Velocity threshold");
                EditorTools.PropertyField(serializedObject, "stopVelocityLerp", "Stop velocity lerp");
            }
            EditorTools.Box_Close();
        }
        private void Info()
        {
            if (!EditorTools.Foldout(ref Target.showInfoParts, "Infomation")) return;
            EditorTools.Box_Open();
            EditorTools.Info("State", Target._pinState.ToString());
            EditorTools.Box_Close();
        }
        #endregion
    }
}
#endif