#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using MyApp.BowlingKit.Extera;
namespace MyApp.BowlingKit
{
    public class BowlingMenu : MainMenu
    {
        #region game manager
        [MenuItem(Root + "/Game manager/" + MainMenuItem.AddAndSelect)]
        public static void AddGameManager()
        {
            CreateComponentIfNotExits<BowlingGameManager>("_Bowling GameManager");
        }
        #endregion
        #region ball
        [MenuItem(Root + "/Ball/" + MainMenuItem.AddAndSelect)]
        public static void AddBall()
        {
            CreateComponentIfNotExits<BowlingBall>("_Bowling GameManager");
        }
        [MenuItem(Root + "/Ball/" + MainMenuItem.AttachAndSelect)]
        public static void AttachBall()
        {
            var node = FindObjectOfType<BowlingBall>();
            if (node == null)
            {
                if (Selection.objects != null && Selection.objects.Length > 1)
                {
                    Selection.objects = new Object[] { Selection.objects[0] };
                }
                AttachComponentToSelection<BowlingBall>();
            }
            else
            {
                Selection.objects = new Object[] { node.gameObject };
            }
        }
        [MenuItem(Root + "/Ball/" + MainMenuItem.RemoveAll)]
        public static void RemoveBall()
        {
            RemoveComponentFromSelection<BowlingBall>();
        }
        #endregion
        #region pin
        [MenuItem(Root + "/Pin/" + MainMenuItem.Attach)]
        public static void AttachPin()
        {
            AttachComponentToSelection<BowlingPin>();
        }
        [MenuItem(Root + "/Pin/" + MainMenuItem.RemoveAll)]
        public static void RemovePin()
        {
            RemoveComponentFromSelection<BowlingPin>();
        }
        [MenuItem(Root + "/Pin/" + MainMenuItem.SelectAll)]
        public static void SelectPins()
        {
            SelectAllObjectsByComponent<BowlingPin>();
        }
        #endregion
        #region pin
        [MenuItem(Root + "/Accelerator sensor/" + MainMenuItem.Create)]
        public static void CreateAcceleratorSensor()
        {
            var node = new GameObject("Accelerator sensor");
            node.transform.position = getPosition();
            var _cc=node.AddComponent<CapsuleCollider>();
            _cc.transform.localScale = Vector3.one + Vector3.up * 3f;
            node.AddComponent<AcceleratorSensor>();
            Undo.RegisterCreatedObjectUndo(node, "Create Accelerator sensor");
            Selection.objects = new Object[] { node };
        }
        [MenuItem(Root + "/Accelerator sensor/" + MainMenuItem.Attach)]
        public static void AttachAcceleratorSensor()
        {
            AttachComponentToSelection<AcceleratorSensor>();
        }
        [MenuItem(Root + "/Accelerator sensor/" + MainMenuItem.RemoveAll)]
        public static void RemoveAcceleratorSensor()
        {
            RemoveComponentFromSelection<AcceleratorSensor>();
        }
        [MenuItem(Root + "/Accelerator sensor/" + MainMenuItem.SelectAll)]
        public static void SelectAcceleratorSensors()
        {
            SelectAllObjectsByComponent<AcceleratorSensor>();
        }
        #endregion
    }
}
#endif