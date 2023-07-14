using System.Runtime.InteropServices;
using Foundation;
using UIKit;
using Veldrid;
using Veldrid.SPIRV;
using VeldridSandbox;

namespace iOSApplication;

[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
    public override UIWindow? Window { get; set; }

    private VeldridRenderer veldrid = null!;

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        NativeLibrary.SetDllImportResolver(typeof(SpirvCompilation).Assembly,
            (_, assembly, path) => NativeLibrary.Load("@rpath/veldrid-spirv.framework/veldrid-spirv", assembly, path));

        // create a new window instance based on the screen size
        Window = new UIWindow(UIScreen.MainScreen.Bounds);

        // create a UIViewController with a single UILabel
        var vc = new UIViewController();

        Window.RootViewController = vc;

        // make the window visible
        Window.MakeKeyAndVisible();

        veldrid = new VeldridRenderer();
        veldrid.Initialise((int)Window.Frame.Width, (int)Window.Frame.Height, SwapchainSource.CreateUIView(vc.View!.Handle.Handle));

        var link = UIScreen.MainScreen.CreateDisplayLink(veldrid.Render);
        link.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Default);
        return true;
    }
}