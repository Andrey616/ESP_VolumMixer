#include <EncButton.h>
#include <SoftwareSerial.h>
#include <EEPROM.h>
SoftwareSerial mySerial(2, 3);
uint32_t TimerAutoPush;
uint32_t TimerDeleyPush;

#define BateryPin A4

const int Potenseometrs[] = {A0, A1, A2, A3, A7};
const int PotenseometrCount = sizeof(Potenseometrs) / sizeof(Potenseometrs[0]);

const int Encoders[] = {2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, A6};
const int EncoderCount = sizeof(Encoders) / sizeof(Encoders[0]);

int PotenseometrsValueOld[] = {0, 0, 0, 0, 0};
int PotenseometrsValueNev[] = {0, 0, 0, 0, 0};
int EncoderValue[] = {115, 0, 0, 0};

int Mode = 100;
int PodMode = 10;
int Bright = 5;

EncButton* encoderObjects[EncoderCount / 3]; 

void setup() {
  Serial.begin(115200);

  pinMode(BateryPin, INPUT);

  TimerAutoPush = millis();
  TimerDeleyPush = millis();

  for (int PotenseometrPin = 0; PotenseometrPin < PotenseometrCount; PotenseometrPin++) {
    pinMode(Potenseometrs[PotenseometrPin], INPUT_PULLUP);
  }
  for (int EncoderPin = 0; EncoderPin < EncoderCount / 3; EncoderPin++) {
    encoderObjects[EncoderPin] = new EncButton(Encoders[EncoderPin*3], Encoders[EncoderPin*3 + 1], Encoders[EncoderPin*3 + 2]);
  }
  readInEprom();
}

String ReadPotenseometrs(){
  String RezulrReadValuePotenseometrs = "";
  for (int PotenseometrPin = 0; PotenseometrPin < PotenseometrCount - 1; PotenseometrPin++) {
    PotenseometrsValueNev[PotenseometrPin] = map(analogRead(Potenseometrs[PotenseometrPin]), 16, 1023, 0, 100);
    if (PotenseometrsValueNev[PotenseometrPin] != PotenseometrsValueOld[PotenseometrPin]){
      PotenseometrsValueOld[PotenseometrPin] = PotenseometrsValueNev[PotenseometrPin];
    }
    RezulrReadValuePotenseometrs += String(PotenseometrsValueOld[PotenseometrPin]) + "| ";
  }
  PotenseometrsValueNev[PotenseometrCount-1] = map(analogRead(Potenseometrs[PotenseometrCount-1]), 0, 1023, 0, 100);
  if (PotenseometrsValueNev[PotenseometrCount-1] != PotenseometrsValueOld[PotenseometrCount-1]){
    PotenseometrsValueOld[PotenseometrCount-1] = PotenseometrsValueNev[PotenseometrCount-1];
  }
  RezulrReadValuePotenseometrs += String(PotenseometrsValueOld[PotenseometrCount-1]) + "| ";
  return RezulrReadValuePotenseometrs;
}

String ReadEncoders(){
  String RezulrReadValueEncoders = "";
  for (int EncoderObgectPosition = 1; EncoderObgectPosition < (EncoderCount) / 3; EncoderObgectPosition++) {
    encoderObjects[EncoderObgectPosition]->tick();

    if (encoderObjects[EncoderObgectPosition]->right()) {
      EncoderValue[EncoderObgectPosition] += 1;
    }

    if (encoderObjects[EncoderObgectPosition]->left()) {
      EncoderValue[EncoderObgectPosition] -= 1;
    }

    if (encoderObjects[EncoderObgectPosition]->rightH()) {
      EncoderValue[EncoderObgectPosition] += 5;
    }

    if (encoderObjects[EncoderObgectPosition]->leftH()) {
      EncoderValue[EncoderObgectPosition] -= 5;
    }

    EncoderValue[EncoderObgectPosition] = constrain(EncoderValue[EncoderObgectPosition], 0, 100);
    RezulrReadValueEncoders += String(EncoderValue[EncoderObgectPosition]) + "| ";
  }
  return RezulrReadValueEncoders;  
}

String ReadEncoderMode(){
  String RezulrReadEncoderMode = "";
  encoderObjects[0]->tick();

  if (encoderObjects[0]->right()) {
    Mode += 100;
    if (Mode > 401) {Mode = 100;}
  }
  if (encoderObjects[0]->left()) {
     Mode -= 100;
    if (Mode < 100) {Mode = 400;}
  }

  if (encoderObjects[0]->click()) {
    PodMode += 10;
    if (PodMode > 91){PodMode = 0;}
  }

  if (encoderObjects[0]->rightH()) {
    Bright += 1;
    if (Bright==10){Bright = 0;}
  }
  if (encoderObjects[0]->leftH()) {
    Bright -= 1;
    if (Bright<0){Bright = 9;}
  }

  EncoderValue[0] = Mode + PodMode + Bright;
  RezulrReadEncoderMode += String(EncoderValue[0]) + "| ";
  return RezulrReadEncoderMode;
}

float filteredValue = 0;
float alpha = 0.2; 
String ReadBatteryVolum(){
  int rawValue = analogRead(BateryPin);
  filteredValue = alpha * rawValue + (1 - alpha) * filteredValue;
  float voltage = filteredValue * (5.0 / 1023.0) * 2;
  int percentage = map(voltage * 100, 3.3 * 100, 4.2 * 100, 0, 100);
  return String(constrain(percentage, 0, 100));
}

void SaveInEprom(){
  for (int IdValue = 0; IdValue < 4; IdValue++){
    EEPROM.update(IdValue, EncoderValue[IdValue]);
  }
  EEPROM.put(5, Mode);
  EEPROM.put(7, PodMode);
  EEPROM.put(9, Bright);
}

void readInEprom(){
  for (int IdValue = 0; IdValue < 4; IdValue++){
    EncoderValue[IdValue] = EEPROM.read(IdValue);
  }
  Mode = EEPROM.get(5, Mode);
  PodMode = EEPROM.get(7, PodMode);
  Bright = EEPROM.get(9, Bright);
}

String OldRead = " ";
String NevRead;
void loop() {
  NevRead = ReadPotenseometrs() + ReadEncoders() + ReadEncoderMode();
  if (millis() - TimerAutoPush >= 500 || (NevRead != OldRead && millis() - TimerDeleyPush >= 100)){
    OldRead = NevRead;
    NevRead += ReadBatteryVolum();
    Serial.println(NevRead);
    TimerDeleyPush = millis();
    TimerAutoPush = millis();
    SaveInEprom();
  }
}
