using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class GameManager : MonoBehaviour {

    public static GameManager inst;

    const int HEIGHT = 24;
    const int WIDTH = 10;

    #region Properties

    [SerializeField]
    private uint _level = 1;
    public uint Level {
        get {
            return _level;
        }
        set {
            _level = value;
            UpdateFallSpeed();
        }
    }

    [SerializeField]
    private float _fallSpeed = 1.0f;
    public float FallSpeed {
        get {
            return _fallSpeed;
        }
    }

    #endregion


    #region Private Variables

    private int[] filledCount = new int[HEIGHT];

    #endregion


    #region Public Variables

    public BlockGenerator blockGen;

    public GameObject[,] GameGrid = new GameObject[HEIGHT, WIDTH];

    #endregion


    #region Private Methods

    void Start()
    {
        if (inst == null) {
            inst = this;
        } else {
            Destroy(this);
        }

        UpdateFallSpeed();

        GameEvents.inst.BlockDropEvent += OnBlockDrop;

        // 임시 게임 필드 윤곽선
        DrawLine(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 22.0f, 0.0f), Color.red, 0.1f);
        DrawLine(new Vector3(10.0f, 0.0f, 0.0f), new Vector3(10.0f, 22.0f, 0.0f), Color.red, 0.1f);
        DrawLine(new Vector3(0.0f, 22.0f, 0.0f), new Vector3(10.0f, 22.0f, 0.0f), Color.red, 0.1f);
        DrawLine(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(10.0f, 0.0f, 0.0f), Color.red, 0.1f);
    }

    void Update()
    {
        
    }

    #endregion


    #region Private Methods

    private void DrawLine(Vector3 start, Vector3 end, Color color, float width) {
        GameObject line = new GameObject();
        line.transform.position = start;
        LineRenderer lr = line.AddComponent<LineRenderer>();    
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    private void DeleteLine(int y) {
        for (int x = 0; x < 10; x++) {
            Destroy(GameGrid[y, x]);
            GameGrid[y, x] = null;
        }
        filledCount[y] = 0;
    }

    private void DropLines() {
        int[] lines2drop = new int[HEIGHT];
        int highestY = 0;
        for (int y = 1; y < HEIGHT; y++) {
            lines2drop[y] = lines2drop[y - 1];
            if (filledCount[y - 1] == 0) lines2drop[y] += 1;
            if (filledCount[y] != 0) highestY = y;
        }

        for (int y = 1; y <= highestY; y++) {
            if (lines2drop[y] > 0) {
                for (int x = 0; x < WIDTH; x++) {
                    // 존재하는 블럭들 드랍
                    if (GameGrid[y, x] != null) {
                        GameGrid[y, x].transform.Translate(0.0f, -lines2drop[y], 0.0f, Space.World);
                        GameGrid[y - lines2drop[y], x] = GameGrid[y, x];
                        GameGrid[y, x] = null;
                    }
                }
                filledCount[y - lines2drop[y]] = filledCount[y];
                filledCount[y] = 0;
            }
        }
    }

    #endregion


    #region Public Methods

    // 블럭이 떨어졌을 때
    public void OnBlockDrop(Block obj) {
        foreach (Transform child in obj.gameObject.GetComponentsInChildren<Transform>()) {
            if (child == obj.transform) continue;

            Vector3 p = child.position;
            int x = Mathf.RoundToInt(p.x - 0.5f);
            int y = Mathf.RoundToInt(p.y - 0.5f);

            GameGrid[y, x] = child.gameObject;
            filledCount[y] += 1;

            child.transform.SetParent(this.transform);
        }
        Destroy(obj.gameObject);

        for (int y = 0; y < HEIGHT; y++) {
            if (filledCount[y] == 10) {
                DeleteLine(y);
            }
        }
        DropLines();
    }

    public void UpdateFallSpeed() {
        _fallSpeed = Mathf.Pow(0.8f - (_level - 1) * 0.007f, _level - 1);
    }

    public void GetAvailablePositions(out bool[,] testField) {
        testField = new bool[HEIGHT, WIDTH];
        for (int i = 0; i < HEIGHT; i++) {
            for (int j = 0; j < WIDTH; j++) {
                testField[i, j] = GameGrid[i, j] == null ? true : false;
            }
        }
    }

    #endregion

}
