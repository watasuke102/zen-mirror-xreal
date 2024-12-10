import java.util.Properties

plugins {
    id("com.android.library") version "8.7.0"
}

val localProps = Properties().apply {
    load(project.rootProject.file("local.properties").reader())
}
fun getLocalProps(key: String, required: Boolean = false): String? {
    val v = localProps.getProperty(key)
    if (v === null && required) {
        throw org.gradle.api.GradleException("Property '${key}' not found")
    }
    return v
}

android {
    namespace = "net.watasuke.${project.name}"
    compileSdkVersion = "android-31"
    defaultConfig {
        minSdk = 31
        externalNativeBuild {
            var cmake_args = listOf(
                "-DZEN_REMOTE_PROTOC_EXECUTABLE:STRING=${getLocalProps("zwin.protoc")}",
                "-DZEN_REMOTE_GRPC_CPP_PLUGIN_EXECUTABLE:STRING=${getLocalProps("zwin.grpc_cpp_plugin")}",
                "-DZEN_REMOTE_GRPC_SYSROOT:STRING=${getLocalProps("zwin.grpc_sysroot")}",
            )
            val unityDir = getLocalProps("unity_dir", required=false)
            if (unityDir !== null) {
                cmake_args += "-DUNITY_PLUGIN_API_DIR:STRING=${unityDir}"
            }
            cmake {
                cppFlags += listOf("-std=c++23")
                arguments += cmake_args
            }
        }
        ndk {
            abiFilters += listOf("arm64-v8a")
        }
    }
    externalNativeBuild {
        cmake {
            path = file("CMakeLists.txt")
        }
    }
}

tasks.register<Copy>("deployPlugin") {
    group = "install"
    dependsOn("build")
    from("build/outputs/aar") {
        rename(".*", "${project.name}.aar")
    }
    into("../unity-project/Assets/Plugins")
    include("*-release.aar")
}
tasks.register<Copy>("deployDebugPlugin") {
    group = "install"
    dependsOn("build")
    from("build/outputs/aar") {
        rename(".*", "${project.name}.aar")
    }
    into("../unity-project/Assets/Plugins")
    include("*-debug.aar")
}

tasks.register<Delete>("cleanCxx") {
    group = "build"
    description = "Deletes the .cxx directory."
    delete("${projectDir}/.cxx")
}
tasks.named("clean") {
    dependsOn("cleanCxx")
}
