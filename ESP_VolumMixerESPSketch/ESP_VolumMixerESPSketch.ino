#include <FastLED.h>
#define LED_PIN 13
#define LED_NUM 25
CRGB leds[LED_NUM];

void setup() {
  // put your setup code here, to run once:
  
  FastLED.addLeds<WS2812, LED_PIN, GRB>(leds, LED_NUM);
  FastLED.setBrightness(50);

  Serial.begin(9600);
  
  Serial2.begin(115200, SERIAL_8N1, 16, 17);
  
  Serial.println("ESP готов к приему данных от Arduino...");
}

byte counter;
void startRGB(){
  FastLED.clear();
  leds[counter] = CRGB::Red;
  if (++counter >= LED_NUM) counter = 0;
  FastLED.show();
  delay(300);
}

void loop() {

  //Serial.println("Ничего");
  if (Serial2.available()) {
    String message = Serial2.readStringUntil('\n');
    Serial.print("Получено от Arduino: ");
    Serial.println(message);
  }
  //startRGB();
}
