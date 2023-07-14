// See https://aka.ms/new-console-template for more information

using SDL2;
using Veldrid;
using VeldridSandbox;

SDL.SDL_Init(SDL.SDL_INIT_VIDEO);

var window = SDL.SDL_CreateWindow("MetalSandbox (via NETApplication)", 0, 0, 1280,720, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_METAL | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

var renderer = new VeldridRenderer();
var view = SDL.SDL_Metal_CreateView(window);
renderer.Initialise(1280, 720, SwapchainSource.CreateNSView(view));

bool running = true;

new Thread(() =>
{
    while (running)
        renderer.Render();
}).Start();

while (running)
{
    if (SDL.SDL_PollEvent(out var @event) > 0 && @event.type == SDL.SDL_EventType.SDL_QUIT)
        running = false;
}

SDL.SDL_Metal_DestroyView(view);
SDL.SDL_DestroyWindow(window);