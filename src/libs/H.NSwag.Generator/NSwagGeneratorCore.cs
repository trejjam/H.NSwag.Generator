﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using H.NSwag.Generator.Core.Extensions;

namespace H.NSwag.Generator
{
    public static class NSwagGeneratorCore
    {
        public static string Generate(string consolePath, string nswagPath)
        {
            consolePath = consolePath ?? throw new ArgumentNullException(nameof(consolePath));
            nswagPath = nswagPath ?? throw new ArgumentNullException(nameof(nswagPath));

            var nswagTempPath = $"{Path.GetTempFileName()}.nswag";
            var outputPath = $"{Path.GetTempFileName()}.cs".Replace('\\', '/');

            var output = string.Empty;
            try
            {
                File.Copy(nswagPath, nswagTempPath, true);

                var nswagContents = File.ReadAllText(nswagTempPath);

                nswagContents = nswagContents.Replace("\"output\": null,", "\"output\": \"\",");
                var outputIndex = nswagContents.ExtractAllIndexes("\"output\": \"", "\"").Last();
                nswagContents = nswagContents
                    .Remove(outputIndex.Start, outputIndex.Length)
                    .Insert(outputIndex.Start, outputPath);

                File.WriteAllText(nswagTempPath, nswagContents);

                if (!consolePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "The path to the console application must contain the path to the .exe file.");
                }

                using var process = Process.Start(new ProcessStartInfo(
                    Environment.ExpandEnvironmentVariables(consolePath),
                    $"run \"{nswagTempPath}\"")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                });

                process?.WaitForExit();

                output = process?.StandardOutput.ReadToEnd();

                return File.ReadAllText(outputPath);
            }
            catch (FileNotFoundException)
            {
                throw new InvalidOperationException($"NSwag console error: {output}");
            }
            finally
            {
                File.Delete(outputPath);
                File.Delete(nswagTempPath);
            }
        }
    }
}