using UnityEngine;
using System;
using System.IO;
using Zenject;

/// <summary>
/// Implementación de repositorio que persiste las opciones de display
/// en un archivo JSON dentro del sistema de archivos.
/// 
/// Responsabilidad:
/// - Cargar y guardar configuraciones de display
/// - Validar y migrar datos persistidos
/// - Asegurar escritura atómica para evitar corrupción
/// 
/// Esta clase pertenece a la capa Infrastructure.
/// </summary>
public sealed class JsonDisplaySettingsRepository : IDisplaySettingsRepository
{
    private readonly string _path;

    /// <summary>
    /// Crea el repositorio usando un path absoluto inyectado.
    /// El path se inyecta para facilitar testing y desacoplar
    /// del entorno concreto (Application.persistentDataPath).
    /// </summary>
    public JsonDisplaySettingsRepository([Inject(Id = "DisplaySettingsPath")] string absolutePath)
    {
       _path = absolutePath ?? throw new ArgumentNullException(nameof(absolutePath));
    }
    
    
    public bool Exists() => File.Exists(_path);
    
    public bool TryLoad(out DisplaySettingsDTO dto)
    {
        dto = null;
        try
        {
            if(!File.Exists(_path)) return false;
            string json = File.ReadAllText(_path);
            DisplaySettingsDTO data = JsonUtility.FromJson<DisplaySettingsDTO>(json);
            if(data == null) return false;
            
            // Minimal validation + migration hook
            data.gamma = Mathf.Clamp(data.gamma, 0.5f, 2.5f);
            switch (data.schemaVersion)
            {
                case 1: break;
                default: break;
            }
            
            dto = data;
            return true;
        }
        catch (Exception e)
        {
            // En un entorno productivo esto podría reportarse
            // a un sistema de logging centralizado.
            Debug.LogWarning($"[JSON DISPLAY] Failed to load settings: {e.Message}");
            return false;
        }
    }

    public void Save(DisplaySettingsDTO dto)
    {
        try
        {
            var json = JsonUtility.ToJson(data, prettyPrint: true);

            // Escritura atómica:
            // se escribe primero un archivo temporal y luego se reemplaza.
            var tmpPath = _absolutePath + ".tmp";

            File.WriteAllText(tmpPath, json);
            File.Replace(tmpPath, _absolutePath, null);
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"[DisplaySettings] Error al guardar settings: {e}");
        }
    }

    /// <summary>
    /// Valida los valores cargados y aplica migraciones
    /// según la versión del esquema.
    /// </summary>
    private static void ValidateAndMigrate(ref DisplaySettingsData data)
    {
        // Clamp defensivo por si el archivo fue editado a mano
        // o proviene de una versión vieja.
        data.gamma = Mathf.Clamp(data.gamma, 0.5f, 2.5f);

        switch (data.schemaVersion)
        {
            case 1:
                // Versión inicial, no requiere migración.
                break;

            default:
                // Si aparece una versión desconocida,
                // se asume compatibilidad hacia atrás.
                break;
        }
    }
}
