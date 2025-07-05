using HelixToolkit.Wpf;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ClawBot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadModel();
            rotationSlider.ValueChanged += RotationSlider_ValueChanged;
        }
        private void LoadModel()
        {
            try
            {
                // 1. Создаём импортёр моделей
                var importer = new ModelImporter();

                // 2. Указываем путь к модели (3 варианта)

                // Вариант A: Из папки с исполняемым файлом
                string modelPath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "model/1.obj");

                // Вариант B: Абсолютный путь (для теста)
                // string modelPath = @"C:\Project\ClawBot\models\YourModel.obj";

                // Вариант C: Из ресурсов (если добавили в проект)
                // string modelPath = "pack://application:,,,/YourModel.obj";

                // 3. Загружаем модель
                var model = importer.Load(modelPath);

                // 4. Добавляем в сцену
                robotModel.Content = model;

                // 5. Автоматически подгоняем камеру
                viewport.ZoomExtents();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки модели:\n{ex.Message}",
                               "ClawBot",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }

        private void RotationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (robotModel.Content is Model3DGroup modelGroup)
            {
                var transform = new RotateTransform3D();
                transform.Rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), e.NewValue);
                modelGroup.Transform = transform;
            }
        }

    }
}
