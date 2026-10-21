# SevenDeadlyPatrol - Unity C# Scripts

Contenido:
- Hero.cs
- Item.cs
- Equipment.cs
- Inventory.cs
- PatrolZone.cs
- PatrolManager.cs
- PatrolResult.cs
- GameManager.cs
- SaveSystem.cs

Instrucciones rápidas:
1. Crea una nueva escena en Unity.
2. Añade un GameObject vacío llamado "GameManager" y pega el script GameManager.cs.
3. Crea otro GameObject "PatrolManager" y pega PatrolManager.cs.
4. Crea prefabs simples para héroes con el script Hero.cs.
5. Conecta referencias en el inspector (roster, zones, etc).
6. Llama a PatrolManager.Dispatch(hList, chosenZone) para iniciar una patrulla.

Este conjunto es un punto de partida: necesitarás UI, ScriptableObjects para recipes/items, y pulir la lógica de recompensas, guardado y sincronización.
