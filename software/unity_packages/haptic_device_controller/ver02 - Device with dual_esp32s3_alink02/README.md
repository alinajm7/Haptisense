# Haptic Device Integration for Unity

This guide explains how to integrate the "AliN_HapticDevice" package into your Unity project and provides a manual setup process for configuring the haptic device.

## How to Run the Haptic Device

1. **Download the Package:**  
   Download the "AliN_HapticDevice" package and add it to your Unity project.

2. **Sample Scene:**  
   A fully functioning sample scene is provided in the `Sample Scenes` folder to help you get started quickly.

## Manual Setup

To manually set up the haptic device in your Unity project, follow these steps:

### 1. Setting Up the Communication Manager

1. **Create an Empty GameObject:**  
   - In your scene, create an empty GameObject.
   - Add the `MicroControllerCommunicationManager` & `ActuatorDataSender` components to this GameObject.

Note about `MicroControllerCommunicationManager`:
   This component manages communication with the haptic device. The microcontroller expects a string that starts with `@` and ends with `#`. Use the following commands:
   - `@B#` to turn on the reading inputs process.
   - `@F#` to turn off the reading inputs process.

   This device has 58 actuators. The input should be an array of 580 numeric digits (58 sets of 10), where the first 5 digits indicate the low duration, and the second 5 digits indicate the high duration. Including the starting, ending characters, and a null terminator, the total transmission length is 583 characters.

  **Note:** If either the low duration or high duration is set to `0`, the microcontroller will consider it an error and keeps actuators turned off. To function correctly, low duration value can be any values but zero.
  
Note about `ActuatorDataSender`: 
     Required for collecting actuators array info from all the available actuatorsManagers in the scene and combine them together and send it to the DeviceControllManager.
     Also, this class is responsible to send data only if there is any changes in the actuators value.

### 2. Setting Up the Actuator Manager

1. **Create Another Empty GameObject:**  
   - Add an empty GameObject to manage the actuators.
   - This GameObject will serve as the parent for actuator objects.

2. **Add the Following Components:**

   - **`Collider` (with "Is Trigger" checked):**  
     Defines the scanning area for the actuators.

   - **`Rigidbody`:**  
     Enables object detection functions.

   - **`Actuators Manager`:**  
     Manages the behavior of actuators, including properties like scanning distance, power factor, and distance-based weighting.  
     
     **Properties:**
     - **Scanning Distance:** 
       Defines the range within which actuators can detect tangible objects.
       
     - **Power Factor:** 
       Adjusts the intensity of actuator responses based on proximity. Higher values increase sensitivity to closer objects.
       
     - **Minimum Actuator Value Threshold:**  
       The minimum value an actuator can output. Values below this threshold are considered inactive.
       
     - **Maximum Actuator Value:**  
       The maximum intensity an actuator can reach.
       
     - **Distance Base Weights:**
       Defines how the distance of multiple tangible objects affects actuator response by weight factors. The first value corresponds to the affecting weight of the closest object, the second value to the affecting weight of the next closest tangible object, and so on.
       
     - **Show Actuator Values:** 
       Enables or disables the display of actuator values in the scene. Default is enabled.
       
     - **Gradient:**  
       Defines the color gradient used for visual feedback based on actuator values.



   - **Optional Components:**
     - **`Actuator Layout`:**  
       Automatically creates a pattern of actuators based on specified parameters.
       
     - **`Actuator Mapper on Surface`:**  
       Maps all actuator cells onto any 3D object surface with a Collider (or Mesh Collider) using three different mapping methods.

### 3. Example Setup

Below is an example image showing a typical setup:

![Haptic Device Setup](./doc/images/haptic_device_setup.png)

## Additional Notes

- Ensure all GameObjects with actuators have the necessary components as listed above.
- Use the sample scene for reference if you encounter any issues.
