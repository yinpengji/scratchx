# scratchx
A workable solution to use ScratchX extension control an Arduino car with Bluetooh

# Introduction

In simple, the solution is to use a seperated server in the middle of ScratchX and the Arduino device.

- Arduino/CarRun: The ino file that controls the logic for read/write through Bluetooth as a Serial.
- BluetoothWSServer: The Bluetooth connection Server in the middle. Using Bluetooth to connect a "BT04-A" on Arduino. Also containing a WebSocket server to connect to the ScratchX extention.
- ScratchX: The Scrach Extension. It uses websocket to connect with the BluetoothWSServer.

# Usage

1. Start Arudino, make sure the bluetooth started.
2. Start the websocket server. You'll have to change the bluetooth device name and pin in the code. 
3. Load the scratch extention from ScratchX.
   e.g. Go to http://localhost:8088/#scratch, in case you downloaded the ScrachX web package and put then under your webserver.
   Press shift and click "Load Experimental Extension", choose the arduinocar.js to load all the modules.

# Why doing this

My 7 years old son took one programming class in Apple store. In the calss all the children used sphero https://www.sphero.com/ and an IDE likes Scratch to learn how to programming. They practiced how to use those programming blocks to control a sphero bot to work out a maze.

It's nice and interesting, but the price is pretty high. So I think maybe I can customize one of my Arduino car bought from Taobao to make something similar. As a software engineer, I thouth there should be plenty of open sourced samples to make this, and I should be able to finish this in several days.

I found I was wrong when I started searching google. I found people asked the same question like "how to use scratch to control my arduino through bluetooth", and no single reply. May be I'm wrong, the Scratch is very poor on extensiblity. I still don't know how to make an extra block in Scratch. I also tried S4A16, but it still cannot answer my question on the bluetooth part. I used the liberay of  32feet to connect my bluetooth device. Thanks for the code https://gist.github.com/elbruno/8d253679235a2d6fe008.

From the Scratch site, I saw 2 kinds of extensions, one is ScratchX, another one is something like to connect with a server app like http server. I first checked the app server one, seems no good example and looks like the documentation is not finished. I turned to the ScratchX, pretty handy and easy. But for the bluetooth, there are something to connect LeGo, while not works for my device. I tried to install the device manager and the chrome plugin, still not working. I debugged the code and try to figure out, and I saw it's just using js to query device from a server and the server is defined like "https://device-manager.scratch.mit.edu:3030", which is redirected to localhost. It's also a webserver helping to manage all the devices. So I turned back to the first option, and decide to use my own websocket server. 

When working on the websocket server, my first choice is using nodejs, while it's hard to find a workable bluetooth lib in node for connecting my BT04-A. So use the C# code since the bluetooth part has already working, and there's some good lib for websocket service. JSON is used for the command transfered between the ScratchX and the Arduino.

# TODO:
- Add a 9250 module to report the angle of the car.
- Target is to use ipad contorl the car. Still need to find how to make the extention run from ipad.
- The code need to be refined, of course ;)
- Some features like automatically retry/restart connection once found the connection is closed. For both bluetooth and scratch extension.
