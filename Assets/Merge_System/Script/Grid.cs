using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class Grid : MonoBehaviour
{
    public int rows = 10;                    // �� �� (Y ��)
    public int columns = 7;                  // �� �� (X ��)
    public float margin = 0.1f;              // ȭ�� ���� ���� ������ ���� ����
    public int gridnum = 5;                  // ���� ������Ʈ ��;
    public GameObject objectPrefab;          // ���� �ȿ� ���� ������Ʈ ������

    private GameObject[,] gridArray;         // ���� ������ �����ϴ� 2���� �迭
    private Vector3[,] cellCenters;          // �� ���� �߾� ��ǥ�� �����ϴ� �迭
    public float cellSize;                  // �� ���� ũ��
    public Vector3 startPosition;           // �׸��� ���� ��ġ

    private void Start()
    {
        gridArray = new GameObject[rows, columns];
        cellCenters = new Vector3[rows, columns];

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
        //���� �߾� ��ġ �迭�� ����
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 cellPosition = startPosition + new Vector3(col * cellSize + cellSize / 2, -row * cellSize - cellSize / 2, 0);
                cellCenters[row, col] = cellPosition;

                //Debug.Log(cellCenters[row, col]);
            }
        }
        // gridnum ��ŭ ���� ����
        for (int i = 0; i < gridnum; i++)
        {
            if (objectPrefab != null)
            {
                int row = Random.Range(0, rows);
                int col = Random.Range(0, columns);
                GameObject obj = Instantiate(objectPrefab, cellCenters[row, col], Quaternion.identity);
                obj.transform.localScale = new Vector3(cellSize, cellSize, 1);
                obj.transform.SetParent(transform);

                // ������Ʈ�� Grid ������ ����
                Item item = obj.AddComponent<Item>();
                item.Initialize(this, row, col, cellCenters[row, col], item.itemcode);

                gridArray[row, col] = obj;

            }
        }
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
    // ���� ����� �� ã��
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

    // Gizmos�� ����� �׸��带 �ð������� �׸���
    private void OnDrawGizmos()
    {
        if (Camera.main == null) return;

        // ȭ�� ũ�⿡ ���� cellSize ��� (ȭ���� ���� �������� ���)
        float screenWidth = Camera.main.aspect * Camera.main.orthographicSize * 2; // ���� ī�޶��� �ʺ�
        float screenHeight = Camera.main.orthographicSize * 2; // ���� ī�޶��� ����

        // ȭ���� ���� ���̸� ����Ͽ� cellSize ����
        cellSize = Mathf.Min(screenWidth / (columns + (columns + 1) * margin), screenHeight / (rows + (rows + 1) * margin));

        // ������ ���� ��ġ
        startPosition = new Vector3(-columns / 2f * cellSize, rows / 2.7f * cellSize, 0);

        // �׸��� �� ���� ����
        Gizmos.color = Color.green;

        // ���� �� �׸��� (��)
        for (int row = 0; row <= rows; row++)
        {
            Vector3 startLine = startPosition + new Vector3(0, -row * cellSize, 0);
            Vector3 endLine = startLine + new Vector3(columns * cellSize, 0, 0);
            Gizmos.DrawLine(startLine, endLine);
        }

        // ���� �� �׸��� (��)
        for (int col = 0; col <= columns; col++)
        {
            Vector3 startLine = startPosition + new Vector3(col * cellSize, 0, 0);
            Vector3 endLine = startLine + new Vector3(0, -rows * cellSize, 0);
            Gizmos.DrawLine(startLine, endLine);
        }
    }
}
