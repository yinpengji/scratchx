/* Extension demonstrating a blocking command block */
/* Sayamindu Dasgupta <sayamindu@media.mit.edu>, May 2014 */
var id = 0;
function getID(){
	id = id + 1;
	return id;
}
function wait(time){
	window.setTimeout(function() {
            //callback();
        }, time*1000);
}
new (function() {
    var ext = this;

	var ws;
	var connected = false;
	if ("WebSocket" in window) {
	   console.log("WebSocket is supported by your Browser!");
	   
	   // Let us open a web socket for the websocket server that connect to bluetooth device
	   ws = new WebSocket("ws://localhost:1339/BleDevice");
	   
	   ws.onopen = function() {
		  
		  // Web Socket is connected, send data using send()
		  
		  //var checkStatus = {"ID": getID(), "Command":"CheckStatus"};
		  
		  //var msg = JSON.stringify(checkStatus);
		  
		  //ws.send(msg);
		  //console.log("Message:'" + msg + "' is sent...");
		  connected = true;
	   };
		
	   ws.onmessage = function (evt) { 
		  var received_msg = evt.data;
		  jobj = JSON.parse(received_msg);
		  // message parser
		  
		  if (jobj["Result"] != "OK")
		  {
			  console.error("error happens!");
		  }
		  
		  console.log("Message:'" +  received_msg + "' is received...");
	   };
		
	   ws.onclose = function() { 
		  
		  connected = false;
		  
		  // websocket is closed.
		  console.log("Connection is closed..."); 
	   };
	} else {
	  
	   // The browser doesn't support WebSocket
	   console.log("WebSocket NOT supported by your Browser!");
	}
	
	ext.resetAll = function(){};
	ext.Go = function(time, leftSpeed, rightSpeed){
		//Let the car run at speed.
		console.log("time:" + time);
		console.log("rightSpeed:" + rightSpeed);
		console.log("leftSpeed:" + leftSpeed);
		var gocommand = {"ID": getID(), "Command":"Go", "Parameters":[time, rightSpeed, leftSpeed]};
		var cmdstr =JSON.stringify(gocommand); 
		console.log(cmdstr);
		ws.send(cmdstr);
		wait(time);
	};
	ext.Forward = function(time){
		var gocommand = {"ID": getID(), "Command":"Forward", "Parameters":[time]};
		var cmdstr =JSON.stringify(gocommand); 
		console.log(cmdstr);
		ws.send(cmdstr);
		wait(time);
	};
	
	ext.Back = function(time){
		var gocommand = {"ID": getID(), "Command":"Back", "Parameters":[time]};
		var cmdstr =JSON.stringify(gocommand); 
		console.log(cmdstr);
		ws.send(cmdstr);
		wait(time);
	};
	ext.Back = function(time){
		var gocommand = {"ID": getID(), "Command":"Back", "Parameters":[time]};
		var cmdstr =JSON.stringify(gocommand); 
		console.log(cmdstr);
		ws.send(cmdstr);
		wait(time);
	};
	ext.Brake = function(time){
		var gocommand = {"ID": getID(), "Command":"Brake", "Parameters":[time]};
		var cmdstr =JSON.stringify(gocommand); 
		console.log(cmdstr);
		ws.send(cmdstr);
		wait(time);
	};
	ext.Left = function(time){
		var gocommand = {"ID": getID(), "Command":"Left", "Parameters":[time]};
		var cmdstr =JSON.stringify(gocommand); 
		console.log(cmdstr);
		ws.send(cmdstr);
		wait(time);
	};
	ext.Right = function(time){
		var gocommand = {"ID": getID(), "Command":"Right", "Parameters":[time]};
		var cmdstr =JSON.stringify(gocommand); 
		console.log(cmdstr);
		ws.send(cmdstr);
		wait(time);
	};
	ext.SpinLeft = function(time){
		var gocommand = {"ID": getID(), "Command":"SpinLeft", "Parameters":[time]};
		var cmdstr =JSON.stringify(gocommand); 
		console.log(cmdstr);
		ws.send(cmdstr);
		wait(time);
	};
	ext.SpinRight = function(time){
		var gocommand = {"ID": getID(), "Command":"SpinRight", "Parameters":[time]};
		var cmdstr =JSON.stringify(gocommand); 
		console.log(cmdstr);
		ws.send(cmdstr);
		wait(time);
	};
    ext._shutdown = function() {
    };

    ext._getStatus = function() {
        if(!connected) return {status: 1, msg: 'ArduinoCar disconnected'};
        
        return {status: 2, msg: 'ArduinoCar connected'};
    }
    var descriptor = {
        blocks: [
            [' ', 'Run %n seconds wheel speed： left %n, right %n ',   'Go', 2.0, 150, 150, ],
            [' ', 'Go Forward for %n seconds ',   'Forward', 2.0 ],
			[' ', 'Go Back for %n seconds ',      'Back', 2.0 ],
			[' ', 'Brake for %n seconds ',   'Brake', 1.0 ],
			[' ', 'Turn Left for %n seconds ',   'Left', 1.0 ],
			[' ', 'Turn Right for %n seconds ',   'Right', 1.0 ],
			[' ', 'Spin Left for %n seconds ',   'SpinLeft', 1.0 ],
			[' ', 'Spin Right for %n seconds ',   'SpinRight', 1.0 ],
        ]
    };
    ScratchExtensions.register('ARduinoCar', descriptor, ext);
})();