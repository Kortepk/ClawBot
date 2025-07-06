using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace ClawBot.Controls
{
    public class RobotArmController
    {
        public List<RobotPart> Parts { get; } = new List<RobotPart>();
        private readonly HelixViewport3D _viewport;

        public RobotArmController(HelixViewport3D viewport)
        {
            _viewport = viewport;
        }

        // axis - ось вокруг которой происходит вращение, connectionPoint - точка соединения с предыдущей деталью, по умолчанию своё вращение происходит вокруг н
        public RobotPart AddRootPart(string modelPath, Vector3D axis, Point3D connectionPoint)
        {
            var part = new RobotPart(modelPath, connectionPoint)
            {
                RotationAxis = axis
            };

            Parts.Add(part);
            _viewport.Children.Add(new ModelVisual3D { Content = part.Model });
            return part;
        }

        public RobotPart AddChildPart(RobotPart parent, string modelPath, Vector3D axis, Point3D connectionPoint)
        {
            var child = new RobotPart(modelPath, connectionPoint)
            {
                RotationAxis = axis
            };
            // Тут добавить инициализацию начального угла поворота детали
            parent.AddChild(child);

            child.GlobalShiftPoint = new Point3D(
                parent.GlobalShiftPoint.X + connectionPoint.X,
                parent.GlobalShiftPoint.Y + connectionPoint.Y,
                parent.GlobalShiftPoint.Z + connectionPoint.Z
            );

            child.UpdateTransform(0);

            _viewport.Children.Add(new ModelVisual3D { Content = child.Model });
            return child;
        }

        public void UpdatePart(RobotPart part, double angle)
        {
            part.UpdateTransform(angle);
        }
    }
}
