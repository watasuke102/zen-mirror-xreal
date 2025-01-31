TARGET           := ZenMirrorForXREAL.apk
UNITY_EXECUTABLE ?= $(HOME)/Unity/Hub/Editor/2022.3.53f1/Editor/Unity
UNITY_ASSETS_DIR := unity-project/Assets
NRSDK_DIR        := $(UNITY_ASSETS_DIR)/NRSDK
ZEN_MIRROR_LIB   := $(UNITY_ASSETS_DIR)/Plugins/ZenMirrorXrealLib.aar

.PHONY: all apk $(TARGET) plugin $(ZEN_MIRROR_LIB) install

all: $(TARGET) install

apk: $(TARGET)
$(TARGET): $(NRSDK_DIR) $(ZEN_MIRROR_LIB)
	$(UNITY_EXECUTABLE) -quit -batchmode -nographics -buildTarget Android -executeMethod Builder.Build -projectPath unity-project

plugin: $(ZEN_MIRROR_LIB)
$(ZEN_MIRROR_LIB):
	cd plugin && ./gradlew deployPlugin

# do not set TARGET as a dependency to avoid building
# try install without building will obviously fail
install:
	adb -d install $(TARGET)

$(NRSDK_DIR):
	$(error "NRSDK is not imported; open `unity-project` directory with Unity and import NRSDK unitypackage.")
