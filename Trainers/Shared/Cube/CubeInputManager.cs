using HelixToolkit.Wpf;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace CubeForge.Trainers.Shared
{
    public sealed class CubeInputManager
    {
        private readonly HelixViewport3D viewPort;
        private readonly Point3D cameraTarget = new Point3D(0, 0, 0);

        private bool isAttached;
        private bool isDragging;
        private bool isTouchDragging;
        private Point lastMousePosition;
        private Point lastTouchPosition;
        private double cameraYaw = 45;
        private double cameraPitch = 30;
        private double cameraDistance = 13.86;

        public CubeInputManager(HelixViewport3D viewPort)
        {
            this.viewPort = viewPort;
        }

        public void UpdateCamera()
        {
            if (viewPort.Camera is not PerspectiveCamera camera)
                return;

            double yawRadians = cameraYaw * Math.PI / 180.0;
            double pitchRadians = cameraPitch * Math.PI / 180.0;

            double x = cameraTarget.X + cameraDistance * Math.Cos(pitchRadians) * Math.Cos(yawRadians);
            double y = cameraTarget.Y + cameraDistance * Math.Sin(pitchRadians);
            double z = cameraTarget.Z + cameraDistance * Math.Cos(pitchRadians) * Math.Sin(yawRadians);

            camera.Position = new Point3D(x, y, z);
            camera.LookDirection = cameraTarget - camera.Position;
            camera.UpDirection = new Vector3D(0, 1, 0);
        }

        public void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            isDragging = true;
            lastMousePosition = e.GetPosition(viewPort);
            viewPort.CaptureMouse();
            e.Handled = true;
        }

        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging)
                return;

            Point currentPosition = e.GetPosition(viewPort);
            double deltaX = currentPosition.X - lastMousePosition.X;
            double deltaY = currentPosition.Y - lastMousePosition.Y;

            cameraYaw -= deltaX * 0.35;
            cameraPitch += deltaY * 0.35;
            cameraPitch = Math.Clamp(cameraPitch, -85, 85);
            lastMousePosition = currentPosition;

            UpdateCamera();
            e.Handled = true;
        }

        public void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            viewPort.ReleaseMouseCapture();
            e.Handled = true;
        }

        public void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                cameraDistance -= 0.8;
            else
                cameraDistance += 0.8;

            cameraDistance = Math.Clamp(cameraDistance, 6, 30);
            UpdateCamera();
            e.Handled = true;
        }

        public void OnTouchDown(object sender, TouchEventArgs e)
        {
            isTouchDragging = true;
            lastTouchPosition = e.GetTouchPoint(viewPort).Position;
            viewPort.CaptureTouch(e.TouchDevice);
            e.Handled = true;
        }

        public void OnTouchMove(object sender, TouchEventArgs e)
        {
            if (!isTouchDragging)
                return;

            Point currentPosition = e.GetTouchPoint(viewPort).Position;
            double deltaX = currentPosition.X - lastTouchPosition.X;
            double deltaY = currentPosition.Y - lastTouchPosition.Y;

            cameraYaw += deltaX * 0.35;
            cameraPitch -= deltaY * 0.35;
            cameraPitch = Math.Clamp(cameraPitch, -85, 85);
            lastTouchPosition = currentPosition;

            UpdateCamera();
            e.Handled = true;
        }

        public void OnTouchUp(object sender, TouchEventArgs e)
        {
            isTouchDragging = false;
            viewPort.ReleaseTouchCapture(e.TouchDevice);
            e.Handled = true;
        }
    }
}
