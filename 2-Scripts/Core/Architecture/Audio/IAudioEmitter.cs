using UnityEngine;

public interface IAudioEmitter 
{
    // Método que invocará el sonido configurado
    void EmitSound(Sound soundId, Transform soundTransform);
}
