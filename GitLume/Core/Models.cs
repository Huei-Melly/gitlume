using System.Collections.Generic;

namespace GitLume.Core;

/// <summary>本地仓库的 Git 状态。</summary>
public enum RepoStatus
{
    /// <summary>未初始化（还不是 Git 仓库）。</summary>
    NotGitRepo,

    /// <summary>已初始化，但还没有任何提交（首次使用）。</summary>
    EmptyRepo,

    /// <summary>已有提交历史。</summary>
    HasCommits,

    /// <summary>无法判定（路径无效等）。</summary>
    Unknown,
}

/// <summary>一个远程仓库（名称 + URL）。</summary>
public sealed class RemoteInfo
{
    public string Name { get; set; } = "origin";
    public string Url { get; set; } = "";

    public override string ToString() => $"{Name}  {Url}";
}

/// <summary>持久化的应用程序配置。</summary>
public sealed class GitSettings
{
    /// <summary>全局 Git 用户名。</summary>
    public string UserName { get; set; } = "";

    /// <summary>全局 Git 邮箱。</summary>
    public string UserEmail { get; set; } = "";

    /// <summary>记住的用户名（明文，旧版单凭据字段，仅用于迁移）。</summary>
    public string CredentialUsername { get; set; } = "";

    /// <summary>记住的密码（简单加密后的密文，旧版单凭据字段，仅用于迁移）。</summary>
    public string CredentialPassword { get; set; } = "";

    /// <summary>按远程仓库 URL 保存的凭据（每个仓库可不同账号密码；密码为 SecureCodec 加密后的密文）。</summary>
    public Dictionary<string, StoredCredential> CredentialsByUrl { get; set; } = new();

    /// <summary>远程仓库列表，支持多仓库推送。</summary>
    public List<RemoteInfo> Remotes { get; set; } = new();

    /// <summary>是否启用 HTTP 代理（用于 GitHub 等需要代理访问的平台）。</summary>
    public bool ProxyEnabled { get; set; }

    /// <summary>HTTP 代理端口（默认 7897，仅 ProxyEnabled 为 true 时生效）。</summary>
    public int ProxyPort { get; set; } = 7897;
}

/// <summary>按远程 URL 持久化的凭据。</summary>
public sealed class StoredCredential
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

/// <summary>用户在登录对话框里提交的凭据（本次会话用，密码明文仅存在于内存）。</summary>
public sealed class CredentialEntry
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public bool Remember { get; set; }
}

/// <summary>本次会话内的凭据（仅存在于内存，不会保存到磁盘）。</summary>
public sealed class GitSessionCredentials
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";

    public bool HasCredentials => !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password);
}

/// <summary>一次 Git 命令的执行结果。</summary>
public sealed class GitResult
{
    public int ExitCode { get; set; }
    public string Output { get; set; } = "";
    public bool Success => ExitCode == 0;
    public bool AuthFailed { get; set; }
}
