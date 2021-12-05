#if UNITY_EDITOR
using UnityEditor;

namespace MyApp.BowlingKit.Editors
{
    [CustomEditor(typeof(BowlingGameManager))]
    public class BowlingGameManagerEditor : Editor
    {
        #region variable
        protected BowlingGameManager Target;
        #endregion
        private void OnEnable()
        {
            Target = (BowlingGameManager)target;
        }
        #region Inspector
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Parameter();
            Events();
            Info();

            serializedObject.ApplyModifiedProperties();
        }
        private void Parameter()
        {
            if (!EditorTools.Foldout(ref Target.showParametersParts, "Parameter(s)")) return;
            EditorTools.Box_Open();
            EditorTools.PropertyField(serializedObject, "startFromBallHolder", "Start from ball holder");
            EditorTools.PropertyField(serializedObject, "updatePinsStateInRealTime", "Real-time pins state updating");
            EditorTools.PropertyField(serializedObject, "infinityShoots", "Infinity shoots");
            if (!Target.infinityShoots)
            {
                EditorTools.PropertyField(serializedObject, "maxShoots", "Max shoot(s)");
            }
            EditorTools.PropertyField(serializedObject, "isOverrideGroundLayer", "Overriding Ground mask");
            if (Target.isOverrideGroundLayer)
            {
                EditorTools.PropertyField(serializedObject, "groundMask", "Ground mask");
            }
            EditorTools.Box_Close();
        }
        private void Events()
        {
            if (!EditorTools.Foldout(ref Target.showEventsParts, "Event(s)")) return;
            EditorTools.Box_Open();
            EditorTools.PropertyField(serializedObject, "gameOverEventDelay", "Game is over event delay");
            EditorTools.PropertyField(serializedObject, "gameOverEvent", "Game is over event");
            EditorTools.Line();
            EditorTools.PropertyField(serializedObject, "gameFirstStartEventDelay", "Game first start event delay");
            EditorTools.PropertyField(serializedObject, "gameFirstStartEvent", "Game first start event");
            EditorTools.Line();
            EditorTools.PropertyField(serializedObject, "takeBallToBallHolderEventDelay", "Ball loaded from ball holder event delay");
            EditorTools.PropertyField(serializedObject, "takeBallToBallHolderEvent", "Ball loaded from ball holder event");
            EditorTools.Line();
            EditorTools.PropertyField(serializedObject, "shootsOverEventDelay", "Shoots over event delay");
            EditorTools.PropertyField(serializedObject, "shootsOverEvent", "Shoots over event");
            EditorTools.Line();
            EditorTools.PropertyField(serializedObject, "allPinsFallenEventDelay", "All pin(s) fallen delay");
            EditorTools.PropertyField(serializedObject, "allPinsFallenEvent", "All pin(s) fallen event");
            EditorTools.Box_Close();
        }
        private void Info()
        {
            if (!EditorTools.Foldout(ref Target.showInfoParts, "Infomation")) return;
            EditorTools.Box_Open();
            EditorTools.Info("Game is over", Target._gameIsOver.ToString());
            EditorTools.Info("Game event", Target._gameEvent.ToString());
            EditorTools.Info("Current shoot(s) count", Target._currentShoots.ToString());
            EditorTools.Info("Fallen pin(s) count", Target._fallenPinCount.ToString());
            EditorTools.Info("Standed pin(s) count", Target._standedPinCount.ToString());
            EditorTools.Box_Close();
        }
        #endregion
    }
}
#endif