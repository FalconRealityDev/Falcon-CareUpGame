## Deployment

[Reference Video](https://www.youtube.com/watch?v=2R7-XGoefT0)

The project can be deployed by doing the following steps

# Build Process

- A new branch can be created to track the release
- The scene that needs to be included in the build scene is just the `LoginMenu` and the other scenes are loaded via the `Addressables` when the user clicks on a UI to a new scene
- The Version in the `Build Settings > Player` needs to be updated in each build and the version is baked to the addressables. It is better to close and reload the unity project again as the unity stores some cache
- The Addressable Groups are created automatically by a tool and the it can be accessed by going to `Tools > Organize Assets in Bundles > ++ Start Creating Addressable Groups ++`
- After creating the addressable groups Remove Empty Addressable Groups to clean out empty groups
- You can build the addressable groups by `Window > Asset Management > Addressables > Groups` and then in the addressable groups panel `Build > New Build > Default Build Script`
- The addressable are loaded from the RemoteLoadPath which can be found in the Addressable Profiles

# Deployment

- You can connect to the server using some FTP solution like `FileZilla`.
- The test directory is located in `Web/wgl`. The `WebGL` build files can be uploaded in this directory by creating a new folder with the version
- The `Addressables` are uploaded in `Web/Addressables` directory. A new folder can be created with the version name and the files can be uploaded there
- After uploading the `Build` and the `Addressables` , the project can be tested in the website `leren.careup.online/wgl/{Folder Name}`
- Once everything is tested, it can be uploaded to the folder `web`.
- All the existing files are renamed and moved to the `web/deleted` folder to have a backup

# Changing the Link of the Addressables

- To change the remote load url of the addressables, we can change the `ReleaseBundleAddress` in the script `Assets/Scripts/GetServerAddr`
