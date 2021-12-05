#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MyApp.BowlingKit.Editors
{
    [CustomEditor(typeof(BowlingBall))]
    public class BowlingBallEditor : Editor
    {
        #region variable
        protected BowlingBall Target;
        #endregion
        private void OnEnable()
        {
            Target = (BowlingBall)target;
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
                UI();
                Shadow();
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
            EditorTools.PropertyField(serializedObject, "maxMovementDistance", "Max movement distance", "Max distance that physics will be calculated.");
            EditorTools.PropertyField(serializedObject, "placementCollider", "Placement collider", "The place that ball can be drop over.");
            EditorTools.PropertyField(serializedObject, "maxForceDistance", "Max force distance", "Distance that with touch screen will be potential energy save.");

            EditorTools.PropertyField(serializedObject, "shootingMode", "Shooting mode");
            if (Target.shootingMode == BowlingBall.ShootingMode.Touching)
            {
                EditorTools.PropertyField(serializedObject, "minForceDistance", "Min force distance", "Distance that with touch screen will be not potential energy save.");
                if (GUILayout.Button("Auto calculate Min force distance"))
                {
                    Target.AutoCalculateMinForceDistance();
                }
            }
            else
            {
                EditorTools.PropertyField(serializedObject, "pivotAngle", "Pivot angle");
                EditorTools.PropertyField(serializedObject, "theta", "theta");
                EditorTools.PropertyField(serializedObject, "orbitDampening", "Orbit dampening");
                EditorTools.PropertyField(serializedObject, "powerDampening", "Power dampening"); 


            }

            EditorTools.Box_Close();
        }
        private void Force()
        {
            if (!EditorTools.Foldout(ref Target.showForceParts, "Force")) return;
            EditorTools.Box_Open();
            EditorTools.PropertyField(serializedObject, "potentialForceCoefficient", "Potential force Coefficient");
            EditorTools.PropertyField(serializedObject, "potentialForceSensitivity", "Potential force Sensitivity");
            EditorTools.PropertyField(serializedObject, "forceMode", "Force mode");
            EditorTools.PropertyField(serializedObject, "forceToStop", "Force to stop");
            if (Target.forceToStop)
            {
                EditorTools.PropertyField(serializedObject, "velocityThreshold", "Velocity threshold");
                EditorTools.PropertyField(serializedObject, "stopVelocityLerp", "Stop velocity lerp");
            }
            EditorTools.Box_Close();
        }
        private void UI()
        {
            if (!EditorTools.Foldout(ref Target.showUIParts, "UI")) return;
            EditorTools.Box_Open();
            EditorTools.PropertyField(serializedObject, "sliderCanvas", "Slider canvas");
            EditorTools.PropertyField(serializedObject, "slider", "Slider");
            EditorTools.PropertyField(serializedObject, "sliderFillAreaImage", "Slider fill area image");
            EditorTools.PropertyField(serializedObject, "solidColor", "Solid color");
            EditorTools.PropertyField(serializedObject, "color1", "Color1");
            if (!Target.solidColor)
            {
                EditorTools.PropertyField(serializedObject, "color2", "Color2");
            }
            EditorTools.Box_Close();
        }
        private void Shadow()
        {
            if (!EditorTools.Foldout(ref Target.showShadowParts, "Shadow")) return;
            EditorTools.Box_Open();
            EditorTools.PropertyField(serializedObject, "shadowEnable", "Enable");
            if (Target.shadowEnable)
            {
                EditorTools.PropertyField(serializedObject, "shadowPrefab", "Shadow prefab");
                EditorTools.PropertyField(serializedObject, "shadowScale", "Shadow local scale");
                EditorTools.PropertyField(serializedObject, "shadowTTL", "Time To Life (TTL)");
                EditorTools.PropertyField(serializedObject, "shadowMode", "Shadow mode");
                if (Target.shadowMode == BowlingBall.ShadowMode.ByTime)
                {
                    EditorTools.PropertyField(serializedObject, "shadowInstantiateRate", "Instantiate rate");
                }
                else if (Target.shadowMode == BowlingBall.ShadowMode.ByDistance)
                {
                    EditorTools.PropertyField(serializedObject, "shadowInstantiateDistance", "Instantiate distance");
                }
            }
            EditorTools.Box_Close();
        }
        private void Info()
        {
            if (!EditorTools.Foldout(ref Target.showInfoParts, "Infomation")) return;
            EditorTools.Box_Open();
            EditorTools.Info("State", Target._ballState.ToString());
            EditorTools.Info("Event", Target._ballEvent.ToString());
            EditorTools.Box_Close();
        }
        #endregion
    }
}
#endif