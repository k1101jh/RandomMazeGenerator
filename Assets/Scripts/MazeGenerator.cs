// 참고:    https://weblog.jamisbuck.org/2010/12/29/maze-generation-eller-s-algorithm
//          http://www.neocomputer.org/projects/eller.html
//          https://github.com/cocodding0723/MazeAutoGenerator
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public GameObject wallObject;
    public Camera mainCamera;
    public int size = 7;
    public int[,] board;

    private Vector3 wallSize;
    private int nextIdx = 1;
    private Dictionary<int, List<int>> rowBlocks = new Dictionary<int, List<int>>();

    public int[, ] GetBoard() {
        return board;
    }

    public void Start() {
        if(size % 2 == 0) {
            throw new System.Exception("크기가 홀수가 아닙니다!");
        } else if(size <= 2) {
            throw new System.Exception("크기가 너무 작습니다!");
        }
        if(mainCamera != null) {
            mainCamera.GetComponent<Camera>().orthographicSize = (size + 1) / 2;
        }
        wallSize = wallObject.GetComponent<Renderer>().bounds.size;
        GenerateMaze();
    }

    // ----------------- Eller's Algorithm -------------------
    public void GenerateMaze() {
        InitializeBoard();
        // 미로 생성
        Step();

        // 마지막 줄 정리
        for(int i=2; i < size - 2; i += 2) {
            if(board[size - 2, i - 1] == board[size - 2, i + 1]) {
                if(board[size - 2, i - 1] != 0) {
                    board[size - 2, i] = -1;
                }
            } else {
                if(board[size - 2, i - 1] == 0) {
                    board[size - 2, i - 1] = board[size - 2, i] = board[size - 2, i + 1];
                } else if(board[size - 2, i + 1] == 0) {
                    board[size - 2, i + 1] = board[size - 2, i] = board[size - 2, i - 1];
                }
            }
        }

        // 오브젝트 생성
        GenerateObjects();
    }

    private void InitializeBoard() {
        board = new int[size, size];
        for(int i=0; i < size; i++) {
            System.Array.Clear(board, 0, size);
        }

        // 사방을 벽으로 막기
        for(int i=0; i < size; i++) {
            board[0, i] = -1;
            board[size - 1, i] = -1;
            board[i, 0] = -1;
            board[i, size - 1] = -1;
        }
    }

    private void Step() {
        for(int i=1; i < size - 3; i += 2) {
            MergeRow(i);
            MoveBottom(i + 1);
        }
    }

    // 무작위로 좌우 칸을 결합
    private void MergeRow(int rowNum) {
        rowBlocks.Clear();
        int blockNumber;
        for(int i=2; i < size - 1; i += 2) {
            if(board[rowNum, i - 1] != 0 && board[rowNum, i - 1] == board[rowNum, i + 1]) {
                board[rowNum, i] = -1;
            } else if(Random.Range(0, 2) == 1) {
                // 오른쪽과 합치기
                if(board[rowNum, i - 1] != 0 && board[rowNum, i + 1] != 0) {
                    board[rowNum, i] = board[rowNum, i + 1] = board[rowNum, i - 1];
                } else if(board[rowNum, i - 1] != 0) {
                    board[rowNum, i] = board[rowNum, i + 1] = board[rowNum, i - 1];
                } else if(board[rowNum, i + 1] != 0) {
                    board[rowNum, i] = board[rowNum, i - 1] = board[rowNum, i + 1];
                } else {
                    board[rowNum, i] = board[rowNum, i - 1] = board[rowNum, i + 1] = nextIdx++;
                }
            } else {
                board[rowNum, i] = -1;
                if(board[rowNum, i - 1] == 0) {
                    board[rowNum, i - 1] = nextIdx++;
                }
            }

            blockNumber = board[rowNum, i - 1];
            if(!rowBlocks.ContainsKey(blockNumber)) {
                rowBlocks.Add(blockNumber, new List<int>());
            }

            rowBlocks[blockNumber].Add(i - 1);
        }

        if(board[rowNum, size - 2] == 0) {
            board[rowNum, size - 2] = nextIdx++;
        }

        blockNumber = board[rowNum, size - 2];
        if(!rowBlocks.ContainsKey(blockNumber)) {
            rowBlocks.Add(blockNumber, new List<int>());
        }

        rowBlocks[blockNumber].Add(size - 2);
    }

    // 아래로 내려가기
    private void MoveBottom(int rowNum) {
        foreach(var group in rowBlocks) {
            if(group.Value.Count == 0) continue;

            var randomDownCount = Random.Range(1, group.Value.Count);

            for(int i=0; i < randomDownCount; i++) {
                var randomBlockIndex = Random.Range(0, group.Value.Count);
                int colNum = group.Value[randomBlockIndex];
                board[rowNum, colNum] = board[rowNum + 1, colNum] = board[rowNum - 1, colNum];

                group.Value.RemoveAt(randomBlockIndex);
            }

            foreach(var idx in group.Value) {
                board[rowNum, idx] = -1;
            }
        }

        for(int i=2; i < size - 1; i += 2) {
            board[rowNum, i] = -1;
        }
    }

    private Vector3 GetWallPosition(int row, int col) {
        return new Vector3((col - (size - 1) / 2) * wallSize.x, wallSize.y, ((size - 1) / 2 - row) * wallSize.z);
    }

    private void GenerateObjects() {
        for(int i=0; i < size; i++) {
            for(int j=0; j < size; j++) {
                if(board[i, j] == -1) {
                    Instantiate(wallObject, GetWallPosition(i, j), Quaternion.identity, gameObject.transform);
                }
            }
        }
    }
}
