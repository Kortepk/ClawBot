using ClawBot.Controls;
using HelixToolkit.Wpf;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace ClawBot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RobotArmController _controller;
        private RobotPart _mainPart,
            _arduinoPart,
            _platformPart,
            _armPart1,
            _armPart2,
            _armPart3,
            _armPart4,
            _clawPart1,
            _clawPart2;

        public MainWindow()
        {
            InitializeComponent();

            _controller = new RobotArmController(viewport);

            // Инициализация модели
            LoadModels();
        }

        private async void LoadModels()
        {
            try
            {
                string modelsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "model");

                // Базовая деталь
                _mainPart = _controller.AddRootPart(
                    Path.Combine(modelsDir, "Main.obj"),
                    new Vector3D(0, 1, 0),
                    new Point3D(0, 0, 0),
                    Color.FromRgb(241, 213, 174)
                );

                _arduinoPart = _controller.AddChildPart(
                    _mainPart,
                    Path.Combine(modelsDir, "Arduino.obj"),
                    new Vector3D(0, 0, 0),
                    new Point3D(70, 0, 75),
                    Colors.DarkCyan
                );

                _platformPart = _controller.AddChildPart(
                    _mainPart,
                    Path.Combine(modelsDir, "RotPlatform.obj"),
                    new Vector3D(0, 0, 1),
                    new Point3D(-30, 0, 75),
                    Colors.GhostWhite
                );

                _armPart1 = _controller.AddChildPart(
                    _platformPart,
                    Path.Combine(modelsDir, "Arm1.obj"),
                    new Vector3D(1, 0, 0),
                    new Point3D(0, 0, 30),
                    Color.FromRgb(64, 64, 64)
                );

                _armPart2 = _controller.AddChildPart(
                    _armPart1,
                    Path.Combine(modelsDir, "Arm2.obj"),
                    new Vector3D(1, 0, 0),
                    new Point3D(0, 0, 60),
                    Color.FromRgb(64, 64, 64)
                );
                _armPart3 = _controller.AddChildPart(
                    _armPart2,
                    Path.Combine(modelsDir, "Arm3.obj"),
                    new Vector3D(1, 0, 0),
                    new Point3D(0, 0, 90),
                    Color.FromRgb(64, 64, 64)
                );
                _armPart4 = _controller.AddChildPart(
                    _armPart3,
                    Path.Combine(modelsDir, "Arm4.obj"),
                    new Vector3D(1, 0, 0),
                    new Point3D(0, 0, 125),
                    Color.FromRgb(64, 64, 64)
                );
                _clawPart1 = _controller.AddChildPart(
                    _armPart4,
                    Path.Combine(modelsDir, "Claw1.obj"),
                    new Vector3D(1, 0, 0),
                    new Point3D(15, 0, 50),
                    Colors.Green
                );
                _clawPart2 = _controller.AddChildPart(
                    _armPart4,
                    Path.Combine(modelsDir, "Claw2.obj"),
                    new Vector3D(1, 0, 0),
                    new Point3D(-15, 0, 50),
                    Colors.Green
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }

            // Ждём обновления layout
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Loaded);

            viewport.ZoomExtents(0.7);
        }

        private void BaseSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _controller.UpdatePart(_mainPart, e.NewValue);
        }
        private void ArmSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _controller.UpdatePart(_platformPart, e.NewValue);
        }
    }
}
