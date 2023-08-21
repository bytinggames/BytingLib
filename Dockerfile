# alpine fails building the assets later on, probably because some deps missing
# FROM alpine
# bash is needed for some internal build scripts which i'm not touching yet
# RUN apk add wine 7zip curl dotnet6-sdk assimp-dev bash

FROM ubuntu
RUN apt update && apt install -y git wine64 p7zip-full curl dotnet-sdk-6.0 libassimp-dev

WORKDIR /tmp
COPY ./Deploy/crashdialog.reg ./crashdialog.reg

# Disables the wine crash dialog
RUN wine64 regedit ./crashdialog.reg

# maybe these should be bundled somewhere
ENV DOTNET_URL="https://download.visualstudio.microsoft.com/download/pr/44d08222-aaa9-4d35-b24b-d0db03432ab7/52a4eb5922afd19e8e0d03e0dbbb41a0/dotnet-sdk-6.0.302-win-x64.zip"
# Needed for d3dcompiler_47.dll
# This could be installed with wine tricks, but that's still in the test repos on alpine
ENV FIREFOX_URL="https://download-installer.cdn.mozilla.net/pub/firefox/releases/62.0.3/win64/ach/Firefox%20Setup%2062.0.3.exe"

ENV PATH="${PATH}:/opt/.dotnet/tools"
ENV MGFXC_WINE_PATH="/opt/.winemonogame"
ENV WINEARCH="win64"
ENV WINEPREFIX="/opt/.winemonogame"

WORKDIR /tmp/deps
RUN curl $DOTNET_URL --output "./dotnet-sdk.zip"
RUN 7z x "./dotnet-sdk.zip" -o"$WINEPREFIX/drive_c/windows/system32/"

RUN curl $FIREFOX_URL --output "./firefox.exe"
RUN 7z x "./firefox.exe" -o"./firefox_data/"
RUN cp -f "./firefox_data/core/d3dcompiler_47.dll" "$WINEPREFIX/drive_c/windows/system32/d3dcompiler_47.dll"

RUN rm -rf "/tmp/deps/"

WORKDIR /opt
