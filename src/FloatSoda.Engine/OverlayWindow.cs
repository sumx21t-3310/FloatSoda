using System.Numerics;
using FloatSoda.Common.Layer;
using OVRSharp;
using OVRSharp.Math;
using Valve.VR;

namespace FloatSoda.Engine;

public interface IWindow : IDisposable
{
    string Key { get; }
    ILayer Root { get; set; }

    void Update();
}

public enum TrackingDevice
{
    World,
    HMD,
    LeftController,
    RightController,
}

public class OverlayWindow : IWindow
{
    private readonly Renderer _renderer;


    public Overlay Overlay { get; }

    public string Key => Overlay.Key;
    public ILayer? Root { get; set; }

    public Transform Transform { get; }

    public OverlayWindow(string key, string name, bool isDashboard, Renderer renderer, string? thumbnail = null)
    {
        _renderer = renderer;
        Overlay = new Overlay(key, name, isDashboard);
        Transform = new Transform(Overlay);

        if (isDashboard && thumbnail != null)
        {
            Overlay.SetTextureFromFile(thumbnail);
        }
        else
        {
            Overlay.Show();
        }
    }

    public void Update()
    {
        if (Root == null)
        {
            Console.WriteLine($"{Overlay.Key} is no root found.");
            return;
        }

        _renderer.Render(Root);

        var texture = new Texture_t
        {
            handle = _renderer.GetTextureHandle(),
            eType = ETextureType.OpenGL,
            eColorSpace = EColorSpace.Auto,
        };

        Overlay.SetTexture(texture);
    }


    public void Dispose() => Overlay.Destroy();
}

public class Transform(Overlay overlay)
{
    private uint _deviceIndex = OpenVR.k_unTrackedDeviceIndexInvalid;

    public TrackingDevice TrackingDevice
    {
        get;
        set
        {
            if (field == value) return;
            field = value;

            _deviceIndex = field switch
            {
                TrackingDevice.World => OpenVR.k_unTrackedDeviceIndexInvalid,
                TrackingDevice.HMD => OpenVR.k_unTrackedDeviceIndex_Hmd,
                TrackingDevice.LeftController => OpenVR.System.GetTrackedDeviceIndexForControllerRole(
                    ETrackedControllerRole.LeftHand),
                TrackingDevice.RightController => OpenVR.System.GetTrackedDeviceIndexForControllerRole(
                    ETrackedControllerRole.RightHand),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public Vector3 Position
    {
        get;
        set
        {
            field = value;
            Update();
        }
    } = Vector3.Zero;

    public Quaternion Rotation
    {
        get;
        set
        {
            field = value;
            Update();
        }
    } = Quaternion.Identity;

    public Vector3 Scale
    {
        get;
        set
        {
            field = value;
            Update();
        }
    } = Vector3.One;

    public void Update()
    {
        overlay.Transform = (Matrix4x4.CreateScale(Scale)
                             * Matrix4x4.CreateFromQuaternion(Rotation)
                             * Matrix4x4.CreateTranslation(Position)).ToHmdMatrix34_t();
    }
}