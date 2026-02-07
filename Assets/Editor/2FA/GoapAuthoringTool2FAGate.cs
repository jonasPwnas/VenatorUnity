#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public class GoapAuthoringTool2FAGate : EditorWindow
{
    // Must match the key you already used when enrolling your authenticator
    private const string SecretPrefKey = "MyTool_2FA_Secret_Base32";

    private string _code = "";
    private string _status = "";

    [MenuItem("Tools/GOAP/GOAP Authoring Tool")]
    private static void OpenGate()
    {
        var w = GetWindow<GoapAuthoringTool2FAGate>(true, "GOAP Tool (2FA)", true);
        w.minSize = new Vector2(360, 150);
        w.ShowUtility();
    }

    private static bool TryGetSecret(out string secret)
    {
        secret = EditorPrefs.GetString(SecretPrefKey, "");
        return !string.IsNullOrEmpty(secret);
    }

    private static void OpenRealTool()
    {
        // Calls your existing window open code (unchanged)
        GoapAuthoringToolWindow.Open();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Enter your 2FA code to open GOAP Authoring Tool.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space(8);

        if (!TryGetSecret(out var secret))
        {
            EditorGUILayout.HelpBox(
                "2FA secret not found on this machine. (EditorPrefs key missing)\n" +
                "If you already enrolled on your phone, re-set the secret once on this machine.",
                MessageType.Error
            );
            return;
        }

        GUI.SetNextControlName("CodeField");
        _code = EditorGUILayout.TextField("6-digit code", _code);

        EditorGUILayout.Space(6);

        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_code)))
        {
            if (GUILayout.Button("Verify"))
            {
                bool ok = Totp.Verify(secret, _code, DateTimeOffset.UtcNow, allowedStepDrift: 1);

                if (ok)
                {
                    _status = "Verified!";
                    OpenRealTool();
                    Close(); // requires code again next time
                }
                else
                {
                    _status = "Invalid code.";
                    _code = "";
                    GUI.FocusControl("CodeField");
                }
            }
        }

        if (!string.IsNullOrEmpty(_status))
            EditorGUILayout.HelpBox(_status, _status.StartsWith("Y") ? MessageType.Info : MessageType.Error);
    }
}
#endif
