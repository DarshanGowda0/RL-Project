using UnityEngine;

namespace MyApp
{
    public static class Extensions
    {
        public static bool isPlaying()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorApplication.isPlaying;
#else
         return Application.isPlaying;
#endif
        }
        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
        }
        public static Vector3 toVector3(this float n)
        {
            return Vector3.one * n;
        }
        public static void Swap<T>(ref T a, ref T b)
        {
            T t = a;
            a = b;
            b = t;
        }
    }
}