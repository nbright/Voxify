using UnityEngine;
using System;
using System.Reflection;

[assembly: AssemblyVersion("0.1.*")]
public static class Util {

	public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck) {
		while (toCheck != null && toCheck != typeof(object)) {
			var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
			if (generic == cur) {
				return true;
			}
			toCheck = toCheck.BaseType;
		}
		return false;
	}

	public static Vector2 MainGameViewSize {
		get {
			Type T = Type.GetType("UnityEditor.GameView,UnityEditor");
			MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
			object Res = GetSizeOfMainGameView.Invoke(null,null);
			return (Vector2)Res;
		}
	}

	public static Vector2 ScreenSize {
		get {
#if UNITY_EDITOR
			return MainGameViewSize;
#else
			return new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
#endif
		}
	}

	public static string AssemblyVersion {
		get {
			
			return Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}
	}

	public static float SquareMagnitudeVector3(Vector3 vector, bool includeMagnitudeZ = true) {
		float mag = 0f;

		mag = (vector.x * vector.x) + (vector.y * vector.y);
		if (includeMagnitudeZ)
			mag += (vector.z * vector.z);

		return mag;
	}

	public static Vector3 SubtractVector3(Vector3 a, Vector3 b) {
		return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
	}

	public static Vector3 RoundVector3(Vector3 a) {
		return new Vector3(Mathf.Round(a.x), Mathf.Round(a.y), Mathf.Round(a.z));
	}
}