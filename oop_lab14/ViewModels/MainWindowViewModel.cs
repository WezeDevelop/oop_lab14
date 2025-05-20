using System;
using System.IO;
using System.IO.Compression;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace oop_lab14.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private string textContent;
    private string? currentTextFile;

    [RelayCommand] private async void CreateFile()
    {
        var dialog = new SaveFileDialog();
        var result = await dialog.ShowAsync(GetMainWindow());
        if (result != null)
            File.Create(result).Close();
    }

    [RelayCommand] private async void CreateFolder()
    {
        var dialog = new OpenFolderDialog();
        var folder = await dialog.ShowAsync(GetMainWindow());
        if (folder != null)
        {
            var name = await Prompt("Назва каталогу:");
            if (!string.IsNullOrEmpty(name))
                Directory.CreateDirectory(Path.Combine(folder, name));
        }
    }

    [RelayCommand] private async void Copy()
    {
        var file = await GetOpenFilePath();
        var folder = await GetFolderPath();
        if (file != null && folder != null)
            File.Copy(file, Path.Combine(folder, Path.GetFileName(file)), true);
    }

    [RelayCommand] private async void Move()
    {
        var file = await GetOpenFilePath();
        var folder = await GetFolderPath();
        if (file != null && folder != null)
            File.Move(file, Path.Combine(folder, Path.GetFileName(file)), true);
    }

    [RelayCommand] private async void Delete()
    {
        var path = await GetOpenFilePath();
        if (path != null)
        {
            if (Directory.Exists(path)) Directory.Delete(path, true);
            else File.Delete(path);
        }
    }

    [RelayCommand] private async void ToggleReadonly()
    {
        var path = await GetOpenFilePath();
        if (path != null)
        {
            var attr = File.GetAttributes(path);
            if (attr.HasFlag(FileAttributes.ReadOnly))
                File.SetAttributes(path, attr & ~FileAttributes.ReadOnly);
            else
                File.SetAttributes(path, attr | FileAttributes.ReadOnly);
        }
    }

    [RelayCommand] private async void OpenText()
    {
        var file = await GetOpenFilePath();
        if (file != null)
        {
            TextContent = File.ReadAllText(file);
            currentTextFile = file;
        }
    }

    [RelayCommand] private void SaveText()
    {
        if (currentTextFile != null)
            File.WriteAllText(currentTextFile, TextContent);
    }

    [RelayCommand] private async void Zip()
    {
        var file = await GetOpenFilePath();
        var save = new SaveFileDialog { DefaultExtension = "zip" };
        var zipPath = await save.ShowAsync(GetMainWindow());
        if (file != null && zipPath != null)
        {
            using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
            archive.CreateEntryFromFile(file, Path.GetFileName(file));
        }
    }

    [RelayCommand] private async void Unzip()
    {
        var zip = await GetOpenFilePath("zip");
        var folder = await GetFolderPath();
        if (zip != null && folder != null)
            ZipFile.ExtractToDirectory(zip, folder);
    }

    private async Task<string?> GetOpenFilePath(string? extension = null)
    {
        var dialog = new OpenFileDialog { AllowMultiple = false };
        if (extension != null)
            dialog.Filters.Add(new FileDialogFilter { Name = extension.ToUpper(), Extensions = { extension } });
        var result = await dialog.ShowAsync(GetMainWindow());
        return result?.Length > 0 ? result[0] : null;
    }

    private async Task<string?> GetFolderPath()
    {
        var dialog = new OpenFolderDialog();
        return await dialog.ShowAsync(GetMainWindow());
    }

    private Window GetMainWindow() =>
        Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime life
            ? life.MainWindow! : null!;

    private async Task<string?> Prompt(string message)
    {
        // Можна реалізувати кастомне вікно замість InputBox
        return await Task.Run(() => Microsoft.VisualBasic.Interaction.InputBox(message, "Введення", ""));
    }
}
