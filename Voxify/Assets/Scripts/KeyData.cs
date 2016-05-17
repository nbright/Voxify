using UnityEngine;

public enum KeyState {
	DOWN = 0,
	UP,
	HELD
}

/// <summary>
/// A type which holds information for a advanced keypresses.
/// Replaces traditional KeyCode and Input.Get use-cases.
/// 
/// KeyData will implicity cast to a bool based on if an instance.GetInput() returns true or false.
/// </summary>
public struct KeyData {
	KeyCode Key;
	KeyCode[] ModifierKeys;
	KeyState PreferredState; 

	public KeyData(KeyCode key, KeyState preferredState = KeyState.HELD, params KeyCode[] modKeys) {
		Key = key;
		PreferredState = preferredState;
		ModifierKeys = modKeys;
	}

	public bool GetInput() {
		foreach (KeyCode key in ModifierKeys) {
			switch (PreferredState) {
				case KeyState.DOWN:

					if (!Input.GetKeyDown(key))
						return false;
					break;
				case KeyState.HELD:

					if (!Input.GetKey(key))
						return false;
					break;
				case KeyState.UP:

					if (!Input.GetKeyUp(key))
						return false;
					break;
			}
		}
		switch (PreferredState) {
			case KeyState.DOWN:

				return Input.GetKeyDown(Key);
			case KeyState.HELD:

				return Input.GetKey(Key);
			case KeyState.UP:

				return Input.GetKeyUp(Key);
		}

		return false;
	}

	public static implicit operator bool(KeyData kd) {
		return kd.GetInput();
	}
}
