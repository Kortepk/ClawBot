using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Text;
using System.Threading;


public class ArduinoPort : IDisposable
{
    private SerialPort _serialPort;
    private bool _isConnected = false;

    public bool Connect(string portName, int baudRate = 115200)
    {
        try
        {
            _serialPort = new SerialPort(portName, baudRate)
            {
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReadTimeout = 500,
                WriteTimeout = 500
            };

            _serialPort.Open();
            _isConnected = true;

            _serialPort.DataReceived += (sender, e) =>
            {
                if (_serialPort.BytesToRead > 0)
                {
                    string receivedData = _serialPort.ReadExisting(); 
                    Console.WriteLine($"Получено: {receivedData}");
                }
            };

            // Даем Arduino время на инициализацию
            Thread.Sleep(2000);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка подключения: {ex.Message}");
            return false;
        }
    }
    public bool Disconnect()
    {
        _serialPort.Close();
        _isConnected = false;
        return true;
    }


    public void SendServoCommand(int servoId, int angle)
    {
        if (!_isConnected || _serialPort == null || !_serialPort.IsOpen)
        {
            Console.WriteLine("Порт не подключен!");
            return;
        }

        try
        {
            string command = $"{servoId}:{angle}\n";
            byte[] asciiBytes = Encoding.ASCII.GetBytes(command);
            _serialPort.Write(asciiBytes, 0, asciiBytes.Length);
            Console.WriteLine($"Отправлено: {command.Trim()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка отправки: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _isConnected = false;
        _serialPort?.Close();
        _serialPort?.Dispose();
    }
}