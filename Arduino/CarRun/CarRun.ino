//===============================================================
//  CarRun
//===============================================================
//#include <Servo.h> 

#include <SoftwareSerial.h>
#include <ArduinoJson.h>

//#define DEBUG

#ifdef DEBUG
#define SERIAL_PRINT(x) Serial.print((x))
#define SERIAL_PRINTLN(x) Serial.println((x))
#define JSON_PRINT_TO_SERIAL(x) (x).printTo(Serial)
#else
#define SERIAL_PRINT(x)
#define SERIAL_PRINTLN(x)
#define JSON_PRINT_TO_SERIAL(x)
#endif

#define bufferSize 64

char buffer[bufferSize];
int bufferPos = 0;
int braceCount = 0;

SoftwareSerial mySerial(6, 7); // RX, TX // TODO: need to change the pin.

int Left_motor_go=8;     //Left Moter forward(IN1)
int Left_motor_back=9;     //Left Moter backward (IN2)

int Right_motor_go=10;    // Right Moter forward(IN3)
int Right_motor_back=11;    // Left Moter backward(IN4)

int defaultSpeed = 150;

void setup()
{
#ifdef DEBUG
  Serial.begin(9600);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB port only
  }
#endif
  //initialize the pin as OUTPUT
  pinMode(Left_motor_go,OUTPUT); // PIN 8 (PWM)
  pinMode(Left_motor_back,OUTPUT); // PIN 9 (PWM)
  pinMode(Right_motor_go,OUTPUT);// PIN 10 (PWM) 
  pinMode(Right_motor_back,OUTPUT);// PIN 11 (PWM)
  //initialize the bluetooth
  mySerial.begin(9600);
  while (!mySerial.available()){
    SERIAL_PRINTLN("no data");
    delay(1000);
  }
}

void run(float time, int rightSpeed, int leftSpeed)     // forward
{
  int rGoDig = HIGH;
  int rBackDig = LOW;
  int rGoSpeed = rightSpeed;
  int rBackSpeed = 0;
  int lGoDig = HIGH;
  int lBackDig = LOW;
  int lGoSpeed = leftSpeed;
  int lBackSpeed = 0;
  if (rightSpeed>0)//wheel forward
  {
    SERIAL_PRINTLN("rightSpeed>0");
    rGoDig = HIGH;
    rBackDig = LOW;
    rGoSpeed = rightSpeed;
    rBackSpeed = 0;
  }
  else if(rightSpeed<0)//wheel backward
  {
    SERIAL_PRINTLN("rightSpeed<0");
    rGoDig = LOW;
    rBackDig = HIGH;
    rGoSpeed = 0;
    rBackSpeed = -rightSpeed;
  }
  else
  {
    SERIAL_PRINTLN("rightSpeed==0");
    rGoDig = LOW;
    rBackDig = LOW;
    rGoSpeed = 0;
    rBackSpeed = 0;
  }

  if (leftSpeed>0)
  {
    SERIAL_PRINTLN("leftSpeed>0");
    lGoDig = LOW;
    lBackDig = HIGH;
    lGoSpeed = 0;
    lBackSpeed = leftSpeed;
  }
  else if (leftSpeed<0)
  {
    SERIAL_PRINTLN("leftSpeed<0");
    lGoDig = HIGH;
    lBackDig = LOW;
    lGoSpeed = -leftSpeed;
    lBackSpeed = 0;
  }
  else
  {
    SERIAL_PRINTLN("leftSpeed==0");
    lGoDig = LOW;
    lBackDig = LOW;
    lGoSpeed = 0;
    lBackSpeed = 0;
  }

  digitalWrite(Right_motor_go,rGoDig);  // Right Moter go forward
  digitalWrite(Right_motor_back,rBackDig); 
  analogWrite(Right_motor_go,rGoSpeed);//PWM Spped 0~255, you may just change a little for right and left since the two moters are not exactly the same.
  analogWrite(Right_motor_back,rBackSpeed);
  digitalWrite(Left_motor_go,lGoDig);
  digitalWrite(Left_motor_back,lBackDig);
  analogWrite(Left_motor_go,lGoSpeed);
  analogWrite(Left_motor_back,lBackSpeed);
  delay(time * 1000);   //execution time
}
void run(float time)     // forward
{
  run(time, defaultSpeed, defaultSpeed);
}

void brake(float time)         //stop the car
{
  run(time, 0, 0);
}

void left(float time)         //turn left(left Moter is off, right moter go forward)
{
  run(time, defaultSpeed, 0);
}

void spin_left(float time)         //spin left(left wheel back，right weel forward)
{
  run(time, defaultSpeed, -defaultSpeed);
}

void right(float time)        //turn right(right moter is off，left wheel go forward)
{
  run(time, 0, defaultSpeed);
}

void spin_right(float time)
{
  run(time, -defaultSpeed, defaultSpeed);
}

void back(float time) 
{
  run(time,-defaultSpeed, -defaultSpeed);
}

void CommandExecutor(const JsonObject& root)
{
  DynamicJsonBuffer jsonBufferRes(bufferSize);
  SERIAL_PRINTLN("parsing success");
  String cmd = root["Command"];
  JsonArray& parameters = root["Parameters"];
  float time = parameters[0];
  if (cmd == "Go")
  {
    SERIAL_PRINTLN("cmd is Go");
    int rightSpeed = parameters[1];
    int leftSpeed = parameters[2];
    run(time, rightSpeed, leftSpeed);
  }
  else if(cmd == "Forward")
  {
    SERIAL_PRINTLN("cmd is Forward");
    run(time);
  }
  else if(cmd == "Back")
  {
    SERIAL_PRINTLN("cmd is Back");
    back(time);
  }
  else if(cmd == "Left")
  {
    SERIAL_PRINTLN("cmd is Left");
    left(time);
  }
  else if(cmd == "Right")
  {
    SERIAL_PRINTLN("cmd is Right");
    right(time);
  }
  else if(cmd == "SpinLeft")
  {
    SERIAL_PRINTLN("cmd is SpinLeft");;
    spin_left(time);
  }
  else if(cmd == "SpinRight")
  {
    SERIAL_PRINTLN("cmd is SpinRight");
    spin_right(time);
  }
  else if(cmd == "Brake")
  {
    SERIAL_PRINTLN("cmd is Brake");
    brake(time);
  }
  else
  {
    brake(0);
  }

  brake(0);
  JsonObject& ret = jsonBufferRes.createObject();
  int id  = root["ID"];
  ret["ID"] = id;
  ret["Result"] = "OK";
  ret["Message"] = "I'm coming back!";
  ret.printTo(mySerial);
  JSON_PRINT_TO_SERIAL(ret);
  mySerial.println();
}

void loop()
{

  bool needParse = false;
  if (mySerial.available()) {
    char c = mySerial.read();
    if (braceCount == 0 && c=='{') //start of a json obj or nested json obj
    {
      braceCount++;
    }
    if (braceCount>0)
    {
      buffer[bufferPos] = c;
      bufferPos++;
      buffer[bufferPos]= '\0';
    }
    if (braceCount>0 && c == '}') // end of json or nested json
    {
      braceCount--;
      if (braceCount == 0)
      {
        needParse = true;
      }
    }

    if (needParse)
    {
      DynamicJsonBuffer jsonBuffer(bufferSize);
      DynamicJsonBuffer jsonBufferRes(bufferSize);
      JsonObject& root = jsonBuffer.parseObject(buffer);
      if (root.success())
      {
        CommandExecutor(root);
      }
      else
      {
        SERIAL_PRINTLN("parsing failed");
        JsonObject& ret = jsonBufferRes.createObject();
        ret["ID"] = -1;
        ret["Result"] = "Fail";
        ret["Message"] = "cannot parse the json!";
        ret.printTo(mySerial);
        JSON_PRINT_TO_SERIAL(ret);
        mySerial.println();
      }
      bufferPos = 0;
      needParse = false;
    }
    SERIAL_PRINT(c);
  }
}
