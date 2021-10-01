using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuHelpScreen : MonoBehaviour
{
    const string PrivacyPolicyURL = "https://www.timberfortress.ca/privacy-policy/";

    public Text policyURLText;

    void OnEnable() {
        
    }

    public void OnPrivacyPolicyPressed() {
        Application.OpenURL(PrivacyPolicyURL);
    }
}
