#include <WiFi.h>
#include <WiFiUdp.h>
#include <Adafruit_PWMServoDriver.h>

const char* ssid = "HaptiSense_WIFI";
const char* password = "AliNajm32";
IPAddress localIP(192, 168, 14, 1);  // Local IP address for the ESP32 router
IPAddress gateway(192, 168, 14, 1);  // Gateway IP address
IPAddress subnet(255, 255, 255, 0);  // Subnet mask
const int serverPort = 8888;         // UDP server port number

WiFiUDP udp;  // Create a UDP object

Adafruit_PWMServoDriver board01(0x40, Wire);
Adafruit_PWMServoDriver board02(0x41, Wire);
Adafruit_PWMServoDriver board03(0x42, Wire);

const int MAX_INPUT_LENGTH = 15;  // Maximum length of the input line, including newline character
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

  // Configure ESP32 as a router
  WiFi.mode(WIFI_AP);
  WiFi.softAPConfig(localIP, gateway, subnet);
  WiFi.softAP(ssid, password);

  Serial.println("ESP32 router mode activated");
  Serial.print("SSID: ");
  Serial.println(ssid);
  Serial.print("Password: ");
  Serial.println(password);
  Serial.print("Router IP address: ");
  Serial.println(localIP);

  // Start UDP server
  udp.begin(serverPort);
  Serial.println("UDP server started on port " + String(serverPort));

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

    // Check if any devices are connected
    if (WiFi.softAPgetStationNum() > 0) { ReadDataFromWIFI(); }

}

void ReadDataFromWIFI() {
  int packetSize = udp.parsePacket();  // Check for incoming UDP packets

  if (packetSize) {
    //Serial.println("Received UDP packet");

    // Read the data into a buffer
    char packetData[packetSize + 1];
    int bytesRead = udp.read(packetData, packetSize);
    packetData[bytesRead] = '\0';  // Add null termination to the buffer

    // Process the received data as needed
    String data = packetData;
    data.trim();  // Remove leading/trailing whitespaces

    // Ensure the received data has a length of 6 (actuator value) or 15 characters (Device setting)
    if (data.length() == 6) {
      SetActuatorsValues(data);
    } else if (data.length() == 15) {
      SetMicrocontrollerSetting(data);
    }else if (data.length() == 3) {
       } else if (data.startsWith("$$$")) {
  ResetActuatorValues();
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

  if (settingData.substring(0, 1) == "$") {

    Vibration_with_Interval = settingData.substring(1, 2).toInt();
    Relative_INTERVAL_ON = settingData.substring(2, 3).toInt();
    VIBRATION_DURATION_MS = settingData.substring(3, 6).toInt();
    VIBRATION_INTERVAL_MS = settingData.substring(6, 9).toInt();
    INTERVAL_VALUE_Percentage = settingData.substring(9, 11).toInt();
    Fixed_INTERVAL_VALUE = settingData.substring(11, 15).toInt();
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
