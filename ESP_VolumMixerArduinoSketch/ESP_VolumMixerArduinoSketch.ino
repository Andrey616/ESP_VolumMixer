const int Potenseometrs[] = {A0, A1, A2, A3, A7};
const int PotenseometrCount = sizeof(Potenseometrs) / sizeof(Potenseometrs[0]);

int PotenseometrsValueOld[] = {0, 0, 0, 0, 0};
int PotenseometrsValueNev[] = {0, 0, 0, 0, 0};

void setup() {
  // put your setup code here, to run once:
  Serial.begin(115200);

  for (int PotenseometrPin = 0; PotenseometrPin < PotenseometrCount; PotenseometrPin++) {
    pinMode(Potenseometrs[PotenseometrPin], INPUT_PULLUP);
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
    return (RezulrReadValuePotenseometrs);
}

void loop() {
  // put your main code here, to run repeatedly:
  //Serial.println(ReadPotenseometrs()); 
  



}
