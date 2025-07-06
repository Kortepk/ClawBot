using ClawBot.Controls;
using HelixToolkit.Wpf;
using System;
using System.IO;
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
        private RobotPart _basePart;
        private RobotPart _childPart;

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
                _basePart = _controller.AddRootPart(
                    Path.Combine(modelsDir, "1.obj"),
                    new Vector3D(0, 1, 0),
                    new Point3D(0, 0, 0));

                _childPart = _controller.AddChildPart(
                    _basePart,
                    Path.Combine(modelsDir, "2.obj"),
                    new Vector3D(1, 0, 0),
                    new Point3D(0, 0, 25));
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
            int partIndex = int.Parse(((FrameworkElement)sender).Tag.ToString());
            _controller.UpdatePart(_basePart, e.NewValue);
            viewport.ZoomExtents();
        }
        private void ArmSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _controller.UpdatePart(_childPart, e.NewValue);
            viewport.ZoomExtents();
        }
    }
}
