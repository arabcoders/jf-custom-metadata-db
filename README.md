# Custom Metadata DB

This project is to make a custom database for my custom shows. You can clone and change what's needed to make it work for your use case.

## Build and Installing from source

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
8. If performed correctly you will see a plugin named CustomMetadataDB in `Admin -> Dashboard -> Advanced -> Plugins`.
