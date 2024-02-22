﻿using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DrawnUi.Maui.Infrastructure;

public static class SkSl
{

    public static async Task<string> LoadFromResourcesAsync(string fileName)
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        return json;
    }

    public static string LoadFromResources(string fileName)
    {
        using var stream = FileSystem.OpenAppPackageFileAsync(fileName).Result;
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        return json;
    }

    public static SKRuntimeEffect Compile(string shaderCode)
    {
        string errors;
        var effect = SKRuntimeEffect.CreateShader(shaderCode, out errors);
        if (!string.IsNullOrEmpty(errors))
        {
            ThrowCompilationError(shaderCode, errors);
        }
        Debug.WriteLine($"[SKSL] Compiled shader code!");
        return effect;
    }

    static void ThrowCompilationError(string shaderCode, string errors)
    {
        // Regular expression to find the line number in the error message
        var regex = new Regex(@"error: (\d+):");
        var match = regex.Match(errors);

        var error = string.Empty;
        if (match.Success && int.TryParse(match.Groups[1].Value, out int lineNumber))
        {
            var lines = shaderCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            if (lineNumber - 1 < lines.Length)
            {
                error = ($"Error occurred at line {lineNumber}: {lines[lineNumber - 1]}");
            }
            else
            {
                error = ($"Error line number {lineNumber} exceeds shader code line count.");
            }
        }
        else
        {
            error = $"Failed to find error line number.";
        }
        if (!string.IsNullOrEmpty(error))
        {
            throw new ApplicationException($"Shader compilation failed:{Environment.NewLine}{errors}{Environment.NewLine}" + error);
        }
    }

    public static SKShader CreateShader(SKRuntimeEffect compiled, SKRuntimeEffectUniforms uniforms, Dictionary<string, SKShader> textures)
    {

        var children = new SKRuntimeEffectChildren(compiled);
        foreach (var texture in textures)
        {
            children.Add(texture.Key, texture.Value);
        }

        return compiled.ToShader(uniforms, children); ;
    }
}