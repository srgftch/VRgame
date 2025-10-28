using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRats : MonoBehaviour
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private Transform _pos;
    [SerializeField] private float _spawnInterval = 120f; // 2 минуты в секундах

    private void Start()
    {
        // Запускаем повторяющийся вызов спавна
        InvokeRepeating(nameof(Spawn), _spawnInterval, _spawnInterval);

    }

    public void Spawn()
    {
        Instantiate(_prefab, _pos.position, _pos.rotation);
    }

    // Альтернативный способ с корутиной
    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_spawnInterval);
            Spawn();
        }
    }
}