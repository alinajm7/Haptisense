#include <Adafruit_PWMServoDriver.h>

const int BAUD_RATE = 921600;
Adafruit_PWMServoDriver board01(0x40, Wire);
Adafruit_PWMServoDriver board02(0x41, Wire);
Adafruit_PWMServoDriver board03(0x42, Wire);

bool Vibration_with_Interval = false;
int VIBRATION_DURATION_MS = 300;
int VIBRATION_INTERVAL_MS = 300;
int INTERVAL_VALUE_Percentage = 50;
bool Relative_INTERVAL_ON = false;
int Fixed_INTERVAL_VALUE = 0;

bool BEATING_ON = true;
unsigned long lastVibrationTime = 0;

void setup() {
  Serial.begin(BAUD_RATE);
  delay(1000);

  board01.begin();
  board02.begin();
  board03.begin();

  board01.setOscillatorFrequency(27000000);
  board01.setPWMFreq(1500);
  board02.setOscillatorFrequency(27000000);
  board02.setPWMFreq(1500);
  board03.setOscillatorFrequency(27000000);
  board03.setPWMFreq(1500);
  ResetActuatorValues();
}

void loop() {
  if (Vibration_with_Interval) {
    beating();
  } else {
    BEATING_ON = true;
  }

 if (Serial.available() >= 206) {
    ReadDataFor48Actuators();
  }
}

void SetAllActuatorValues(int boardIndex, int values[]) {
  Adafruit_PWMServoDriver* board;
  switch(boardIndex) {
    case 0: board = &board01; break;
    case 1: board = &board02; break;
    case 2: board = &board03; break;
  }

  int effectiveValue = 0;
  for (int i = 0; i < 16; i++) {
    if (BEATING_ON) {
      effectiveValue = values[i];
    } else {
      effectiveValue = values[i] ? (Relative_INTERVAL_ON ? (values[i] * INTERVAL_VALUE_Percentage / 100) : Fixed_INTERVAL_VALUE) : 0;
    }
    board->setPWM(i, 0, effectiveValue);
  }
}



void ReadDataFor48Actuators() {
  char inData[207];
  Serial.readBytes(inData, 206);
  inData[206] = '\0';

  // Parse the first 14 characters for control settings
  Vibration_with_Interval = inData[0] - '0';
  Relative_INTERVAL_ON = inData[1] - '0';

  // Directly calculate 3-character Active and Interval durations
  VIBRATION_DURATION_MS = (inData[2] - '0') * 100 + (inData[3] - '0') * 10 + (inData[4] - '0');
  VIBRATION_INTERVAL_MS = (inData[5] - '0') * 100 + (inData[6] - '0') * 10 + (inData[7] - '0');

  // Directly calculate 2-character and 4-character Values during intervals
  INTERVAL_VALUE_Percentage = (inData[8] - '0') * 10 + (inData[9] - '0');
  Fixed_INTERVAL_VALUE = (inData[10] - '0') * 1000 + (inData[11] - '0') * 100 + (inData[12] - '0') * 10 + (inData[13] - '0');

  int values01[16], values02[16], values03[16];

  for (int i = 0; i < 48; i++) {
    // Directly calculate the 4-character actuator value
    int actuatorValue = (inData[14 + i * 4] - '0') * 1000 + (inData[15 + i * 4] - '0') * 100 + (inData[16 + i * 4] - '0') * 10 + (inData[17 + i * 4] - '0');

    if (i < 16) {
      values01[i] = actuatorValue;
    } else if (i >= 16 && i < 32) {
      values02[i - 16] = actuatorValue;
    } else {
      values03[i - 32] = actuatorValue;
    }
  }

  SetAllActuatorValues(0, values01);
  SetAllActuatorValues(1, values02);
  SetAllActuatorValues(2, values03);
}




void SetActuatorValue(Adafruit_PWMServoDriver &board, int index, int value) {
  if (BEATING_ON) {
    board.setPWM(index, 0, value);
  } else {
    if (value == 0) {
      board.setPWM(index, 0, 0);
    } else {
      if (Relative_INTERVAL_ON) {
        board.setPWM(index, 0, value * INTERVAL_VALUE_Percentage / 100);
      } else {
        board.setPWM(index, 0, Fixed_INTERVAL_VALUE);
      }
    }
  }
}

void ResetActuatorValues() {
  for (int i = 0; i < 16; i++) {
    board01.setPWM(i, 0, 0);
    board02.setPWM(i, 0, 0);
    board03.setPWM(i, 0, 0);
  }
}

void beating() {
  unsigned long currentTime = millis();
  unsigned long timeDiff = currentTime - lastVibrationTime;

  if (timeDiff >= VIBRATION_INTERVAL_MS) {
    BEATING_ON = true;
    if (timeDiff >= VIBRATION_INTERVAL_MS + VIBRATION_DURATION_MS) {
      BEATING_ON = false;
      lastVibrationTime = currentTime;
    }
  } else {
    BEATING_ON = false;
  }
}
