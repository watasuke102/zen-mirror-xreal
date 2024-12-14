using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class Builder
{
  private static void ProcessBuild(bool silent)
  {
    var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
    {
      scenes = new[] { "Assets/scene.unity" },
      locationPathName = "../ZenMirrorForXREAL.apk",
      options = BuildOptions.None,
      target = BuildTarget.Android
    });
    if (report == null) { return; }

    if (silent) { return; }
    var log = new StringBuilder();
    var summary = report.summary; // shortening
    log.AppendLine(">>> Build finished");
    log.AppendLine($"time   : {summary.totalTime.Seconds} sec");
    log.AppendLine($"result : {summary.result}");
    log.AppendLine($"error  : {summary.totalErrors} error(s), {summary.totalWarnings} warn(s)");
    if (summary.result == BuildResult.Succeeded)
    {
      log.AppendLine($"size   : about {(int)(summary.totalSize / 1024 / 1024)} MiB");
      log.AppendLine($"output : {summary.outputPath}");
      Debug.Log(log);
    }
    else
    {
      Debug.LogError(log);
    }
  }
  public static void Build()
  {
    ProcessBuild(false);
  }
  public static void SilentBuild()
  {
    ProcessBuild(true);
  }
}
