using SFML.Window;
using SFML.System;
using SFML.Audio;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using static SFML_NET_3D.Data;
using static SFML_NET_3D.Constants;
using static SFML_NET_3D.Utility;

namespace SFML_NET_3D
{
    class Game
    {
        NoiseFactors noiseFactors;
        float[] noise;
        Box[,,] boxes;
        List<Box> boxList;
        Vector3f boxCount = new Vector3f(20, 1, 20);
        Vector3f boxSize = new Vector3f(12, 12, 12);

        bool mousePressed = false;
        Vector2i mousePrePos = new Vector2i(0, 0);

        public Game()
        {
            Awake();
            Run();
        }

        private void Awake()
        {
            window.SetFramerateLimit(30);
            window.Closed += OnClosed;
            window.KeyPressed += OnKeyPressed;
            window.MouseButtonPressed += OnMouseButtonPressed;
            window.MouseButtonReleased += OnMouseButtonReleased;
            window.MouseMoved += OnMousePointerMoved;

            Initialize();
        }

        private void Initialize()
        {
            noiseFactors = new NoiseFactors
            (
                size: 800,
                octave: 10,
                interval: 10,
                randomSeed: new Random().Next(255),
                softness: 2.35f
            );
            noise = Noise(noiseFactors);
            Renderer.Clear();

            boxes = new Box[(int)boxCount.X, (int)boxCount.Y, (int)boxCount.Z];
            for (int x = 0; x < (int)boxCount.X; x++)
                for (int y = 0; y < (int)boxCount.Y; y++)
                    for (int z = 0; z < (int)boxCount.Z; z++)
                        SetBoxState(x, y, z);

            boxList = new List<Box>();

            foreach (var box in boxes)
                boxList.Add(box);
        }

        private void SetBoxState(int x, int y, int z)
        {
            boxes[x, y, z] = new Box
            (
                size: boxSize,
                position: new Vector3f
                (
                    x * boxSize.X - ((boxCount.X - 1) * boxSize.X) / 2,
                    y * boxSize.Y - ((boxCount.Y - 1) * boxSize.Y) / 2,
                    z * boxSize.Z - ((boxCount.Z - 1) * boxSize.Z) / 2 + 0.0001f // Preventing Zero Exeption
                ),
                rotation: new Vector3f
                (
                    MathF.PI / 4f,
                    -MathF.Atan(1 / MathF.Sqrt(2)),
                    0
                ),
                fillColor: new Color
                (
                    (byte)(Map(x, 0, boxCount.X, 0, 30) + 150),
                    (byte)(Map(y, 0, boxCount.Y, 0, 30) + 50),
                    (byte)(Map(z, 0, boxCount.Z, 0, 30) + 30)
                ),
                type: PrimitiveType.Quads
            );
        }

        float offset1 = 0;
        private void Update()
        {
            for (int x = 0; x < boxCount.X; x++)
            {
                for (int y = 0; y < boxCount.Y; y++)
                {
                    for (int z = 0; z < boxCount.Z; z++)
                    {
                        float dist = Distnace(new Vector2f(x + 0.5f, z + 0.5f), new Vector2f(boxCount.X / 2, boxCount.Z / 2));
                        float offset2 = Map(dist, 0, Distnace(new Vector2f(0, 0), new Vector2f(boxCount.X / 2, boxCount.Z / 2)), 0, 1000);
                        float height = 180 + 90 * Map(noise[(int)(offset1 + offset2) % noiseFactors.Size], GetMin(noise), GetMax(noise), -1, 1);
                        boxes[x, y, z].SetSize(new Vector3f(boxSize.X, height, boxSize.Y));
                    }
                }
            }

            foreach (var box in boxList)
                box.Update();

            if (offset1 <= noiseFactors.Size - noiseFactors.Interval * 1.01f)
                offset1 += noiseFactors.Interval;
            else
                offset1 = 0;
        }

        private void Display()
        {
            Renderer.Render();

            for (int i = 1; i < noiseFactors.Size; i++)
            {
                VertexArray line = new VertexArray(PrimitiveType.Lines, 2);
                line[0] = new Vertex(new Vector2f
                (
                    Map(i, 0, noiseFactors.Size, 0, winSizeX),
                    Map(noise[((i - 1) + (int)offset1) % noiseFactors.Size], GetMin(noise), GetMax(noise), -winSizeY * 0.25f, winSizeY * 0.25f) + winSizeY * 0.5f
                ), Color.White);
                line[1] = new Vertex(new Vector2f
                (
                    Map(i, 0, noiseFactors.Size, 0, winSizeX),
                    Map(noise[(i + (int)offset1) % noiseFactors.Size], GetMin(noise), GetMax(noise), -winSizeY * 0.25f, winSizeY * 0.25f) + winSizeY * 0.5f
                ), Color.White);
                window.Draw(line);
            }

            window.Display();
            window.Clear(new Color(25, 25, 25));
        }

        private void LateUpdate()
        {
            foreach (var box in boxList)
            {
                box.LateUpdate();
            }
        }

        private void Run()
        {
            while (window.IsOpen)
            {
                HandleEvent();
                Update();
                Display();
                LateUpdate();
            }
        }

        #region EVENTS 
        private void HandleEvent()
        {
            window.DispatchEvents();
            DispatchEventImmediately();
        }

        private void DispatchEventImmediately()
        {
            OnKeyPressed();
        }

        private void OnKeyPressed()
        {
            if (Keyboard.IsKeyPressed(Keyboard.Key.W))
            {
                foreach (var box in boxList)
                    box.Rotation += new Vector3f(0, 0.05f, 0);
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.A))
            {
                foreach (var box in boxList)
                    box.Rotation += new Vector3f(0.05f, 0, 0);
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.S))
            {
                foreach (var box in boxList)
                    box.Rotation += new Vector3f(0, -0.05f, 0);
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.D))
            {
                foreach (var box in boxList)
                    box.Rotation += new Vector3f(-0.05f, 0, 0);
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Q))
            {
                foreach (var box in boxList)
                    box.Rotation += new Vector3f(0, 0, 0.05f);
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.E))
            {
                foreach (var box in boxList)
                    box.Rotation += new Vector3f(0, 0, -0.05f);
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Up))
            {
                foreach (var box in boxList)
                    box.Position += new Vector3f(0, 0, 10);
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Left))
            {
                foreach (var box in boxList)
                    box.Position += new Vector3f(-10, 0, 0);
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Down))
            {
                foreach (var box in boxList)
                    box.Position += new Vector3f(0, 0, -10);
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Right))
            {
                foreach (var box in boxList)
                    box.Position += new Vector3f(10, 0, 0);
            }
        }

        private void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Code == Keyboard.Key.Escape || e.Code == Keyboard.Key.F4)
            {
                window.Close();
            }
            if (e.Code == Keyboard.Key.F5)
            {
                Initialize();
            }
            if (e.Code == Keyboard.Key.F6)
            {
                if (winViewMode == ViewMode.Perspective)
                {
                    winViewMode = ViewMode.Orthographic;
                    winTitle = "SFML.NET 3D - View Mode : Orthographic";
                    window.SetTitle(winTitle);
                }
                else
                {
                    winViewMode = ViewMode.Perspective;
                    winTitle = "SFML.NET 3D - View Mode : Perspective";
                    window.SetTitle(winTitle);
                }
            }
        }

        private void OnClosed(object sender, EventArgs e)
        {
            window.Close();
        }

        private void OnMouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == Mouse.Button.Right && !mousePressed)
            {
                mousePressed = true;
            }
        }

        private void OnMousePointerMoved(object sender, MouseMoveEventArgs e)
        {
            if (mousePressed)
            {
                foreach (var box in boxList)
                    box.Rotation = new Vector3f
                    (
                       Map(Mouse.GetPosition(window).X, 0, winSizeX, MathF.PI, -MathF.PI),
                       Map(Mouse.GetPosition(window).Y, 0, winSizeY, MathF.PI, -MathF.PI),
                       box.Rotation.Z
                    );
            }
        }

        private void OnMouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == Mouse.Button.Right && mousePressed)
            {
                mousePressed = false;
            }
        }
        #endregion
    }
}