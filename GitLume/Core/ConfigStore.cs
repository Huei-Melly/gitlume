using System;
using System.IO;
using System.Text.Json;

namespace GitLume.Core;

/// <summary>
/// 配置持久化：保存到软件所在目录的 config.json（不往系统盘写任何文件），
/// 应用重启后自动恢复上次的用户名、邮箱、仓库列表与目录。
/// </summary>
public static class ConfigStore
{
    private static readonly string Dir = AppContext.BaseDirectory;

    private static readonly string FilePath = Path.Combine(Dir, "config.json");

    public static GitSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<GitSettings>(json) ?? new GitSettings();
            }

            // 迁移：老版本把配置存在 %APPDATA%\GitLume\config.json，首次运行复制到本目录后不再写系统盘
            var legacy = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GitLume", "config.json");
            if (File.Exists(legacy))
            {
                var settings = JsonSerializer.Deserialize<GitSettings>(File.ReadAllText(legacy)) ?? new GitSettings();
                Save(settings);
                return settings;
            }
        }
        catch
        {
            // 配置文件损坏时回退到默认配置，不影响使用
        }
        return new GitSettings();
    }

    public static void Save(GitSettings settings)
    {
        try
        {
            Directory.CreateDirectory(Dir);
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
        catch
        {
            // 保存失败不阻塞操作
        }
    }
}
