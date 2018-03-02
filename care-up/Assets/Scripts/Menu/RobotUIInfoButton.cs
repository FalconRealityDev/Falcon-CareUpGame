﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotUIInfoButton : MonoBehaviour {
    
    private static Paroxe.PdfRenderer.PDFViewer pdf;
    private Paroxe.PdfRenderer.PDFAsset asset;

    private void Awake()
    {
        if (pdf == null)
        {
            pdf = GameObject.FindObjectOfType<Paroxe.PdfRenderer.PDFViewer>();
        }
    }
    
    public void Set(string name)
    {
        asset = Resources.Load<Paroxe.PdfRenderer.PDFAsset>("Prefabs\\UI\\" + name);
    }

    public void Toggle(bool value)
    {
        pdf.PDFAsset = asset;
        pdf.ReloadDocument();
    }
}
