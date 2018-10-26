# scratchx
A workable solution to use ScratchX extension control an Arduino car with Bluetooh

# Introduction
- Arduino/CarRun: The ino file that controls the logic for read/write through Bluetooth as a Serial.
- BluetoothWSServer: The Bluetooth connection Server in the middle. Using Bluetooth to connect a "BT04-A" on Arduino. Also containing a WebSocket server to connect to the ScratchX extention.
- ScratchX: The Scrach Extension. It uses websocket to connect with the BluetoothWSServer.

