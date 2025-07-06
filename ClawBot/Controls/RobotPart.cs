using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace ClawBot.Controls
{
    public class RobotPart
    {
        // Свойства детали
        public Model3D Model { get; set; }
        public Vector3D RotationAxis { get; set; } = new Vector3D(0, 1, 0); // Ось по умолчанию (Y)
        public double CurrentAngle { get; private set; } // Текущий угол
        public double initAngle { get; set; }
        public Point3D LocalConnectionPoint { get; set; } // Точка соединения предыдущий детали в локальных координатах
        public Point3D GlobalShiftPoint { get; set; } // Глобальная точка сдвига, меняется при изменения угла родителя
        public Matrix3D LocalTransform { get; set; }
        public RobotPart Parent { get; set; }
        public List<RobotPart> Children { get; } = new List<RobotPart>();
        public Transform3DGroup Transform { get; } = new Transform3DGroup();

        public RobotPart(string modelPath, Point3D connectionPoint, Color color)
        {
            var importer = new ModelImporter();
            Model = importer.Load(modelPath);

            // Создаем новый материал
            var newMaterial = new DiffuseMaterial(new SolidColorBrush(color));

            // Перебираем все части модели
            if (Model is Model3DGroup group)
            {
                RecolorModel(group, newMaterial);
            }
            else if (Model is GeometryModel3D geometryModel)
            {
                geometryModel.Material = newMaterial;
                geometryModel.BackMaterial = newMaterial;
            }

            Model.Transform = Transform;
            LocalConnectionPoint = connectionPoint;
            GlobalShiftPoint = LocalConnectionPoint;
        }
        private void RecolorModel(Model3DGroup modelGroup, Material newMaterial)
        {
            foreach (var childModel in modelGroup.Children)
            {
                if (childModel is Model3DGroup nestedGroup)
                {
                    RecolorModel(nestedGroup, newMaterial);
                }
                else if (childModel is GeometryModel3D geometryModel)
                {
                    geometryModel.Material = newMaterial;
                    geometryModel.BackMaterial = newMaterial;
                }
            }
        }

        public void AddChild(RobotPart child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        public void UpdateTransform(double angle)
        {
            // Обновляем текущий угол
            CurrentAngle = angle + initAngle;

            var tempMatrix = new Transform3DGroup();

            tempMatrix.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(RotationAxis, CurrentAngle)));
            tempMatrix.Children.Add(new TranslateTransform3D(LocalConnectionPoint.X, LocalConnectionPoint.Y, LocalConnectionPoint.Z));

            Transform.Children.Clear();
            Transform.Children.Add(tempMatrix); // Добавляем локальную
            Transform.Children.Add(Parent.Transform); // Добавляем глобальную
            
            // Обновляем всех детей
            foreach (var child in Children)
            {
                // Применяем компенсацию к детям
                child.CompensateParentTransform(Transform);
            }

        }

        public void CompensateParentTransform(Transform3DGroup parentTransform)
        {
            var tempMatrix = new Transform3DGroup();

            tempMatrix.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(RotationAxis, CurrentAngle)));
            tempMatrix.Children.Add(new TranslateTransform3D(LocalConnectionPoint.X, LocalConnectionPoint.Y, LocalConnectionPoint.Z));

            Transform.Children.Clear();
            Transform.Children.Add(tempMatrix); // Добавляем локальную
            Transform.Children.Add(Parent.Transform); // Добавляем глобальную

            // Делаем компенсацию всем детям
            foreach (var child in Children)
            {
                child.CompensateParentTransform(Transform);
            }
        }
    }
}
