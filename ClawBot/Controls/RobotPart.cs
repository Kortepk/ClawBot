using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace ClawBot.Controls
{
    public class RobotPart
    {
        // Свойства детали
        public Model3D Model { get; set; }
        public Vector3D RotationAxis { get; set; } = new Vector3D(0, 1, 0); // Ось по умолчанию (Y)
        public Point3D RotationCenter { get; set; } = new Point3D(0, 0, 0); // Точка вращения
        public double CurrentAngle { get; private set; } // Текущий угол
        public Point3D LocalConnectionPoint { get; set; } // Точка соединения предыдущий детали в локальных координатах
        public Point3D GlobalShiftPoint { get; set; } // Глобальная точка сдвига, меняется при изменения угла родителя
        public Matrix3D LocalTransform { get; set; }
        public Matrix3D GlobalTransform { get; set; }
        public RobotPart Parent { get; set; }
        public List<RobotPart> Children { get; } = new List<RobotPart>();
        public Transform3DGroup Transform { get; } = new Transform3DGroup();

        public RobotPart(string modelPath, Point3D connectionPoint)
        {
            var importer = new ModelImporter();
            Model = importer.Load(modelPath);
            Model.Transform = Transform;
            LocalConnectionPoint = connectionPoint;
            GlobalShiftPoint = LocalConnectionPoint;
        }

        public void AddChild(RobotPart child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        public void UpdateTransform(double angle)
        {
            // Обновляем текущий угол
            CurrentAngle = angle;

            // Применяем трансформации
            Transform.Children.Clear();
            Transform.Children.Insert(0, new MatrixTransform3D(GlobalTransform));

            var rotation = new RotateTransform3D(
            new AxisAngleRotation3D(RotationAxis, CurrentAngle));
            LocalTransform = rotation.Value;
            Transform.Children.Add(new MatrixTransform3D(LocalTransform));

            Transform.Children.Add(new TranslateTransform3D(
            new Vector3D(
                GlobalShiftPoint.X,
                GlobalShiftPoint.Y,
                GlobalShiftPoint.Z)));

            // Получаем новую мировую матрицу
            Matrix3D newWorldTransform = GetWorldTransform();

            // Обновляем всех детей
            foreach (var child in Children)
            {
                // Применяем компенсацию к детям
                child.CompensateParentTransform(GlobalShiftPoint, newWorldTransform);
            }
        }

        public void CompensateParentTransform(Point3D shiftPoint, Matrix3D compensationTransform)
        {
            GlobalTransform = compensationTransform;
            Transform.Children.Clear();

            Point3D positionAfter = compensationTransform.Transform(LocalConnectionPoint);

            GlobalShiftPoint = new Point3D(
                shiftPoint.X + positionAfter.X,
                shiftPoint.Y + positionAfter.Y,
                shiftPoint.Z + positionAfter.Z
            );

            Transform.Children.Insert(0, new MatrixTransform3D(GlobalTransform));

            Transform.Children.Add(new MatrixTransform3D(LocalTransform));

            // Компенсируем родительскую трансформацию
            Transform.Children.Add(new TranslateTransform3D(
            new Vector3D(
                GlobalShiftPoint.X,
                GlobalShiftPoint.Y,
                GlobalShiftPoint.Z)));

            // Пропагируем компенсацию всем детям
            foreach (var child in Children)
            {
                child.CompensateParentTransform(GlobalShiftPoint, compensationTransform);
            }
        }

        public Matrix3D GetWorldTransform()
        {
            var matrix = Transform.Value;
            if (Parent != null)
            {
                matrix = Matrix3D.Multiply(matrix, Parent.GetWorldTransform());
            }
            return matrix;
        }
    }
}
