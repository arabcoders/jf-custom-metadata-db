# Custom Metadata DB

![Build Status](https://github.com/ArabCoders/jf-custom-metadata-db/actions/workflows/build-validation.yml/badge.svg)
![MIT License](https://img.shields.io/github/license/ArabCoders/jf-custom-metadata-db.svg)

This is a plugin for [Jellyfin Media Server](https://jellyfin.org/) that purely here to generate unique custom IDs for TV series. to work with
my other two plugins for [Emby](https://github.com/arabcoders/emby-custom-metadata-db) and [Plex](https://github.com/arabcoders/cmdb.bundle).

**This plugin require server that respond with a unique ID for a given TV series name in the following format.**

```json5
[
    {
        "id": "jp-bded21fb4b",
        "title": "Show Real title",
        "match": [
            "Matchers"
        ]
    },
    ...
]
```

## Server implementation

For a quick server implementation please refer to [this page](https://github.com/arabcoders/cmdb.bundle?tab=readme-ov-file#server-implementation).

# Installation

Go to the Releases page and download the latest release.

Unzip the file and make sure the contents are stored inside a folder named `CustomMetadataDB` and then copy the folder to the Jellyfin plugins directory. You can find your directory by going to Dashboard, and noticing the Paths section. Mine is the root folder of the default Metadata directory.

## Building from source

1. Clone or download this repository.
2. Ensure you have .NET Core SDK setup and installed.
3. Build plugin with following command.
   ```bash
   $ dotnet publish CustomMetadataDB --configuration Release --output bin
   ```
4. Create folder named `CustomMetadataDB` in the `plugins` directory inside your Jellyfin data
   directory. You can find your directory by going to Dashboard, and noticing the Paths section.
   Mine is the root folder of the default Metadata directory.
   ```bash
   $ mkdir -p <Jellyfin Data Directory>/plugins/CustomMetadataDB/
   ```
5. Place the resulting files from step 3 in the `plugins/CustomMetadataDB` folder created in step 4.
   ```bash
   $ cp -r bin/*.dll <Jellyfin Data Directory>/plugins/CustomMetadataDB/`
   ```
6. Be sure that the plugin files are owned by your `jellyfin` user:
   ```bash
   $ chown -R jellyfin:jellyfin /var/lib/jellyfin/plugins/CustomMetadataDB/
   ```
7. Restart jellyfin.

If performed correctly you will see a plugin named `CustomMetadataDB` in `Admin -> Dashboard -> Advanced -> Plugins`.

# Change the API URL

go to `Admin -> Dashboard -> Advanced -> Plugins -> Custom Metadata DB -> URL for the custom database:` and change the URL to your server URL, then click on `Save`.
