using System;
using System.Runtime.InteropServices;
using System.Threading;
using AppKit;
using CoreGraphics;
using Foundation;
using Veldrid;
using Veldrid.MetalBindings;
using Veldrid.SPIRV;
using VeldridSandbox;
using Vulkan.Xlib;
using CGPoint = CoreGraphics.CGPoint;
using CGRect = CoreGraphics.CGRect;
using NSView = AppKit.NSView;
using NSWindow = AppKit.NSWindow;

namespace macOSApplication;

[Register("AppDelegate")]
public class AppDelegate : NSApplicationDelegate
{
    private VeldridRenderer veldrid;

    public override unsafe void DidFinishLaunching(NSNotification notification)
    {
        NSWindow window = new NSWindow(new CGRect(0, 0, 1280, 720), NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled | NSWindowStyle.Miniaturizable, NSBackingStore.Buffered, false);
        window.MakeKeyAndOrderFront(this);
        window.Title = "MetalSandbox";

        veldrid = new VeldridRenderer();
        veldrid.Initialise((int)window.Frame.Width, (int)window.Frame.Height, SwapchainSource.CreateNSWindow(window.Handle.Handle));

        new Thread(() =>
        {
            while (true)
                veldrid.Render();
        }).Start();
    }

    public override void WillTerminate(NSNotification notification)
    {
    }
}