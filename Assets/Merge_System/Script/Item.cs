using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    private Grid grid;                      // 그리드 참조
    private int currentRow, currentCol;      // 현재 위치한 셀 좌표
    private Vector3 originalPosition;        // 드래그 시작 시 원래 위치

    public int itemcode = 1;
    public bool isGenerate = false;

    // GridObject 초기화
    public void Initialize(Grid grid, int row, int col, Vector3 startPosition, int icode)
    {
        this.grid = grid;
        this.currentRow = row;
        this.currentCol = col;
        this.originalPosition = startPosition;
        this.itemcode = icode;
    }

    // 드래그 시작
    private void OnMouseDown()
    {
        originalPosition = transform.position; // 원래 위치 저장
        grid.ClearCell(currentRow, currentCol); // 현재 셀 비우기
    }

    // 오브젝트를 드래그하는 중
    private void OnMouseDrag()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Z 좌표 고정
        transform.position = mousePosition;
        
    }

    // 드래그 종료
    private void OnMouseUp()
    {
        Vector2Int closestCell = grid.FindClosestCell(transform.position);

        Vector2Int closestEmptyCell = grid.FindClosestEmptyCell(transform.position, currentRow, currentCol);

        // 가까운 셀이 비어있으면 그쪽으로 이동, 아니면 원래 위치로 복귀
        if (transform.position.x >= grid.startPosition.x && transform.position.x <= grid.startPosition.x + grid.columns * grid.cellSize &&
            transform.position.y <= grid.startPosition.y && transform.position.y >= grid.startPosition.y - grid.rows * grid.cellSize)
        {
            Item otherObject = grid.GetObjectAt(closestCell.x, closestCell.y);

            // 합칠 수 있는지 확인하고 합치기
            if (otherObject != null && CanMergeWith(otherObject))
            {
                MergeWith(otherObject);
            }
            else if (grid.IsCellEmpty(closestCell.x, closestCell.y) && !grid.isExists[closestCell.x, closestCell.y])
            {
                transform.position = grid.GetCellCenter(closestCell.x, closestCell.y); // 가까운 셀로 이동
                grid.PlaceObjectAt(closestCell.x, closestCell.y, gameObject); // 셀 채우기
                currentRow = closestCell.x; // 새 좌표 업데이트
                currentCol = closestCell.y;
            }
            else
            {
                transform.position = originalPosition; // 원래 위치로 복귀
                grid.PlaceObjectAt(currentRow, currentCol, gameObject); // 다시 원래 셀로
            }

            if (isGenerate && originalPosition == grid.GetCellCenter(closestCell.x, closestCell.y))
            {
                grid.GenerateObject(closestEmptyCell.x, closestEmptyCell.y, gameObject); // 그 위치에 새 오브젝트 생성
                Debug.Log(closestEmptyCell);
                return; // 오브젝트 생성 후 종료
            }
            else
            {
                Debug.Log("오류");
            }
        }
        else
        {
            transform.position = originalPosition; // 셀 밖이면 원래 위치로 복귀
            grid.PlaceObjectAt(currentRow, currentCol, gameObject); // 다시 원래 셀로
        }
    }
    private bool CanMergeWith(Item otherObject)
    {
        return otherObject != null && otherObject.itemcode == this.itemcode; // 레벨이 같은 오브젝트만 합칠 수 있음
    }


    // 다른 오브젝트와 합치기
    private void MergeWith(Item otherObject)
    {
        // 합쳐질 위치는 다른 오브젝트의 위치로
        transform.position = otherObject.transform.position;

        // 새로운 오브젝트를 생성 (레벨 1 상승)
        int newLevel = itemcode + 1;
        GameObject mergedObject = Instantiate(gameObject, transform.position, Quaternion.identity);
        Item mergedGridObject = mergedObject.GetComponent<Item>();
        mergedGridObject.Initialize(grid, otherObject.currentRow, otherObject.currentCol, transform.position, newLevel);

        // 그리드에 새로운 오브젝트 배치
        grid.PlaceObjectAt(otherObject.currentRow, otherObject.currentCol, mergedObject);
        
        // 기존 오브젝트 삭제
        Destroy(otherObject.gameObject); // 다른 오브젝트 삭제
        Destroy(gameObject); // 현재 오브젝트 삭제
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
