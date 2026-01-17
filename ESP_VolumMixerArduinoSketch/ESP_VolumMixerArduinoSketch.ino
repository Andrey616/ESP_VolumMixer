#include <EncButton.h>

const int Potenseometrs[] = {A0, A1, A2, A3, A7};
const int PotenseometrCount = sizeof(Potenseometrs) / sizeof(Potenseometrs[0]);

const int Encoders[] = {2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, A6};
const int EncoderCount = sizeof(Encoders) / sizeof(Encoders[0]);

int PotenseometrsValueOld[] = {0, 0, 0, 0, 0};
int PotenseometrsValueNev[] = {0, 0, 0, 0, 0};
int EncoderValue[] = {0, 0, 0, 0};



EncButton* encoderObjects[EncoderCount / 3]; 

void setup() {
  // put your setup code here, to run once:
  Serial.begin(115200);

  for (int PotenseometrPin = 0; PotenseometrPin < PotenseometrCount; PotenseometrPin++) {
    pinMode(Potenseometrs[PotenseometrPin], INPUT_PULLUP);
  }
  for (int EncoderPin = 0; EncoderPin < EncoderCount / 3; EncoderPin++) {
    encoderObjects[EncoderPin] = new EncButton(Encoders[EncoderPin*3], Encoders[EncoderPin*3 + 1], Encoders[EncoderPin*3 + 2]);
  }

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
  for (int EncoderObgectPosition = 0; EncoderObgectPosition < EncoderCount / 3; EncoderObgectPosition++) {
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

    // Ограничиваем значение (опционально)
    EncoderValue[EncoderObgectPosition] = constrain(EncoderValue[EncoderObgectPosition], 0, 100);
    
    RezulrReadValueEncoders += String(EncoderValue[EncoderObgectPosition]) + "| ";
  }
  return RezulrReadValueEncoders;  

}

void loop() {
  // put your main code here, to run repeatedly:
  //Serial.print(ReadPotenseometrs()); 
  //Serial.println(ReadEncoders());
  
  



}
