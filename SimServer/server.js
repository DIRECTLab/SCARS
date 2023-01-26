const WebSocket = require('ws')
const exec = require('child_process')
const fs = require('fs')
const ObjectsToCsv = require('objects-to-csv');
const { exit } = require('process');

const PUSH = 'PUSH';
const PULL = 'PULL';
const SUBSCRIBE = 'SUBSCRIBE';
const UNSUBSCRIBE = 'UNSUBSCRIBE';
const INFO = 'INFO';

const SIMULATION_ALLOWED_TIME_MILLIS = 10000000;

//////////////////////////////////////////////
// UPDATE THE VALUES IN THIS SECTION TO MAKE SURE THE SAVE IS RIGHT
//////////////////////////////////////////////

const PLANNERS = ['LoS', 'proximity', 'distance']
let currentPlanner = 0;
let numberObservers = [2, 4, 6, 8, 10];
let currentObserver = 0;

const PLANNER_TYPE = 'astar'
const ROOM_NUMBER = 6;
const COST_TYPE = 'proximity'
const MAX_ITERATIONS = 101;

// ANOTHER NOTE

// Make sure you update on SimAgent(UNITY) which JSON file you are using. LMK (taylor)
// if you want to know how to setup the file

//////////////////////////////////////////////
// ^^^^^^^^^^^^^
//////////////////////////////////////////////


let runNumber = 1;

const generateCSVname = () => `results/${PLANNERS[currentPlanner]}-room-${ROOM_NUMBER}-observers-${numberObservers[currentObserver]}.csv`

// const CSV_FILE_NAME = `results/${COST_TYPE}-room-${ROOM_NUMBER}-run-${runNumber}.csv`;

const REALLY_LONG_COMMAND_THAT_WORKS_ON_RYANS_COMPUTER = "/home/sonorousduck/.jdks/corretto-17.0.4.1/bin/java -javaagent:/home/sonorousduck/idea-IC-222.3739.54/lib/idea_rt.jar=39187:/home/sonorousduck/idea-IC-222.3739.54/bin -Dfile.encoding=UTF-8 -classpath /home/sonorousduck/Desktop/StealthSim/stealth-planner/build/classes/kotlin/main:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/space.kscience/plotlykt-server/0.5.0/f95e6ee63bc76e1a94eedcf1b71bb4fa710c01f4/plotlykt-server-0.5.0.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlin/kotlin-stdlib-jdk8/1.7.10/d70d7d2c56371f7aa18f32e984e3e2e998fe9081/kotlin-stdlib-jdk8-1.7.10.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/com.google.code.gson/gson/2.9.0/8a1167e089096758b49f9b34066ef98b2f4b37aa/gson-2.9.0.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-server-cio/1.6.1/506321f1e0f60f6151a0e474079a3425a00bc645/ktor-server-cio-jvm-1.6.1.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-html-builder/1.6.1/e2898df1b095c1e8f9142487d76fb59b4c733446/ktor-html-builder-jvm-1.6.1.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-client-cio-jvm/2.0.3/441a76bd95e9f33ae15bf1ae5c351b70537ca4d8/ktor-client-cio-jvm-2.0.3.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-client-websockets-jvm/2.0.3/30723df5a3b9096fd9e828db0d6c4897c9cd90a0/ktor-client-websockets-jvm-2.0.3.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-client-core-jvm/2.0.3/e070c1592e85cc10067f5dc65c4c5a3a4e0e8022/ktor-client-core-jvm-2.0.3.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlin/kotlin-stdlib-jdk7/1.7.10/1ef73fee66f45d52c67e2aca12fd945dbe0659bf/kotlin-stdlib-jdk7-1.7.10.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlin/kotlin-stdlib/1.7.10/d2abf9e77736acc4450dc4a3f707fa2c10f5099d/kotlin-stdlib-1.7.10.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlinx/kotlinx-serialization-json-jvm/1.3.3/6298c404c1159685fb657f15fa5b78630da58c96/kotlinx-serialization-json-jvm-1.3.3.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/org.slf4j/slf4j-api/1.7.36/6c62681a2f655b49963a5983b8b0950a6120ae14/slf4j-api-1.7.36.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlinx/kotlinx-html-jvm/0.7.3/cfc1aab4fbd794458011cba04f2c25acda3b0aed/kotlinx-html-jvm-0.7.3.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/space.kscience/plotlykt-core-jvm/0.5.0/be20099a220434aae79bae88d43fcce4c854eea/plotlykt-core-jvm-0.5.0.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/space.kscience/dataforge-context-jvm/0.5.0/8b1abd8375848f6f951ee94df5361af17f1c557f/dataforge-context-jvm-0.5.0.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-websockets-jvm/2.0.3/67920d884f3ea3d9c42efc83fc576cf95723090d/ktor-websockets-jvm-2.0.3.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlinx/kotlinx-coroutines-jdk8/1.6.2/acf5ee3be3fdde5b2c8d7c5fa64af51f5d06317e/kotlinx-coroutines-jdk8-1.6.2.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlin/kotlin-stdlib-common/1.7.10/bac80c520d0a9e3f3673bc2658c6ed02ef45a76a/kotlin-stdlib-common-1.7.10.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/org.jetbrains/annotations/13.0/919f0dfe192fb4e063e7dacadee7f8bb9a2672a9/annotations-13.0.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-server-host-common/1.6.1/9698835384fe9db43a717c50b75706d711995c45/ktor-server-host-common-jvm-1.6.1.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-http-cio-jvm/2.0.3/a475cb902fb598706d96422ac7fa49ace97eb5fa/ktor-http-cio-jvm-2.0.3.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-network-jvm/2.0.3/a6e37fbeaf3a36c627843601b748dcd084c4c23a/ktor-network-jvm-2.0.3.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-server-core/1.6.1/b9a1d11e97a0815870e3e61cd6586aee1f8bc272/ktor-server-core-jvm-1.6.1.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlin/kotlin-reflect/1.5.21/802f1f39735ae1eb2b75714a40fa19bb2e687e96/kotlin-reflect-1.5.21.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-network-tls-jvm/2.0.3/76413c4192466bf5c2caeea23f5886eaa66f40a5/ktor-network-tls-jvm-2.0.3.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlinx/kotlinx-coroutines-core-jvm/1.6.2/c2f7205a38888f9c96150a5f42bec9d4cb16f059/kotlinx-coroutines-core-jvm-1.6.2.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-events-jvm/2.0.3/407b2eede064448e987162bbaf13b21d92546bee/ktor-events-jvm-2.0.3.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-websocket-serialization-jvm/2.0.3/5b524e8fb2b3afd052b81d72cff84d4242b66a49/ktor-websocket-serialization-jvm-2.0.3.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-http-jvm/2.0.3/cc8f40ea70fcd24540ceb256df2eb7ea97611435/ktor-http-jvm-2.0.3.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlinx/kotlinx-serialization-core-jvm/1.3.3/3c22103b5d8e3c262db19e85d90405ca78b49efd/kotlinx-serialization-core-jvm-1.3.3.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/com.typesafe/config/1.3.1/2cf7a6cc79732e3bdf1647d7404279900ca63eb0/config-1.3.1.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/space.kscience/dataforge-meta-jvm/0.5.0/361f9a70fe5d9a37fb0e8f9baed729a7130e8cab/dataforge-meta-jvm-0.5.0.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-utils-jvm/2.0.3/bb2d10070fa26e50d350a8efa133b4a95ebffdaa/ktor-utils-jvm-2.0.3.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-serialization-jvm/2.0.3/4ab5ab0c7e36c3ae5405f21de14325640688d7e7/ktor-serialization-jvm-2.0.3.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-io-jvm/2.0.3/d944d914b3a4686a91cbc8f68a0f034a322df15b/ktor-io-jvm-2.0.3.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlinx/kotlin-jupyter-api-annotations/0.10.0-126-1/aab1febaff3416973d65530a77dd0e9c18de5385/kotlin-jupyter-api-annotations-0.10.0-126-1.jar:/home/sonorousduck/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlinx/kotlinx-coroutines-slf4j/1.6.2/cd4633b721684d6a49de4f91f23463efedb3ee6b/kotlinx-coroutines-slf4j-1.6.2.jar MainKt"

const TAYLORS_COMPUTER = '/home/direct-lab/.jdks/corretto-17.0.4.1/bin/java -javaagent:/home/direct-lab/idea-IC-222.3739.54/lib/idea_rt.jar=42553:/home/direct-lab/idea-IC-222.3739.54/bin -Dfile.encoding=UTF-8 -classpath /home/direct-lab/taylor/StealthSim/stealth-planner/build/classes/kotlin/main:/home/direct-lab/.gradle/caches/modules-2/files-2.1/space.kscience/plotlykt-server/0.5.0/f95e6ee63bc76e1a94eedcf1b71bb4fa710c01f4/plotlykt-server-0.5.0.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlin/kotlin-stdlib-jdk8/1.7.10/d70d7d2c56371f7aa18f32e984e3e2e998fe9081/kotlin-stdlib-jdk8-1.7.10.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/com.google.code.gson/gson/2.9.0/8a1167e089096758b49f9b34066ef98b2f4b37aa/gson-2.9.0.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-server-cio/1.6.1/506321f1e0f60f6151a0e474079a3425a00bc645/ktor-server-cio-jvm-1.6.1.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-html-builder/1.6.1/e2898df1b095c1e8f9142487d76fb59b4c733446/ktor-html-builder-jvm-1.6.1.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-client-cio-jvm/2.0.3/441a76bd95e9f33ae15bf1ae5c351b70537ca4d8/ktor-client-cio-jvm-2.0.3.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-client-websockets-jvm/2.0.3/30723df5a3b9096fd9e828db0d6c4897c9cd90a0/ktor-client-websockets-jvm-2.0.3.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-client-core-jvm/2.0.3/e070c1592e85cc10067f5dc65c4c5a3a4e0e8022/ktor-client-core-jvm-2.0.3.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlin/kotlin-stdlib-jdk7/1.7.10/1ef73fee66f45d52c67e2aca12fd945dbe0659bf/kotlin-stdlib-jdk7-1.7.10.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlin/kotlin-stdlib/1.7.10/d2abf9e77736acc4450dc4a3f707fa2c10f5099d/kotlin-stdlib-1.7.10.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlinx/kotlinx-serialization-json-jvm/1.3.3/6298c404c1159685fb657f15fa5b78630da58c96/kotlinx-serialization-json-jvm-1.3.3.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/org.slf4j/slf4j-api/1.7.36/6c62681a2f655b49963a5983b8b0950a6120ae14/slf4j-api-1.7.36.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlinx/kotlinx-html-jvm/0.7.3/cfc1aab4fbd794458011cba04f2c25acda3b0aed/kotlinx-html-jvm-0.7.3.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/space.kscience/plotlykt-core-jvm/0.5.0/be20099a220434aae79bae88d43fcce4c854eea/plotlykt-core-jvm-0.5.0.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/space.kscience/dataforge-context-jvm/0.5.0/8b1abd8375848f6f951ee94df5361af17f1c557f/dataforge-context-jvm-0.5.0.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-websockets-jvm/2.0.3/67920d884f3ea3d9c42efc83fc576cf95723090d/ktor-websockets-jvm-2.0.3.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlinx/kotlinx-coroutines-jdk8/1.6.2/acf5ee3be3fdde5b2c8d7c5fa64af51f5d06317e/kotlinx-coroutines-jdk8-1.6.2.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlin/kotlin-stdlib-common/1.7.10/bac80c520d0a9e3f3673bc2658c6ed02ef45a76a/kotlin-stdlib-common-1.7.10.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/org.jetbrains/annotations/13.0/919f0dfe192fb4e063e7dacadee7f8bb9a2672a9/annotations-13.0.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-server-host-common/1.6.1/9698835384fe9db43a717c50b75706d711995c45/ktor-server-host-common-jvm-1.6.1.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-http-cio-jvm/2.0.3/a475cb902fb598706d96422ac7fa49ace97eb5fa/ktor-http-cio-jvm-2.0.3.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-network-jvm/2.0.3/a6e37fbeaf3a36c627843601b748dcd084c4c23a/ktor-network-jvm-2.0.3.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-server-core/1.6.1/b9a1d11e97a0815870e3e61cd6586aee1f8bc272/ktor-server-core-jvm-1.6.1.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlin/kotlin-reflect/1.5.21/802f1f39735ae1eb2b75714a40fa19bb2e687e96/kotlin-reflect-1.5.21.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-network-tls-jvm/2.0.3/76413c4192466bf5c2caeea23f5886eaa66f40a5/ktor-network-tls-jvm-2.0.3.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlinx/kotlinx-coroutines-core-jvm/1.6.2/c2f7205a38888f9c96150a5f42bec9d4cb16f059/kotlinx-coroutines-core-jvm-1.6.2.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-events-jvm/2.0.3/407b2eede064448e987162bbaf13b21d92546bee/ktor-events-jvm-2.0.3.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-websocket-serialization-jvm/2.0.3/5b524e8fb2b3afd052b81d72cff84d4242b66a49/ktor-websocket-serialization-jvm-2.0.3.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-http-jvm/2.0.3/cc8f40ea70fcd24540ceb256df2eb7ea97611435/ktor-http-jvm-2.0.3.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlinx/kotlinx-serialization-core-jvm/1.3.3/3c22103b5d8e3c262db19e85d90405ca78b49efd/kotlinx-serialization-core-jvm-1.3.3.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/com.typesafe/config/1.3.1/2cf7a6cc79732e3bdf1647d7404279900ca63eb0/config-1.3.1.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/space.kscience/dataforge-meta-jvm/0.5.0/361f9a70fe5d9a37fb0e8f9baed729a7130e8cab/dataforge-meta-jvm-0.5.0.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-utils-jvm/2.0.3/bb2d10070fa26e50d350a8efa133b4a95ebffdaa/ktor-utils-jvm-2.0.3.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-serialization-jvm/2.0.3/4ab5ab0c7e36c3ae5405f21de14325640688d7e7/ktor-serialization-jvm-2.0.3.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/io.ktor/ktor-io-jvm/2.0.3/d944d914b3a4686a91cbc8f68a0f034a322df15b/ktor-io-jvm-2.0.3.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlinx/kotlin-jupyter-api-annotations/0.10.0-126-1/aab1febaff3416973d65530a77dd0e9c18de5385/kotlin-jupyter-api-annotations-0.10.0-126-1.jar:/home/direct-lab/.gradle/caches/modules-2/files-2.1/org.jetbrains.kotlinx/kotlinx-coroutines-slf4j/1.6.2/cd4633b721684d6a49de4f91f23463efedb3ee6b/kotlinx-coroutines-slf4j-1.6.2.jar MainKt'

const generateSaveFileName = () => `results/map_${ROOM_NUMBER}/${PLANNER_TYPE}-${COST_TYPE}-run-${runNumber}.json`
const IS_SIM = true;
const values = {};
let locations = [];

const subscriptions = {};
const genericSubscriptions = [];

let completed = false;

const wss = new WebSocket.Server({ port: 8080, maxPayload: 1000000 * 1024 }, () => {
  console.log('Server started on ws://localhost:8080')
});

wss.on('connection', function connection(ws){
  console.log("New Client connected: ")
  ws.on('message', (data) => {

    
    const message = JSON.parse(data.toString('utf-8'));

    if (!message) {
      console.log(data.toString('utf-8'))
    }

    if (!message.purpose){
      console.log(message)
    }

    if (message.purpose === PUSH){

      if (message.type === 'TwistMessage'){
        console.log(message);
      }

      if (message.type === 'PointMovement'){
        console.log(message.data);
        locations.push(message.data)
      }

      if (message.type === 'Completed'){

        if (IS_SIM)
        {
          if (message.data == 'true' && !completed){
            console.log(`Planner: ${PLANNERS[currentPlanner]}: \n\tObservers: ${numberObservers[currentObserver]}\n\tRun Number: ${runNumber}\nComplete`);
            danger = JSON.parse(values.Danger.data);
            const result = [{
              run: runNumber,
              costFunction: PLANNERS[currentPlanner],
              roomNumber: ROOM_NUMBER,
              numberObservers: numberObservers[currentObserver],
              overallDanger: danger.amount,
              totalDistance: danger.totalDistance,
            }];
            locations = [];
            runNumber += 1;
  
            const csv = new ObjectsToCsv(result);
            csv.toDisk(generateCSVname(), { append: true })
  
            if (runNumber >= MAX_ITERATIONS)
            {
              runNumber = 0;
              currentObserver += 1;
            }
  
            if (currentObserver >= numberObservers.length)
            {
              currentObserver = 0;
              currentPlanner += 1;
              if (currentPlanner >= PLANNERS.length)
              {
                console.log("Completed");
                setTimeout(() => { exit() }, 2000);
              }
            }
          }

          completed = message.data === 'true';
          
        } else {
          const filename = generateSaveFileName(0);
          console.log(`Saving results for run ${0} in ${filename}`);
          danger = JSON.parse(values.Danger.data);
          const result = {
            run: runNumber,
            planner: PLANNER_TYPE,
            overallDanger: danger.amount,
            uniqueObservers: danger.uniqueObservers, 
            totalDistance: danger.totalDistance,
            traverseLocations: locations
        };
  
         
        fs.writeFileSync(filename, JSON.stringify(result, null, 2));
        locations = []
        runNumber += 1;
        }
      }

      values[message.type] = { format: message.format, type: message.type, data: message.data };

      genericSubscriptions.forEach(subscriber => {
        subscriber.send(JSON.stringify({ ...values[message.type], purpose: SUBSCRIBE }));
      })

      if (subscriptions[message.type]?.length){
        subscriptions[message.type].forEach(socket => {
          let d = values[message.type].data;
          if (values[message.type].format === 'json'){
            d = JSON.stringify(values[message.type].data);
          }
          socket.send(JSON.stringify({ ...values[message.type], data: d, purpose: SUBSCRIBE }));
        });
      }
    }

    if (message.purpose === PULL){
      if (message.type){
        ws.send(JSON.stringify({ purpose: PULL, data: values[message.type], type: message.type }))
      }
      else{
        ws.send(JSON.stringify({ purpose: PULL, data: values}));
      }
    }

    if (message.purpose === SUBSCRIBE){
      if (message.type){
        console.log(`SUBSCRIBE: ${message.type}`)
        subscriptions[message.type] = [...(subscriptions[message.type] ?? []), ws]
      }
      else{
        console.log("SUBSCRIBE: generic");
        genericSubscriptions.push(ws);
      }
    }

    if (message.purpose === UNSUBSCRIBE){
      if (message.type){
        console.log(`UNSUBSCRIBE: ${message.type}`)
        subscriptions[message.type] = subscriptions[message.type]?.filter(socket => socket != ws);
      }
      else{
        genericSubscriptions = genericSubscriptions.filter(socket => socket !== ws);
      }
      
    }

    if (message.purpose === INFO){
      ws.send(JSON.stringify({ purpose: INFO, data: Object.keys(values) }));
    }

  });
});

wss.on('listening', () => {
  console.log('Server listening on ws://localhost:8080')
})



const startNewSim = (runNumber) => {
  
  return new Promise((resolve) => {
    

    const children = { kotlinChildId: -1 };

    const kotlinChild = exec.exec(`sleep 100000; `, (error, stdout, stderr) => {
      
      if (stderr){
	console.error(stderr);
      }
      clearTimeout(children.kotlinChildId);

      // When this is done, we should log the results somewhere
      const filename = generateSaveFileName(runNumber);
      console.log(`Saving results for run ${runNumber} in ${filename}`);
      danger = JSON.parse(values.Danger.data);
      const result = {
        run: runNumber,
        planner: PLANNER_TYPE,
        overallDanger: danger.amount,
        uniqueObservers: danger.uniqueObservers, 
        totalDistance: danger.totalDistance,
        traverseLocations: locations
      };

       
      fs.writeFileSync(filename, JSON.stringify(result, null, 2));

      resolve()
    });
    children.kotlinChildId = setTimeout(() => {
      kotlinChild.kill()

      console.log("Killed child!")
    }, SIMULATION_ALLOWED_TIME_MILLIS)
  });
}

// Actually runs the program
// (async() => {
//   console.log("Starting up!") 
//   //const unityChild = exec.exec('../IndoorBuild.x86_64', (error, stdout, stderr) => {
//   //    if (stderr){
//   //      console.error(stderr);
//   //    }
// 	//const unityChild = exec.exec('../IndoorBuild.x86_64', (error, stdout, stderr) => {
//   //});
//   for (let i = 0; i < 1; i++){
//     await startNewSim(i);
//   }
//   return;
// })();
