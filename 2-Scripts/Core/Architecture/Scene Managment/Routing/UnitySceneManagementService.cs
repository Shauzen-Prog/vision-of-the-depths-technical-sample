using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Implementación real que envuelve UnityEngine.SceneManagement.SceneManager
/// </summary>
public sealed class UnitySceneManagementService : ISceneManagementService
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public ISceneLoadOperation LoadSceneAsync(string sceneName, LoadSceneMode mode)
    {
        var op = SceneManager.LoadSceneAsync(sceneName, mode);
        if (op == null)
            throw new InvalidOperationException($"No se pudo crear AsyncOperation para '{sceneName}'.");

        return new UnitySceneLoadOperation(op);
    }

    public ISceneLoadOperation UnloadSceneAsync(string sceneName)
    {
        var op = SceneManager.UnloadSceneAsync(sceneName);
        if (op == null)
            return null;

        return new UnitySceneLoadOperation(op);
    }

    private sealed class UnitySceneLoadOperation : ISceneLoadOperation
    {
        private readonly AsyncOperation _op;

        public UnitySceneLoadOperation(AsyncOperation op)
        {
            _op = op;
        }

        public float Progress => _op.progress;
        public bool IsDone => _op.isDone;

        public bool AllowSceneActivation
        {
            get => _op.allowSceneActivation;
            set => _op.allowSceneActivation = value;
        }
    }
}
