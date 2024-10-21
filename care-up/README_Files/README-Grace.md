# FalconReality-Grace

- This document explains how the Grace is implemented in the CareUp Scenes

## Grace AI

- Grace has three different APIs integrated.
- Grace uses `Open AI APIs` . It uses `Whisper (Speech-to-Text)`, `Chat Completion` and `TTS (Text-to-Speech)` APIs.
- Once the response for a request has been created the data is sent to the server and it then saves it to the database.

## Locations of Important Assets

- The assets and scripts for Grace is in the **Samples** Folder
- Two UIs **Desktop** & **Mobile** is copied to the **Resources>NecessaryPrefabs>UI**
- All the **API** & **MANAGER** scripts are made as a prefab and included in the **Resources>NecessaryPrefabs** folder

## Execution in the Scenes

- The necessary prefabs are instantiated in the scene by the **PlayerSpawn** script

## Data for the Grace Dashboard

- Everytime a user makes a conversation with the Grace AI , a **POST** request is sent to the server to store the data
- The data sent includes the **User ID** and **Protocol Group**.
- The protocols are grouped into **11 Groups**.
- The XML file in the **Resources>Xml>SceneAndTheirGroups** is created to group the scenes to their corresponding group.
- The scripts `SendDataToDB`, `GetUserID`,`SceneGroupChecker` can be referred to have better clarity.
