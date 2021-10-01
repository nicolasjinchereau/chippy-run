using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class ScreenShotter : MonoBehaviour
{
    public int scale = 1;

    int screenshot = 0;

    void Start()
    {
        var screenshots = Directory.GetFiles("..", "Screenshot*.png");
        if(screenshots.Length > 0)
        {
            Array.Sort(screenshots);

            var fn = Path.GetFileNameWithoutExtension(screenshots[screenshots.Length - 1]);
            var numStr = fn.Substring("Screenshot".Length).Substring(0, 2);

            if(int.TryParse(numStr, out screenshot))
                ++screenshot;
        }

        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Return))
        {
            var res = "" + Screen.width + "x" + Screen.height + "@" + scale;
            var filename = "../Screenshot" + screenshot.ToString("00") + "_" + res + ".png";
            Debug.Log("Saving Screenshot: " + filename);
            ScreenCapture.CaptureScreenshot(filename, scale);
            ++screenshot;
        }

        if(Input.GetKeyUp(KeyCode.Home))
            Time.timeScale = 0;
        
        if(Input.GetKeyUp(KeyCode.End))
            Time.timeScale = 1;
    }
}
