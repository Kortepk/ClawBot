using ClawBot.Controls;
using HelixToolkit.Wpf;
using System;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private ArduinoPort _arduinoPort = new ArduinoPort();

        public MainWindow()
        {
            InitializeComponent();

            _controller = new RobotArmController(viewport);

            RefreshPorts_Click(null, null);
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
                    Color.FromRgb(64, 64, 64),
                    -90
                );
                _armPart3 = _controller.AddChildPart(
                    _armPart2,
                    Path.Combine(modelsDir, "Arm3.obj"),
                    new Vector3D(1, 0, 0),
                    new Point3D(0, 0, 90),
                    Color.FromRgb(64, 64, 64),
                    90
                );
                _armPart4 = _controller.AddChildPart(
                    _armPart3,
                    Path.Combine(modelsDir, "Arm4.obj"),
                    new Vector3D(1, 0, 0),
                    new Point3D(0, 0, 125),
                    Color.FromRgb(64, 64, 64),
                    90
                );
                _clawPart1 = _controller.AddChildPart(
                    _armPart4,
                    Path.Combine(modelsDir, "Claw1.obj"),
                    new Vector3D(0, 1, 0),
                    new Point3D(15, -7, 45),
                    Colors.Green
                );
                _clawPart2 = _controller.AddChildPart(
                    _armPart4,
                    Path.Combine(modelsDir, "Claw2.obj"),
                    new Vector3D(0, 1, 0),
                    new Point3D(-15, -7, 45),
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

        // Вращеине базы
        private void rotationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = Convert.ToInt32(e.NewValue);
            _controller.UpdatePart(_platformPart, val);
            _arduinoPort.SendServoCommand(1, val);
        }

        // Вращение плеча
        private void shoulderSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = Convert.ToInt32(e.NewValue);
            _controller.UpdatePart(_armPart2, val);
            _arduinoPort.SendServoCommand(4, val);
        }

        // Вращение локтя
        private void elbowSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = Convert.ToInt32(e.NewValue);
            _controller.UpdatePart(_armPart3, -val);
            _arduinoPort.SendServoCommand(5, val);

        }

        // Вращение запястья
        private void wristSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = Convert.ToInt32(e.NewValue);
            _controller.UpdatePart(_armPart4, -val);
            _arduinoPort.SendServoCommand(3, val);

        }

        // Вращение захвата
        private void gripSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = Convert.ToInt32(e.NewValue);
            _controller.UpdatePart(_clawPart1, val);
            _controller.UpdatePart(_clawPart2, -val);
            _arduinoPort.SendServoCommand(2, val);
        }

        private void Slider_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var slider = sender as Slider;
            if (slider != null)
            {
                slider.Value += e.Delta > 0 ? 1 : -1;
                e.Handled = true;
            }
        }

        // Кнопка нажатия на обновление портов
        private void RefreshPorts_Click(object sender, RoutedEventArgs e)
        {
            string[] port_list = SerialPort.GetPortNames();
            PortsComboBox.Items.Clear();
            foreach (var i in port_list)
            {
                PortsComboBox.Items.Add(i);
                Console.WriteLine(i);
            }

            // Обновляет список портов, если порт добавлен или удалён  
            if (PortsComboBox.Items.Count <= 1)
                PortsComboBox.SelectedIndex = 0;

        }

        private void ConnectCom_Click(object sender, RoutedEventArgs e)
        {
            string butStr = ConnectCom.Content.ToString();
            if (butStr == "Подключиться")
            {
                bool ret = _arduinoPort.Connect(PortsComboBox.SelectedItem.ToString());
                if(ret)
                    ConnectCom.Content = "Отключится";
            }
            else
            {
                _arduinoPort.Disconnect();
                ConnectCom.Content = "Подключиться";
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _arduinoPort.Dispose();
        }
    }
}
