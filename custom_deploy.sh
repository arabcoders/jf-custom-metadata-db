#!/usr/bin/env bash

set -eou pipefail

if [ -d "/home/coders/apps/jf-custom-metadata-db/bin/" ]; then
    rm -r /home/coders/apps/jf-custom-metadata-db/bin/
fi

if [ -d "/home/coders/apps/jf-custom-metadata-db/CustomMetadataDB/bin/" ]; then
    rm -r /home/coders/apps/jf-custom-metadata-db/CustomMetadataDB/bin/
fi

cd /home/coders/apps/jf-custom-metadata-db

#dotnet test

dotnet publish CustomMetadataDB --configuration Release --output /home/coders/apps/jf-custom-metadata-db/bin

rm -v /home/coders/.docker/data/jellyfin/data/plugins/CustomMetadataDB/*

cp -v /home/coders/apps/jf-custom-metadata-db/bin/*.dll /home/coders/.docker/data/jellyfin/data/plugins/CustomMetadataDB/
