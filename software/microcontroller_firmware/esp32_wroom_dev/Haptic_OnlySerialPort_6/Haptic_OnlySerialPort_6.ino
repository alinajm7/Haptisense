
#include <Adafruit_PWMServoDriver.h>


const int BAUD_RATE = 921600;  // 921600 baud rate

Adafruit_PWMServoDriver board01(0x40, Wire);
Adafruit_PWMServoDriver board02(0x41, Wire);
Adafruit_PWMServoDriver board03(0x42, Wire);


const int MAX_INPUT_LENGTH = 16;  // Maximum length of the input line, including newline character
char inString[MAX_INPUT_LENGTH];  // Input line buffer
int inStringIndex = 0;
bool Vibration_with_Interval = false;  // Selecting if we want interval or no
int VIBRATION_DURATION_MS = 300;       // Duration of each vibration
int VIBRATION_INTERVAL_MS = 300;       // Interval between vibrations
int INTERVAL_VALUE_Percentage = 50;    // Value for vibration during the interval _ percentage from current vibration value
bool Relative_INTERVAL_ON = false;     // Selecting to use a fixed or relative interval value
int Fixed_INTERVAL_VALUE = 0;          // Value for vibration during the interval

bool BEATING_ON = true;               // this is internal value to indicate when has to activate and deactivate
unsigned long lastVibrationTime = 0;  // Time of the last vibration


void setup() {

Serial.begin(BAUD_RATE);
  delay(1000);  // Allow some time for serial port to initialize


  // Initialize the Adafruit_PWMServoDriver instances
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

    // Check if SerialPort has data
  if (Serial.available() > 0) {
    ReadDataFromSerialPort();
  } 

}

void ReadDataFromSerialPort()
{
    if (Serial.available() > 0)
    {
        char inChar = Serial.read();

        if (inChar == '\n')
        {
            inString[inStringIndex] = '\0';  // Null terminate the string

            if (inStringIndex == 6)
            {
                SetActuatorsValues(inString);
            }
            
            inStringIndex = 0;
        }
        else if (inChar == '$')
        {
            String configData = Serial.readStringUntil('\n');
            if (configData.length()==3) {ResetActuatorValues();} else
            {SetMicrocontrollerSetting(configData);}
            
        }
        else if (isdigit(inChar) && inStringIndex < (MAX_INPUT_LENGTH))
        {
            inString[inStringIndex++] = inChar;
        }
    }
}



void SetActuatorsValues(String Rdata) {

  int ActuatorIndex = Rdata.substring(0, 2).toInt();
  int ActuatorValue = Rdata.substring(2, 6).toInt();


  if (ActuatorIndex >= 0 && ActuatorIndex < 16) {
    if (BEATING_ON) {
      board01.setPWM(ActuatorIndex, 0, ActuatorValue);
    } else {
      if (ActuatorValue == 0) {
        board01.setPWM(ActuatorIndex, 0, 0);
      } else {
        if (Relative_INTERVAL_ON) {
          board01.setPWM(ActuatorIndex, 0, ActuatorValue * INTERVAL_VALUE_Percentage / 100);
        } else {
          board01.setPWM(ActuatorIndex, 0, Fixed_INTERVAL_VALUE);
        }
      }
    }

  } else if (ActuatorIndex >= 16 && ActuatorIndex < 32) {

    if (BEATING_ON) {
      board02.setPWM(ActuatorIndex - 16, 0, ActuatorValue);
    } else {
      if (ActuatorValue == 0) {
        board02.setPWM(ActuatorIndex - 16, 0, 0);
      } else {
        if (Relative_INTERVAL_ON) {
          board02.setPWM(ActuatorIndex - 16, 0, ActuatorValue * INTERVAL_VALUE_Percentage / 100);
        } else {
          board02.setPWM(ActuatorIndex - 16, 0, Fixed_INTERVAL_VALUE);
        }
      }
    }
  } else if (ActuatorIndex >= 32 && ActuatorIndex < 48) {

    if (BEATING_ON) {
      board03.setPWM(ActuatorIndex - 32, 0, ActuatorValue);
    } else {
      if (ActuatorValue == 0) {
        board03.setPWM(ActuatorIndex - 32, 0, 0);
      } else {
        if (Relative_INTERVAL_ON) {
          board03.setPWM(ActuatorIndex - 32, 0, ActuatorValue * INTERVAL_VALUE_Percentage / 100);
        } else {
          board03.setPWM(ActuatorIndex - 32, 0, Fixed_INTERVAL_VALUE);
        }
      }
    }
  }
}

void SetMicrocontrollerSetting(String settingData) {

  //  if (settingData.substring(0, 1) == "S") {

    // Vibration_with_Interval = settingData.substring(1, 2).toInt();
    // Relative_INTERVAL_ON = settingData.substring(2, 3).toInt();
    // VIBRATION_DURATION_MS = settingData.substring(3, 6).toInt();
    // VIBRATION_INTERVAL_MS = settingData.substring(6, 9).toInt();
    // INTERVAL_VALUE_Percentage = settingData.substring(9, 11).toInt();
    // Fixed_INTERVAL_VALUE = settingData.substring(11, 15).toInt();

    Vibration_with_Interval = settingData.substring(0, 1).toInt();
    Relative_INTERVAL_ON = settingData.substring(1, 2).toInt();
    VIBRATION_DURATION_MS = settingData.substring(2, 5).toInt();
    VIBRATION_INTERVAL_MS = settingData.substring(5, 8).toInt();
    INTERVAL_VALUE_Percentage = settingData.substring(8, 10).toInt();
    Fixed_INTERVAL_VALUE = settingData.substring(10, 14).toInt();
  //  }
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
