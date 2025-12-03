using UnityEngine;

namespace Utilities
{
    public class InfiniteBackground : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private GameObject backgroundTilePrefab;
        [SerializeField] private int gridSize = 5; // Increased default for better coverage
        [SerializeField] private float repositionThreshold = 0.5f; // Reposition when player moves this fraction of a tile

        private GameObject[,] _tiles;
        private Vector2 _lastPlayerPosition;
        private Vector2 _tileSize;

        private void Start()
        {
            if (!playerTransform)
            {
                playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            }

            CalculateTileSize();
            InitializeTiles();
            _lastPlayerPosition = playerTransform.position;
        }

        private void Update()
        {
            Vector2 currentPlayerPosition = playerTransform.position;
            Vector2 movement = currentPlayerPosition - _lastPlayerPosition;

            // Check if player has moved significantly in any direction
            if (Mathf.Abs(movement.x) > _tileSize.x * repositionThreshold || 
                Mathf.Abs(movement.y) > _tileSize.y * repositionThreshold)
            {
                UpdateTilePositions();
                _lastPlayerPosition = currentPlayerPosition;
            }
        }

        private void CalculateTileSize()
        {
            SpriteRenderer spriteRenderer = backgroundTilePrefab.GetComponent<SpriteRenderer>();
            if (spriteRenderer && spriteRenderer.sprite)
            {
                Bounds bounds = spriteRenderer.bounds;
                _tileSize = new Vector2(bounds.size.x, bounds.size.y);
            }
            else
            {
                Debug.LogWarning("Background tile prefab missing SpriteRenderer. Using default size.");
                _tileSize = new Vector2(20f, 20f);
            }
        }

        private void InitializeTiles()
        {
            _tiles = new GameObject[gridSize, gridSize];
            Vector2 centerGridPos = WorldToGridPosition(playerTransform.position);
            int halfGrid = gridSize / 2;

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    Vector2 gridPos = new Vector2(
                        centerGridPos.x + (x - halfGrid),
                        centerGridPos.y + (y - halfGrid)
                    );

                    Vector2 worldPos = GridToWorldPosition(gridPos);
                    _tiles[x, y] = Instantiate(backgroundTilePrefab, worldPos, Quaternion.identity, transform);
                    _tiles[x, y].GetComponent<SpriteRenderer>().sortingOrder = -10;
                }
            }
        }

        private void UpdateTilePositions()
        {
            Vector2 playerGridPos = WorldToGridPosition(playerTransform.position);
            int halfGrid = gridSize / 2;

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    Vector2 currentTileGridPos = WorldToGridPosition(_tiles[x, y].transform.position);
                    
                    // Calculate desired grid position relative to player
                    Vector2 desiredGridPos = new Vector2(
                        playerGridPos.x + (x - halfGrid),
                        playerGridPos.y + (y - halfGrid)
                    );

                    // Check if tile is too far from player
                    float distanceX = Mathf.Abs(currentTileGridPos.x - playerGridPos.x);
                    float distanceY = Mathf.Abs(currentTileGridPos.y - playerGridPos.y);

                    if (distanceX > halfGrid || distanceY > halfGrid)
                    {
                        Vector2 newWorldPos = GridToWorldPosition(desiredGridPos);
                        _tiles[x, y].transform.position = newWorldPos;
                    }
                }
            }
        }

        private Vector2 WorldToGridPosition(Vector2 worldPosition)
        {
            return new Vector2(
                Mathf.Floor(worldPosition.x / _tileSize.x),
                Mathf.Floor(worldPosition.y / _tileSize.y)
            );
        }

        private Vector2 GridToWorldPosition(Vector2 gridPosition)
        {
            return new Vector2(
                gridPosition.x * _tileSize.x + _tileSize.x * 0.5f,
                gridPosition.y * _tileSize.y + _tileSize.y * 0.5f
            );
        }
    }
}