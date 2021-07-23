var bleService = 'cycling_speed_and_cadence';//GATT service requied in sensor 
var bleCharacteristic = 'csc_measurement'; //GATT characteristic desrited
var bluetoothDeviceDetected; //Device object
var gattCharacteristic; //GATT characteristic object
var isConnected = false;

console.log("**Created with HealthGamez Web Bluetooth Cycling and Cadence sensor plugin – Lite by www.healthgamez.fun ");

//function to check if browser support web-BLT API
function isWebBluetoothEnabled() {
    if (!navigator.bluetooth) {
        window.alert("Web Bluetooth API is not available in this browser!");
        return false;
    }
    return true;
}

function getDeviceInfo() {
    return new Promise(function (resolve, reject) {
        console.log("Requesting cycling speed and cadence Bluetooth Device...");
        return navigator.bluetooth.requestDevice({filters:[{ services: [bleService] }]})
	    .then( function(device){
            bluetoothDeviceDetected = device;
            console.log("device detected: " + bluetoothDeviceDetected.name);
            resolve(bluetoothDeviceDetected);
        }).catch(function(error){
            window.alert("ERROR " + error);
            reject(error);
        })

    })
}

function connect() {
    if (!isWebBluetoothEnabled()){
		return
	}
	if (!bluetoothDeviceDetected) {
        getDeviceInfo()
            .then(function () {
                bluetoothDeviceDetected.addEventListener('gattserverdisconnected', onDisconnected);
                return bluetoothDeviceDetected.gatt.connect()
            })
            .then(function (server) {
                console.log('Connected to Gatt-server');
                console.log('Getting GATT Service...');
                console.log(server);
                return server.getPrimaryService(bleService);
            })
            .then(function (service) {
                console.log('Getting GATT Characteristic...');
                return service.getCharacteristic(bleCharacteristic);
            })
            .then(function (characteristic) {
                gattCharacteristic = characteristic;
            })
            .then(function () {
                gattCharacteristic.startNotifications();
            })
            .then(function () {
                gattCharacteristic.addEventListener('characteristicvaluechanged', handleCharacteristicValueChanged);
                console.log('Start reading...');
                isConnected = true;

            })
            .catch(function (error) {
                window.alert("ERROR " + error);
            })
    }
}

//Called on sensor disconnect
function onDisconnected(event) {
    var device = event.target;
    console.log("Device "+ device.name+ " is disconnected.");
    isConnected = false;
    bluetoothDeviceDetected=null;
}

//Called when notification recived from sensor
function handleCharacteristicValueChanged(event) {
    var value = event.target.value;
    var parsedCscValue = parseCscValue(value);
    SendMessage('WebBTCycleSpeedCadencePlugin', 'UpdateCscMeasurement', JSON.stringify(parsedCscValue));
}

//Parsing function for CSC value
function parseCscValue(value) {
    value = value.buffer ? value : new DataView(value);    // In Chrome 50+, a DataView is returned instead of an ArrayBuffer.
    var flagField = value.getUint8(0);
    var result = {};
    result.FlagField = flagField;
    var index =1;

    switch (flagField) {
        case 1: //Sensor is Wheel revolution sensor
            result.CumulativeWheelRevolutions = value.getUint32(index, /*littleEndian=*/true);
            index += 4;
            result.WheelTimeStamp = value.getUint16(index, /*littleEndian=*/true);
            break;

        case 2: //Sensor is Crank revolution sensor
            result.CumulativeCrankRevolutions = value.getUint16(index, /*littleEndian=*/true);
            index += 2;
            result.CrankTimeStamp = value.getUint16(index, /*littleEndian=*/true);
            break;

        case 3: //Sensor is Wheel and Crank revolution sensor
            result.CumulativeWheelRevolutions = value.getUint32(index, /*littleEndian=*/true);
            index += 4;
            result.WheelTimeStamp = value.getUint16(index, /*littleEndian=*/true);

            result.CumulativeCrankRevolutions = value.getUint16(index, /*littleEndian=*/true);
            index += 2;
            result.CrankTimeStamp = value.getUint16(index, /*littleEndian=*/true);
            break;

        default: //This should never happen
            console.log("error, undefined flagfield value:" + flagField);
    }
    return result;
}