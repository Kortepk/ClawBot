#include <Servo.h>

Servo servo[5];  // Массив из 5 сервоприводов

void setup() {
  Serial.begin(115200);
  Serial.println("Connect 115200");
  servo[0].attach(3);  // Сервопривод 1 на D3
  servo[1].attach(5);  // Сервопривод 2 на D5
  servo[2].attach(6);  // Сервопривод 3 на D6
  servo[3].attach(9);  // Сервопривод 4 на D9
  servo[4].attach(10);  // Сервопривод 5 на D10

  // Установка всех сервоприводов в 0° при запуске
  for (int i = 0; i < 5; i++) 
    servo[i].write(0);

}

void loop() {
  if (Serial.available()) {
    String command = Serial.readStringUntil('\n');  // Читаем строку до \n
    int servo_id = command.substring(0, command.indexOf(':')).toInt();
    int angle = command.substring(command.indexOf(':') + 1).toInt();

    if (servo_id >= 1 && servo_id <= 5 && angle >= 0 && angle <= 180) {
      servo[servo_id - 1].write(angle);  // Управляем сервоприводом
      Serial.print("Servo ");
      Serial.print(servo_id);
      Serial.print(" set to ");
      Serial.println(angle);
    } else {
      Serial.print("Error: Invalid command[");
      Serial.print(command);
      Serial.println("]");
    }
  }
}