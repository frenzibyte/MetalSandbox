using AppKit;
using macOSApplication;

// This is the main entry point of the application.
NSApplication.Init();

var app = NSApplication.SharedApplication;
var del = new AppDelegate();
app.Delegate = del;

NSApplication.Main(args);