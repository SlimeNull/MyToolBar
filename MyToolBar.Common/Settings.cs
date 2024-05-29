﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyToolBar.Common;
/*
 *统一的缓存和配置服务
 */
public class SettingsMgr<T> where T : class
{
    public string Sign { get; set; }
    public string PackageName { get; set; }
    public T Data { get; set; }
    [JsonIgnore]
    private FileSystemWatcher _watcher;
    public event Action<T> OnDataChanged;
    public SettingsMgr() { }
    public SettingsMgr(string Sign, string pkgName)
    {
        this.Sign = Sign;
        this.PackageName = pkgName;
        _watcher = new FileSystemWatcher(Settings.SettingsPath)
        {
            Filter = Sign + ".json",
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };
        _watcher.Changed += _watcher_Changed;
    }
    ~SettingsMgr()
    {
        _watcher?.Dispose();
    }
    public async Task Load()
    {
        Debug.WriteLine(Sign + "   Load!!");
        var dt = await Settings.Load<SettingsMgr<T>>(Sign,Settings.sType.Settings);
        if (dt != null)
            Data = dt.Data;
        else
        {
            Data = Activator.CreateInstance<T>();
            await Save();
        }
    }
    public async Task Save()
    {
        Debug.WriteLine(Sign + "   SAVE!!");
        _watcher.EnableRaisingEvents = false;
        await Settings.Save(this, Sign, Settings.sType.Settings);
        _watcher.EnableRaisingEvents = true;
    }
    private DateTime _lastUpdateTime = DateTime.MinValue;
    private void _watcher_Changed(object sender, FileSystemEventArgs e)
    {
        if (DateTime.Now - _lastUpdateTime > TimeSpan.FromSeconds(1))
        {
            _lastUpdateTime = DateTime.Now;
            Debug.WriteLine(Sign + "   changed!!");
            OnDataChanged?.Invoke(Data);
        }
        else
        {
            Debug.WriteLine(Sign + "   Change canceled!!");
        }
    }
}
public static class Settings
{
    public enum sType { Cache, Settings }
    public static string MainPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyToolBar");
    public static string CachePath =>
        Path.Combine(MainPath, "Cache");
    public static string SettingsPath =>
        Path.Combine(MainPath, "Settings");
    public static void LoadPath()
    {
        if (!Directory.Exists(MainPath))
            Directory.CreateDirectory(MainPath);
        if (!Directory.Exists(CachePath))
            Directory.CreateDirectory(CachePath);
        if (!Directory.Exists(SettingsPath))
            Directory.CreateDirectory(SettingsPath);
    }
    public static string GetPathBySign(string Sign, sType type) => Path.Combine(type switch
    {
        sType.Cache => CachePath,
        sType.Settings => SettingsPath
    }, Sign + ".json");
    public static async Task Save<T>(T Data, string Sign, sType type) where T : class
    {
        string path = GetPathBySign(Sign,type);
        var fs = File.Create(path);
        await JsonSerializer.SerializeAsync<T>(fs, Data, new JsonSerializerOptions { WriteIndented = true });
        fs.Close();
    }
    public static async Task<T?> Load<T>(string Sign, sType t) where T : class
    {
        try
        {
            string path = GetPathBySign(Sign,t);
            if (!File.Exists(path))
                return null;
            var fs = File.OpenRead(path);
            var data = await JsonSerializer.DeserializeAsync<T>(fs);
            fs.Close();
            return data;
        }
        catch
        {
            return null;
        }
    }
}
