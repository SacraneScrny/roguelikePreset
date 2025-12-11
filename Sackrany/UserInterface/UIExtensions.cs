using System;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

namespace Sackrany.UserInterface
{
    public static class UIExtensions
    {
        public static Vector2 ActualSize(this Canvas canvas, Camera targetCamera = null)
        {
            if (!canvas.GetComponent<CanvasScaler>())
                return canvas.pixelRect.size;
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize) return canvas.pixelRect.size;
            if (scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
                return scaler.referenceResolution;

            float screenWidth = targetCamera == null ? Screen.width : targetCamera.pixelWidth;
            float screenHeight = targetCamera == null ? Screen.height : targetCamera.pixelHeight;

            var referenceWidth = scaler.referenceResolution.x;
            var referenceHeight = scaler.referenceResolution.y;

            var referenceAspect = referenceWidth / referenceHeight;

            var screenAspectToWidth = screenWidth / screenHeight / referenceAspect;
            var screenAspectToHeight = screenHeight / screenWidth / (1f / referenceAspect);

            var matchWidthOrHeight = scaler.matchWidthOrHeight;
            var widthCoef = matchWidthOrHeight;
            var heightCoef = 1f - matchWidthOrHeight;

            return new Vector2(
                Mathf.Lerp(referenceWidth, referenceWidth * screenAspectToWidth, widthCoef),
                Mathf.Lerp(referenceHeight, referenceHeight * screenAspectToHeight, heightCoef));
        }
    }
}