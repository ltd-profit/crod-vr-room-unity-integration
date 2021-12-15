﻿using UnityEngine;

public class VRCaveCameraController : MonoBehaviour
{
    [SerializeField] [HideInInspector] private Camera UpCamera;
    [SerializeField] [HideInInspector] private Camera DownCamera;
    [SerializeField] [HideInInspector] private Camera LeftCamera;
    [SerializeField] [HideInInspector] private Camera RightCamera;
    [SerializeField] [HideInInspector] private Camera FrontCamera;
    [SerializeField] [HideInInspector] private Camera BackCamera;

    [SerializeField] private Vector3 roomSize;
    [SerializeField] private VRCaveDisplay frontDisplay;
    [SerializeField] private VRCaveDisplay backDisplay;
    [SerializeField] private VRCaveDisplay rightDisplay;
    [SerializeField] private VRCaveDisplay leftDisplay;
    [SerializeField] private VRCaveDisplay roofDisplay;
    [SerializeField] private VRCaveDisplay floorDisplay;

    private void Awake()
    {
        SetupDisplay(FrontCamera, frontDisplay);
        SetupDisplay(BackCamera, backDisplay);

        SetupDisplay(LeftCamera, leftDisplay);
        SetupDisplay(RightCamera, rightDisplay);

        SetupDisplay(UpCamera, roofDisplay);
        SetupDisplay(DownCamera, floorDisplay);
    }

    private void SetupDisplay(Camera targetCamera, VRCaveDisplay vrCaveDisplay)
    {
        if (vrCaveDisplay.Index == 0 || Application.isEditor) return;

        var display = Display.displays[vrCaveDisplay.Index - 1];
        display.Activate();
        Debug.Log($"VRCave: set target display #{vrCaveDisplay.Index} for camera {targetCamera.name}");
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        var (x, y, z) = GetRoomDimensions();

        if (!AnyDisplaySpecified())
            frontDisplay.Index = 1;

        InitializeCamera(FrontCamera, x, y, z, frontDisplay);
        InitializeCamera(BackCamera, x, y, z, backDisplay);

        InitializeCamera(LeftCamera, z, y, x, leftDisplay);
        InitializeCamera(RightCamera, z, y, x, rightDisplay);

        InitializeCamera(UpCamera, x, z, y, roofDisplay);
        InitializeCamera(DownCamera, x, z, y, floorDisplay);
    }

    private (float, float, float) GetRoomDimensions()
    {
        var (x, y, z) = (roomSize.x, roomSize.y, roomSize.z);

        if (x > 0 && y > 0 && z > 0) return (x, y, z);

        var screenResolution = Screen.currentResolution;
        var screenSize = new Vector2(screenResolution.width, screenResolution.height) / 100f;
        if (x == 0)
        {
            x = screenSize.x;
        }

        if (y == 0)
        {
            y = screenSize.y;
        }

        if (z == 0)
        {
            z = screenSize.x;
        }

        return (x, y, z);
    }

    private bool AnyDisplaySpecified()
    {
        return frontDisplay.Index != 0 ||
               backDisplay.Index != 0 ||
               rightDisplay.Index != 0 ||
               leftDisplay.Index != 0 ||
               roofDisplay.Index != 0 ||
               floorDisplay.Index != 0;
    }

    private void InitializeCamera(Camera targetCamera, float w, float h, float d, VRCaveDisplay display)
    {
        if (display.Index == 0)
        {
            targetCamera.gameObject.SetActive(false);
            return;
        }

        targetCamera.gameObject.SetActive(true);

        if (display.Rotation != 0)
        {
            var rotation = display.Rotation;
            targetCamera.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);

            var rotationStep = (int) Mathf.Round(rotation / 90f);
            if (rotationStep % 2 != 0)
                (w, h) = (h, w);
        }

        targetCamera.usePhysicalProperties = true;
        targetCamera.gateFit = Camera.GateFitMode.None;

        targetCamera.sensorSize = new Vector2(w, h);
        targetCamera.focalLength = d / 2f;

        targetCamera.targetDisplay = display.Index - 1;

        UnityEditor.EditorUtility.SetDirty(targetCamera);
    }
#endif
}