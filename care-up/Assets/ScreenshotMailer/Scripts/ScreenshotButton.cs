﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotButton : MonoBehaviour {


	
	// Update is called once per frame
	public void screenshotButtonPressed() {
        ScreenshotMailer.CaptureScreenshot();
	}
}
