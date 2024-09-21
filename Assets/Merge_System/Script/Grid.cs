using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class Grid : MonoBehaviour
{
    #region 변수
    public int rows = 10;                    // 행 수 (Y 축)
    public int columns = 7;                  // 열 수 (X 축)
    private float margin = 0.1f;              // 화면 경계와 격자 사이의 여백 비율
    private int gridnum = 5;                  // 만들 오브젝트 수
    public GameObject objectPrefab;          // 격자 안에 넣을 오브젝트 프리팹
    #endregion

    private GameObject[,] gridArray;         // 격자 셀들을 저장하는 2차원 배열
    private Vector3[,] cellCenters;          // 각 셀의 중앙 좌표를 저장하는 배열
    public bool[,] isExists;                // 셀의 상태
    public float cellSize;                   // 각 셀의 크기
    public Vector3 startPosition;            // 그리드 시작 위치

    private void Start()
    { 
        gridArray = new GameObject[rows, columns];
        cellCenters = new Vector3[rows, columns];
        isExists = new bool[rows, columns];
        UpdateGrid();
    }

    void UpdateGrid()
    {
        float screenWidth = Camera.main.aspect * Camera.main.orthographicSize * 2;
        float screenHeight = Camera.main.orthographicSize * 2;

        cellSize = Mathf.Min(screenWidth / (columns + (columns + 1) * margin), screenHeight / (rows + (rows + 1) * margin));

        startPosition = new Vector3(-columns / 2f * cellSize, rows / 2.7f * cellSize, 0);

        GenerateGridWithObjects(startPosition);
    }

    void GenerateGridWithObjects(Vector3 startPosition)
    {
        //격자 중앙 위치 배열에 저장
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 cellPosition = startPosition + new Vector3(col * cellSize + cellSize / 2, -row * cellSize - cellSize / 2, 0);
                cellCenters[row, col] = cellPosition;
            }
        }
        isExists[0, 0] = true;
        // gridnum 만큼 옵젝 생성
        for (int i = 0; i < gridnum; i++)
        {
            if (objectPrefab != null)
            {
                int row = Random.Range(0, rows);
                int col = Random.Range(0, columns);
                if(IsCellEmpty(row, col) && !isExists[row, col])
                {
                    GenerateObject(row, col, objectPrefab);
                }
            }
        }
    }

    public void GenerateObject(int row, int col, GameObject obj)
    {
        obj = Instantiate(objectPrefab, cellCenters[row, col], Quaternion.identity);
        obj.transform.localScale = new Vector3(cellSize, cellSize, 1);
        obj.transform.SetParent(transform);

        Item item = obj.AddComponent<Item>();
        item.Initialize(this, row, col, cellCenters[row, col], item.itemcode);

        gridArray[row, col] = obj;
    }

    public Vector3 GetCellCenter(int row, int col)
    {
        return cellCenters[row, col];
    }

    public bool IsCellEmpty(int row, int col)
    {
        return gridArray[row, col] == null;
    }

    public void PlaceObjectAt(int row, int col, GameObject obj)
    {
        gridArray[row, col] = obj;
    }

    public void ClearCell(int row, int col)
    {
        gridArray[row, col] = null;
    }

    public Item GetObjectAt(int row, int col)
    {
        if (gridArray[row, col] != null)
        {
            return gridArray[row, col].GetComponent<Item>();
        }
        return null;
    }
    // 가장 가까운 셀 찾기
    public Vector2Int FindClosestCell(Vector3 position)
    {
        float minDistance = float.MaxValue;
        Vector2Int closestCell = new Vector2Int(-1, -1);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                float distance = Vector3.Distance(position, cellCenters[row, col]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCell = new Vector2Int(row, col);
                }
            }
        }
        return closestCell;
    }

    //bfs 가까운 셀 찾기
    public Vector2Int FindClosestEmptyCell(Vector3 selectedPosition, int selectedRow, int selectedCol)
    {
        // BFS 탐색을 위한 큐 생성
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        // 이미 방문한 셀을 기록할 배열 생성
        bool[,] visited = new bool[rows, columns];

        // 시작 셀을 큐에 추가하고 방문 처리
        queue.Enqueue(new Vector2Int(selectedRow, selectedCol));
        visited[selectedRow, selectedCol] = true;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (isExists[row, col])
                {
                    visited[row, col] = true;
                }
            }
        }
        // 네 방향을 탐색하기 위한 오프셋 배열 (상, 하, 좌, 우)
        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(1, 0),   // 아래
        new Vector2Int(-1, 0),  // 위
        new Vector2Int(0, 1),   // 오른쪽
        new Vector2Int(0, -1)   // 왼쪽
        };

        // BFS 탐색
        while (queue.Count > 0)
        {
            // 큐에서 현재 셀을 꺼냄
            Vector2Int currentCell = queue.Dequeue();
            int currentRow = currentCell.x;
            int currentCol = currentCell.y;

            // 현재 셀이 빈 셀인지 확인 (선택한 셀은 제외)
            if (currentRow != selectedRow || currentCol != selectedCol)
            {
                if (IsCellEmpty(currentRow, currentCol))
                {
                    return currentCell; // 빈 셀을 찾았을 경우 반환
                }
            }

            // 네 방향 탐색
            foreach (var direction in directions)
            {
                int newRow = currentRow + direction.x;
                int newCol = currentCol + direction.y;

                // 그리드 범위 내에 있는지 확인하고, 방문하지 않은 셀인 경우
                if (newRow >= 0 && newRow < rows && newCol >= 0 && newCol < columns && !visited[newRow, newCol])
                {
                    queue.Enqueue(new Vector2Int(newRow, newCol));  // 새로운 셀을 큐에 추가
                    visited[newRow, newCol] = true;  // 방문 처리
                }
            }
        }
        // 모든 셀을 탐색했으나 빈 셀이 없는 경우 (-1, -1)을 반환
        return new Vector2Int(selectedRow, selectedCol);
    }

    // Gizmos를 사용해 그리드를 시각적으로 그리기
    private void OnDrawGizmos()
    {
        if (Camera.main == null) return;

        // 화면 크기에 따라 cellSize 계산 (화면의 폭을 기준으로 계산)
        float screenWidth = Camera.main.aspect * Camera.main.orthographicSize * 2; // 현재 카메라의 너비
        float screenHeight = Camera.main.orthographicSize * 2; // 현재 카메라의 높이

        // 화면의 폭과 높이를 고려하여 cellSize 결정
        cellSize = Mathf.Min(screenWidth / (columns + (columns + 1) * margin), screenHeight / (rows + (rows + 1) * margin));

        // 격자의 시작 위치
        startPosition = new Vector3(-columns / 2f * cellSize, rows / 2.7f * cellSize, 0);

        // 그리드 선 색상 설정
        Gizmos.color = Color.green;

        // 가로 선 그리기 (행)
        for (int row = 0; row <= rows; row++)
        {
            Vector3 startLine = startPosition + new Vector3(0, -row * cellSize, 0);
            Vector3 endLine = startLine + new Vector3(columns * cellSize, 0, 0);
            Gizmos.DrawLine(startLine, endLine);
        }

        // 세로 선 그리기 (열)
        for (int col = 0; col <= columns; col++)
        {
            Vector3 startLine = startPosition + new Vector3(col * cellSize, 0, 0);
            Vector3 endLine = startLine + new Vector3(0, -rows * cellSize, 0);
            Gizmos.DrawLine(startLine, endLine);
        }
    }
}
