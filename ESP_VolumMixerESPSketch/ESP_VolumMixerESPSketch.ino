#include <FastLED.h>
#include "BluetoothSerial.h"
#define LED_PIN 4
#define LED_NUM 25
CRGB leds[LED_NUM];
BluetoothSerial ESP_BT;

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  Serial2.begin(115200, SERIAL_8N1, 16, 17);
  Serial.println("ESP готов к приему данных от Arduino...");

  FastLED.addLeds<WS2812, LED_PIN, GRB>(leds, LED_NUM);
  xTaskCreatePinnedToCore(StartTaskRGBMode, "TaskRGBMode", 16384, NULL, 1, NULL, 0);

  ESP_BT.begin("VolumMixer", "1212");
  Serial.println("Bluetooth готов к сопряжению");
}

int mode = 1;
int PodMode = 1;
int Bright = 50;
void StartTaskRGBMode(void *pvParameters) {
  while(1) {
    FastLED.setBrightness(Bright);
    switch(mode) {
      case 1:
        startRGB();
        break;
      case 2:
        GradientMode();
        break;
      case 3:
        FullCollor();
        break;
      case 4:
        VolumCollor();
        break;
      case 10:
        LowBattery();
        break;
    }
    delay(50);
  }
}

byte counter;
void startRGB(){
  FastLED.clear();
  leds[counter] = CRGB::Red;
  if (++counter >= LED_NUM) counter = 0;
  FastLED.show();
  delay(300);
}

void LowBattery(){
  FastLED.clear();
  leds[0] = CRGB::Red;
  FastLED.show();
  delay(700);
  leds[0] = CRGB::Black;
  FastLED.show();
  delay(700);
}

void GradientMode(){
  int R = 0; int G = 252; int B = 0;
  FastLED.clear();
  if (PodMode > 1){ PodMode = 1;}
  switch(PodMode) {
    case 0:
      R = 0; G = 252; B = 0;
      for (int IdLeds = 0; IdLeds < 5; IdLeds++){
        for (int modeIdLeds = 0; modeIdLeds < 21; modeIdLeds+=5){
          leds[IdLeds+modeIdLeds].setRGB(R,G,B);
        }
        R += 63; G -= 63;
      }
      break;
    case 1:
      R = 252; G = 0; B = 0;
      for (int IdLeds = 0; IdLeds < 5; IdLeds++){
        for (int modeIdLeds = 0; modeIdLeds < 21; modeIdLeds+=5){
          leds[IdLeds+modeIdLeds].setRGB(R,G,B);
        }
        G += 63; R -= 63;
      }
      break;
  }
  FastLED.show();
  delay(50);
}

void FullCollor(){
  FastLED.clear();
  switch(PodMode) {
    case 0:
      fill_solid(leds, LED_NUM, CRGB(255,0,0));
      break;
    case 1:
      fill_solid(leds, LED_NUM, CRGB(255, 153, 0));
      break;
    case 2:
      fill_solid(leds, LED_NUM, CRGB(255, 255, 0));
      break;
    case 3:
      fill_solid(leds, LED_NUM, CRGB(153, 255, 0));
      break;
    case 4:
      fill_solid(leds, LED_NUM, CRGB(0, 255, 0));
      break;
    case 5:
      fill_solid(leds, LED_NUM, CRGB(0, 255, 153));
      break;
    case 6:
      fill_solid(leds, LED_NUM, CRGB(0, 255, 255));
      break;
    case 7:
      fill_solid(leds, LED_NUM, CRGB(0, 153, 255));
      break;
    case 8:
      fill_solid(leds, LED_NUM, CRGB(0, 0, 255));
      break;
    case 9:
      fill_solid(leds, LED_NUM, CRGB(153, 0, 255));
      break;
  }
  FastLED.show();
  delay(50);
}

int numbers[10];
void VolumCollor(){
  FastLED.clear();
  if (PodMode > 5){ PodMode = 5;}
  switch(PodMode) {
    case 0:
      for (int IdValue = 0; IdValue < 5; IdValue++){
        for (int IdLeds = 0; IdLeds < 5; IdLeds++){
          leds[IdValue*5+IdLeds].setRGB(250-(250*numbers[IdValue])/100.0,(250*numbers[IdValue])/100.0,0);
        }
      }
      break;
    case 1:
      for (int IdValue = 0; IdValue < 5; IdValue++){
        for (int IdLeds = 0; IdLeds < 5; IdLeds++){
          leds[IdValue*5+IdLeds].setRGB((250*numbers[IdValue])/100.0,250-(250*numbers[IdValue])/100.0,0);
        }
      }
      break;
    case 2:
      for (int IdValue = 0; IdValue < 5; IdValue++){
        for (int IdLeds = 0; IdLeds < 5; IdLeds++){
          leds[IdValue*5+IdLeds].setRGB(250-(250*numbers[IdValue])/100.0,0,(250*numbers[IdValue])/100.0);
        }
      }
      break;
    case 3:
      for (int IdValue = 0; IdValue < 5; IdValue++){
        for (int IdLeds = 0; IdLeds < 5; IdLeds++){
          leds[IdValue*5+IdLeds].setRGB((250*numbers[IdValue])/100.0,0,250-(250*numbers[IdValue])/100.0);
        }
      }
      break;
    case 4:
      for (int IdValue = 0; IdValue < 5; IdValue++){
        for (int IdLeds = 0; IdLeds < 5; IdLeds++){
          leds[IdValue*5+IdLeds].setRGB(0,250-(250*numbers[IdValue])/100.0,(250*numbers[IdValue])/100.0);
        }
      }
      break;
    case 5:
      for (int IdValue = 0; IdValue < 5; IdValue++){
        for (int IdLeds = 0; IdLeds < 5; IdLeds++){
          leds[IdValue*5+IdLeds].setRGB(0,(250*numbers[IdValue])/100.0,250-(250*numbers[IdValue])/100.0);
        }
      }
      break;
    }
  
  FastLED.show();
  delay(50);
}

void ReadMessege(){
  if (Serial2.available()) {
    String message = Serial2.readStringUntil('\n');
    Serial.print("Получено от Arduino: ");
    Serial.println(message);
    if (ESP_BT.hasClient()) {
      ESP_BT.println("VolumMixer-" + message);
    }
    char buffer[50];
    message.toCharArray(buffer, sizeof(buffer));
    int index = 0;
    char* MessageNotSeparator = strtok(buffer, "| ");
    while (MessageNotSeparator != NULL) {
      numbers[index++] = atoi(MessageNotSeparator);
      MessageNotSeparator = strtok(NULL, "| ");
    }

    mode = numbers[8] / 100;
    PodMode = numbers[8] % 100 / 10;
    Bright = numbers[8] % 10 * 10;
    if (numbers[9] < 20){
      mode = 10;
    }
  }
}

void loop() {
  ReadMessege();
}
