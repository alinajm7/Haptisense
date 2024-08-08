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
   - Add the `MicroControllerCommunicationManager` component to this GameObject.  
   This component manages communication with the haptic device.

### 2. Setting Up the Actuator Manager

1. **Create Another Empty GameObject:**  
   - Add an empty GameObject to manage the actuators.
   - This GameObject will serve as the parent for actuator objects.

2. **Add the Following Components:**

   - **Collider (with "Is Trigger" checked):**  
     Define the scanning area for the actuators.

   - **Rigidbody:**  
     Enables object detection functions.

   - **Actuators Manager:**  
     Manages the behavior of actuators, such as scanning distance, power factor, min & max threshold values, and the order of actuators in the array.

   - **Actuator Data Sender:**  
     Required for the Actuators Manager to send calculated data to the Device Manager.

   - **Optional Components:**
     - **Actuator Layout:**  
       Automatically creates a pattern of actuators based on specified parameters.
       
     - **Actuator Mapper on Surface:**  
       Maps all actuator cells onto any 3D object surface with a Collider (or Mesh Collider) using three different mapping methods.

### 3. Example Setup

Below is an example image showing a typical setup:

![Haptic Device Setup](./doc/images/haptic_device_setup.png)

## Additional Notes

- Ensure all GameObjects with actuators have the necessary components as listed above.
- Use the sample scene for reference if you encounter any issues.

