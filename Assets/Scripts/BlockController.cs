using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    public static BlockController inst;

    // 테트로미노 생성 위치
    readonly Vector3 spawnPosition = new Vector3(5.0f, 22.0f, 0.0f);
    // 다음 테트로미노 위치
    readonly Vector3[] nextBlocksPosition = {
        new Vector3(17.0f, 20.0f, 0.0f),
        new Vector3(17.0f, 16.0f, 0.0f),
        new Vector3(17.0f, 12.0f, 0.0f),
        new Vector3(17.0f, 8.0f, 0.0f),
        new Vector3(17.0f, 4.0f, 0.0f)
    };
    // 홀드된 테트로미노 위치
    readonly Vector3 heldBlockPosition = new Vector3(-7.0f, 20.0f, 0.0f);

    // 슈퍼 로테이션 시스템 테이블
    readonly (int x, int y)[,,] SRSTABLE = {
        {
            { (0, 0), (0, 0), (0, 0), (0, 0), (0, 0) },      // 0 >> 0
            { (0, 0), (-1, 0), (-1, 1), (0, -2), (-1, -2) }, // 0 >> 1
            { (0, 0), (0, 0), (0, 0), (0, 0), (0, 0) },      // 0 >> 2
            { (0, 0), (1, 0), (1, 1), (0, -2), (1, -2) }     // 0 >> 3
        },
        {
            { (0, 0), (1, 0), (1, -1), (0, 2), (1, 2) },     // 1 >> 0
            { (0, 0), (0, 0), (0, 0), (0, 0), (0, 0) },      // 1 >> 1
            { (0, 0), (1, 0), (1, -1), (0, 2), (1, 2) },     // 1 >> 2
            { (0, 0), (0, 0), (0, 0), (0, 0), (0, 0) }       // 1 >> 3
        },
        {
            { (0, 0), (0, 0), (0, 0), (0, 0), (0, 0) },      // 2 >> 0
            { (0, 0), (-1, 0), (-1, 1), (0, -2), (-1, -2) }, // 2 >> 1
            { (0, 0), (0, 0), (0, 0), (0, 0), (0, 0) },      // 2 >> 2
            { (0, 0), (1, 0), (1, 1), (0, -2), (1, -2) }     // 2 >> 3
        },
        {
            { (0, 0), (-1, 0), (-1, -1), (0, 2), (-1, 2) },  // 3 >> 0
            { (0, 0), (0, 0), (0, 0), (0, 0), (0, 0) },      // 3 >> 1
            { (0, 0), (-1, 0), (-1, -1), (0, 2), (-1, 2) },  // 3 >> 2
            { (0, 0), (0, 0), (0, 0), (0, 0), (0, 0) }       // 3 >> 3
        }
    };
    readonly (int x, int y)[,,] I_SRSTABLE = {
        {
            { (0, 0), (0, 0), (0, 0), (0, 0), (0, 0) },      // 0 >> 0
            { (0, 0), (-2, 0), (1, 0), (-2, -1), (1, 2) },   // 0 >> 1
            { (0, 0), (0, 0), (0, 0), (0, 0), (0, 0) },      // 0 >> 2
            { (0, 0), (-1, 0), (2, 0), (-1, 2), (2, -1) }    // 0 >> 3
        },
        {
            { (0, 0), (2, 0), (-1, 0), (2, 1), (-1, -2) },   // 1 >> 0
            { (0, 0), (0, 0), (0, 0), (0, 0), (0, 0) },      // 1 >> 1
            { (0, 0), (-1, 0), (2, 0), (-1, 2), (2, -1) },   // 1 >> 2
            { (0, 0), (0, 0), (0, 0), (0, 0), (0, 0) }       // 1 >> 3
        },
        {
            { (0, 0), (0, 0), (0, 0), (0, 0), (0, 0) },      // 2 >> 0
            { (0, 0), (1, 0), (-2, 0), (1, -2), (-2, 1) },   // 2 >> 1
            { (0, 0), (0, 0), (0, 0), (0, 0), (0, 0) },      // 2 >> 2
            { (0, 0), (2, 0), (-1, 0), (2, 1), (-1, -2) }    // 2 >> 3
        },
        {
            { (0, 0), (1, 0), (-2, 0), (1, -2), (-2, 1) },   // 3 >> 0
            { (0, 0), (0, 0), (0, 0), (0, 0), (0, 0) },      // 3 >> 1
            { (0, 0), (-2, 0), (1, 0), (-2, -1), (1, 2) },   // 3 >> 2
            { (0, 0), (0, 0), (0, 0), (0, 0), (0, 0) }       // 3 >> 3
        }
    };

    // 현재 테트로미노
    GameObject currentBlock;
    // 다음 테트로미노
    GameObject[] nextBlocks = new GameObject[5];
    // 홀드된 테트로미노
    GameObject heldBlock;
    // 홀드 사용 했는지 확인
    bool isHeld = false;

    // 현재 필드 상태 확인
    bool[,] availableField;

    // 현재 테트로미노의 네 블럭들
    [SerializeField]
    Transform[] childs = new Transform[4];

    // 자동낙하를 위한 시간 누적
    float elapsedTime = 0.0f;

    void Start()
    {
        if (inst == null) inst = this;
        else Destroy(this);

        InitSetting();
    }

    void Update()
    {
        // 이동
        Vector3[] pos = new Vector3[4];
        for (int i = 0; i < 4; i++) {
            pos[i] = childs[i].position;
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            // 왼쪽
            if (CheckValid(pos, (-1, 0))) {
                currentBlock.transform.Translate(-1, 0, 0, Space.World);
            }
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            // 오른쪽
            if (CheckValid(pos, (1, 0))) {
                currentBlock.transform.Translate(1, 0, 0, Space.World);
            }
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            // soft drop
            if (CheckValid(pos, (0, -1))) {
                currentBlock.transform.Translate(0, -1, 0, Space.World);
                elapsedTime = 0.0f;
            } else {
                BlockDropped();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space)) {
            // hard drop
            for (int dy = -1; ; dy--) {
                if (!CheckValid(pos, (0, dy))) {
                    currentBlock.transform.Translate(0, dy + 1, 0, Space.World);
                    break;
                }
            }
            BlockDropped();
        } else if (Input.GetKeyDown(KeyCode.LeftShift)) {
            // 테트로미노 홀드
            HoldBlock();
        }

        for (int i = 0; i < 4; i++) {
            pos[i] = childs[i].position;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.X)) {
            CheckRotationValid(currentBlock.transform.position, pos, 1);
        } else if (Input.GetKeyDown(KeyCode.Z)) {
            CheckRotationValid(currentBlock.transform.position, pos, -1);
        }


        // 블록 자동 낙하
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= GameManager.inst.FallSpeed) {
            for (int i = 0; i < 4; i++) {
                pos[i] = childs[i].position;
            }
            if (CheckValid(pos, (0, -1))) {
                currentBlock.transform.Translate(0, -1.0f, 0, Space.World);
            } else {
                BlockDropped();
            }
            elapsedTime = 0.0f;
        }
    }

    private void InitSetting() {
        // 현재 블럭 생성
        BlockGenerator.inst.GenerateBlock(spawnPosition, out this.currentBlock);
        Transform[] chtf = currentBlock.GetComponentsInChildren<Transform>();
        for (int i = 1; i < 5; i++) {
            childs[i - 1] = chtf[i];
        }
        GameManager.inst.GetAvailablePositions(out availableField);

        // 다음 다섯 블럭 생성
        BlockGenerator.inst.GenerateBlock(nextBlocksPosition[0], out this.nextBlocks[0]);
        BlockGenerator.inst.GenerateBlock(nextBlocksPosition[1], out this.nextBlocks[1]);
        BlockGenerator.inst.GenerateBlock(nextBlocksPosition[2], out this.nextBlocks[2]);
        BlockGenerator.inst.GenerateBlock(nextBlocksPosition[3], out this.nextBlocks[3]);
        BlockGenerator.inst.GenerateBlock(nextBlocksPosition[4], out this.nextBlocks[4]);
    }

    // pos위치의 블럭들을 dp만큼 옮긴 곳에 놓을 수 있는지 확인
    private bool CheckValid(Vector3[] pos, (int x, int y) dp) {
        bool flag = true;
        foreach (Vector3 p in pos) {
            int x = Mathf.RoundToInt(p.x - 0.5f) + dp.x;
            int y = Mathf.RoundToInt(p.y - 0.5f) + dp.y;

            if (x < 0 || x >= 10 || y < 0) return false;
            flag &= availableField[y, x];
        }
        return flag;
    }

    private bool CheckValid(Vector3[] pos) {
        return CheckValid(pos, (0, 0));
    }

    private bool CheckRotationValid(Vector3 rotationCenter, Vector3[] pos, int isCW) {
        Matrix4x4 mat = Matrix4x4.Translate(rotationCenter);
        mat *= Matrix4x4.Rotate(Quaternion.Euler(0.0f, 0.0f, isCW * -90.0f));
        mat *= Matrix4x4.Translate(-rotationCenter);

        Block blk = currentBlock.GetComponent<Block>();

        int beforeRot = blk.RotationState;
        int afterRot = (beforeRot + isCW + 4) % 4;

        bool flag = true;

        int i = 0;
        for (i = 0; i < 5; i++) {
            flag = true;
            foreach (Vector3 p in pos) {
                Vector3 p3 = mat.MultiplyPoint3x4(p);
                int y = Mathf.RoundToInt(p3.y - 0.5f);
                int x = Mathf.RoundToInt(p3.x - 0.5f);
                if (blk.type == Block.Type.I) {
                    y += I_SRSTABLE[beforeRot, afterRot, i].y;
                    x += I_SRSTABLE[beforeRot, afterRot, i].x;
                } else {
                    y += SRSTABLE[beforeRot, afterRot, i].y;
                    x += SRSTABLE[beforeRot, afterRot, i].x;
                }

                if (x < 0 || x >= 10 || y < 0 || y >= 24) flag = false;
                else flag &= availableField[y, x];

                if (!flag) break;
            }
            if (flag) break;
        }

        if (flag) {
            currentBlock.transform.Rotate(new Vector3(0f, 0f, isCW * -90.0f));
            blk.RotationState = afterRot;
            if (blk.type == Block.Type.I) {
                currentBlock.transform.Translate(I_SRSTABLE[beforeRot, afterRot, i].x,
                    I_SRSTABLE[beforeRot, afterRot, i].y, 0.0f, Space.World);
            } else {
                currentBlock.transform.Translate(SRSTABLE[beforeRot, afterRot, i].x,
                    SRSTABLE[beforeRot, afterRot, i].y, 0.0f, Space.World);
            }
        }

        return flag;
    }

    private void HoldBlock() {
        if (isHeld) return;
        if (heldBlock == null) {
            heldBlock = currentBlock;
            heldBlock.transform.position = heldBlockPosition;
            if (heldBlock.GetComponent<Block>().type != Block.Type.I && heldBlock.GetComponent<Block>().type != Block.Type.O) {
                heldBlock.transform.position -= new Vector3(0.5f, 0.5f, 0.0f);
            }

            elapsedTime = 0.0f;

            currentBlock = nextBlocks[0];
            if (currentBlock.GetComponent<Block>().type == Block.Type.I || currentBlock.GetComponent<Block>().type == Block.Type.O) {
                currentBlock.transform.position = spawnPosition;
            }
            else {
                currentBlock.transform.position = spawnPosition - new Vector3(0.5f, 0.5f, 0.0f);
            }
            
            GameManager.inst.GetAvailablePositions(out availableField);
            nextBlocks[0] = nextBlocks[1];
            nextBlocks[0].transform.Translate(0, 4.0f, 0.0f, Space.World);
            nextBlocks[1] = nextBlocks[2];
            nextBlocks[1].transform.Translate(0, 4.0f, 0.0f, Space.World);
            nextBlocks[2] = nextBlocks[3];
            nextBlocks[2].transform.Translate(0, 4.0f, 0.0f, Space.World);
            nextBlocks[3] = nextBlocks[4];
            nextBlocks[3].transform.Translate(0, 4.0f, 0.0f, Space.World);
            BlockGenerator.inst.GenerateBlock(nextBlocksPosition[4], out this.nextBlocks[4]);
        }
        else {
            GameObject tmp = currentBlock;
            currentBlock = heldBlock;
            currentBlock.transform.position = spawnPosition;
            if (currentBlock.GetComponent<Block>().type != Block.Type.I && currentBlock.GetComponent<Block>().type != Block.Type.O) {
                currentBlock.transform.position -= new Vector3(0.5f, 0.5f, 0.0f);
            }

            heldBlock = tmp;
            heldBlock.transform.position = heldBlockPosition;
            if (heldBlock.GetComponent<Block>().type != Block.Type.I && heldBlock.GetComponent<Block>().type != Block.Type.O) {
                heldBlock.transform.position -= new Vector3(0.5f, 0.5f, 0.0f);
            }
        }

        Transform[] tfs = currentBlock.GetComponentsInChildren<Transform>();
        for (int i = 1; i < 5; i++) {
            childs[i - 1] = tfs[i];
        }

        isHeld = true;
    }

    private void BlockDropped() {
        GameEvents.inst.BlockDrop(currentBlock.GetComponent<Block>());
        elapsedTime = 0.0f;
        isHeld = false;

        currentBlock = nextBlocks[0];
        if (currentBlock.GetComponent<Block>().type == Block.Type.I || currentBlock.GetComponent<Block>().type == Block.Type.O) {
            currentBlock.transform.position = spawnPosition;
        } else {
            currentBlock.transform.position = spawnPosition - new Vector3(0.5f, 0.5f, 0.0f);
        }
        Transform[] tfs = currentBlock.GetComponentsInChildren<Transform>();
        for (int i = 1; i < 5; i++) {
            childs[i - 1] = tfs[i];
        }
        GameManager.inst.GetAvailablePositions(out availableField);
        nextBlocks[0] = nextBlocks[1];
        nextBlocks[0].transform.Translate(0, 4.0f, 0.0f, Space.World);
        nextBlocks[1] = nextBlocks[2];
        nextBlocks[1].transform.Translate(0, 4.0f, 0.0f, Space.World);
        nextBlocks[2] = nextBlocks[3];
        nextBlocks[2].transform.Translate(0, 4.0f, 0.0f, Space.World);
        nextBlocks[3] = nextBlocks[4];
        nextBlocks[3].transform.Translate(0, 4.0f, 0.0f, Space.World);
        BlockGenerator.inst.GenerateBlock(nextBlocksPosition[4], out this.nextBlocks[4]);
    }
}
