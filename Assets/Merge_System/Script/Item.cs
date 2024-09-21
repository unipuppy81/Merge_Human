using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    private Grid grid;                      // �׸��� ����
    private int currentRow, currentCol;      // ���� ��ġ�� �� ��ǥ
    private Vector3 originalPosition;        // �巡�� ���� �� ���� ��ġ

    public int itemcode = 1;
    public bool isGenerate = false;

    // GridObject �ʱ�ȭ
    public void Initialize(Grid grid, int row, int col, Vector3 startPosition, int icode)
    {
        this.grid = grid;
        this.currentRow = row;
        this.currentCol = col;
        this.originalPosition = startPosition;
        this.itemcode = icode;
    }

    // �巡�� ����
    private void OnMouseDown()
    {
        originalPosition = transform.position; // ���� ��ġ ����
        grid.ClearCell(currentRow, currentCol); // ���� �� ����
    }

    // ������Ʈ�� �巡���ϴ� ��
    private void OnMouseDrag()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Z ��ǥ ����
        transform.position = mousePosition;
        
    }

    // �巡�� ����
    private void OnMouseUp()
    {
        Vector2Int closestCell = grid.FindClosestCell(transform.position);

        Vector2Int closestEmptyCell = grid.FindClosestEmptyCell(transform.position, currentRow, currentCol);

        // ����� ���� ��������� �������� �̵�, �ƴϸ� ���� ��ġ�� ����
        if (transform.position.x >= grid.startPosition.x && transform.position.x <= grid.startPosition.x + grid.columns * grid.cellSize &&
            transform.position.y <= grid.startPosition.y && transform.position.y >= grid.startPosition.y - grid.rows * grid.cellSize)
        {
            Item otherObject = grid.GetObjectAt(closestCell.x, closestCell.y);

            // ��ĥ �� �ִ��� Ȯ���ϰ� ��ġ��
            if (otherObject != null && CanMergeWith(otherObject))
            {
                MergeWith(otherObject);
            }
            else if (grid.IsCellEmpty(closestCell.x, closestCell.y) && !grid.isExists[closestCell.x, closestCell.y])
            {
                transform.position = grid.GetCellCenter(closestCell.x, closestCell.y); // ����� ���� �̵�
                grid.PlaceObjectAt(closestCell.x, closestCell.y, gameObject); // �� ä���
                currentRow = closestCell.x; // �� ��ǥ ������Ʈ
                currentCol = closestCell.y;
            }
            else
            {
                transform.position = originalPosition; // ���� ��ġ�� ����
                grid.PlaceObjectAt(currentRow, currentCol, gameObject); // �ٽ� ���� ����
            }

            if (isGenerate && originalPosition == grid.GetCellCenter(closestCell.x, closestCell.y))
            {
                grid.GenerateObject(closestEmptyCell.x, closestEmptyCell.y, gameObject); // �� ��ġ�� �� ������Ʈ ����
                Debug.Log(closestEmptyCell);
                return; // ������Ʈ ���� �� ����
            }
            else
            {
                Debug.Log("����");
            }
        }
        else
        {
            transform.position = originalPosition; // �� ���̸� ���� ��ġ�� ����
            grid.PlaceObjectAt(currentRow, currentCol, gameObject); // �ٽ� ���� ����
        }
    }
    private bool CanMergeWith(Item otherObject)
    {
        return otherObject != null && otherObject.itemcode == this.itemcode; // ������ ���� ������Ʈ�� ��ĥ �� ����
    }


    // �ٸ� ������Ʈ�� ��ġ��
    private void MergeWith(Item otherObject)
    {
        // ������ ��ġ�� �ٸ� ������Ʈ�� ��ġ��
        transform.position = otherObject.transform.position;

        // ���ο� ������Ʈ�� ���� (���� 1 ���)
        int newLevel = itemcode + 1;
        GameObject mergedObject = Instantiate(gameObject, transform.position, Quaternion.identity);
        Item mergedGridObject = mergedObject.GetComponent<Item>();
        mergedGridObject.Initialize(grid, otherObject.currentRow, otherObject.currentCol, transform.position, newLevel);

        // �׸��忡 ���ο� ������Ʈ ��ġ
        grid.PlaceObjectAt(otherObject.currentRow, otherObject.currentCol, mergedObject);
        
        // ���� ������Ʈ ����
        Destroy(otherObject.gameObject); // �ٸ� ������Ʈ ����
        Destroy(gameObject); // ���� ������Ʈ ����
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
