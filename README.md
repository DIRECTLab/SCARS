# Notes on this

Unity Version 2021.3.6f1

## Helpful Papers

[A LiDAR Point Cloud Generator: from a Virtual World to
Autonomous Driving](https://par.nsf.gov/servlets/purl/10109208)

## ROS Format for LiDAR

You can find some info about the format for the LiDAR senson here. It assumes a planar setup for now

[http://docs.ros.org/en/lunar/api/sensor_msgs/html/msg/LaserScan.html](http://docs.ros.org/en/lunar/api/sensor_msgs/html/msg/LaserScan.html)

## Source for procedural gen

[https://gamedevacademy.org/complete-guide-to-procedural-level-generation-in-unity-part-1/](https://gamedevacademy.org/complete-guide-to-procedural-level-generation-in-unity-part-1/)

## About the communication protocol

I made this one up, but I'll put my notes on it here so it's easier.


The communication happens over websockets, mediated by a central server. All communication happens through a standard format- that is, JSON.

Every message has at most 4 parts (not all need all parts):

```JSON
{
  "purpose": "PUSH",
  "type": "dangerAmount",
  "format": "json",
  "data": "{...}"
}
```

### Message Parts

#### **Purpose**

This tells the server whether you are updating the server's current data, or if you are requesting the current. It can have one of 3 values:

* "PUSH"
* "PULL"
* "SUBSCRIBE"
* "UNSUBSCRIBE"
* "INFO"

Push puts data in the server, pull requests it, and subscribe lets you, well subscribe to it. What this means, is instead of having to request for the current data, you can instead tell the server you want to be notified of any updates to the given "type" you subscribed to. This is convenient for actually controlling the robot, so the robot doesn't need to make a ton of requests to get the current actions. Unsubscribe takes you off the notification list.

Info command requests a list of the currently collected data.

#### **Type**

NOT REQUIRED ON PULL (but can be specified)

This tells the server what data it is storing. Internally, this is used as the 'lookup' for the given data. It can have any value, for whatever the thing that created the data is (camera, LiDAR, etc.). Additionally, if you send a message with purpose "PULL", and specify a type, it will grab only that data, instead of the full storage.

#### **Format**

NOT NEEDED ON PULL OR SUBSCRIBE

This tells the server if it needs to do any extra processing on the data before storing it. For most this won't be necessary (numbers and strings will already be parsed by the initial JSON read). This is mainly important for odd formats like nested JSON, so it knows it needs to parse it again.

**Special Formats**

* json-string (server will parse this into JSON and then store it as regular JSON)

#### **Data**

NOT NEEDED ON PULL OR SUBSCRIBE

The actual data you are wanting to store/send.

### General Info

The server will always echo back the info when it gives you data. What this means, is that if the data is coming from a subscription event, you will recieve the following JSON

```JSON
{
  "purpose": "SUBSCRIBE",
  "type": "dangerAmount",
  "format": "object",
  "data": "{...}",
}
```

In contrast, if the data is coming because of a pull event, you will receive this:

```JSON
{
  "purpose": "PULL",
  "type": "dangerAmount",
  "data": "{...}",
}
```

For many purposes this may not matter, but I thought the extra info could be useful, since the server already knows it anyways.# Stealth Centric Autonomous Robot System
