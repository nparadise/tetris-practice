using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    public static BlockController inst;

    // ��Ʈ�ι̳� ���� ��ġ
    readonly Vector3 spawnPosition = new Vector3(5.0f, 22.0f, 0.0f);
    // ���� ��Ʈ�ι̳� ��ġ
    readonly Vector3[] nextBlocksPosition = {
        new Vector3(17.0f, 20.0f, 0.0f),
        new Vector3(17.0f, 16.0f, 0.0f),
        new Vector3(17.0f, 12.0f, 0.0f),
        new Vector3(17.0f, 8.0f, 0.0f),
        new Vector3(17.0f, 4.0f, 0.0f)
    };
    // Ȧ��� ��Ʈ�ι̳� ��ġ
    readonly Vector3 heldBlockPosition = new Vector3(-7.0f, 20.0f, 0.0f);

    // ���� �����̼� �ý��� ���̺�
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

    // ���� ��Ʈ�ι̳�
    GameObject currentBlock;
    // ���� ��Ʈ�ι̳�
    GameObject[] nextBlocks = new GameObject[5];
    // Ȧ��� ��Ʈ�ι̳�
    GameObject heldBlock;
    // Ȧ�� ��� �ߴ��� Ȯ��
    bool isHeld = false;

    // ���� �ʵ� ���� Ȯ��
    bool[,] availableField;

    // ���� ��Ʈ�ι̳��� �� ����
    [SerializeField]
    Transform[] childs = new Transform[4];

    // �ڵ����ϸ� ���� �ð� ����
    float elapsedTime = 0.0f;

    void Start()
    {
        if (inst == null) inst = this;
        else Destroy(this);

        InitSetting();
    }

    void Update()
    {
        // �̵�
        Vector3[] pos = new Vector3[4];
        for (int i = 0; i < 4; i++) {
            pos[i] = childs[i].position;
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            // ����
            if (CheckValid(pos, (-1, 0))) {
                currentBlock.transform.Translate(-1, 0, 0, Space.World);
            }
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            // ������
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
            // ��Ʈ�ι̳� Ȧ��
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


        // ��� �ڵ� ����
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
        // ���� �� ����
        BlockGenerator.inst.GenerateBlock(spawnPosition, out this.currentBlock);
        Transform[] chtf = currentBlock.GetComponentsInChildren<Transform>();
        for (int i = 1; i < 5; i++) {
            childs[i - 1] = chtf[i];
        }
        GameManager.inst.GetAvailablePositions(out availableField);

        // ���� �ټ� �� ����
        BlockGenerator.inst.GenerateBlock(nextBlocksPosition[0], out this.nextBlocks[0]);
        BlockGenerator.inst.GenerateBlock(nextBlocksPosition[1], out this.nextBlocks[1]);
        BlockGenerator.inst.GenerateBlock(nextBlocksPosition[2], out this.nextBlocks[2]);
        BlockGenerator.inst.GenerateBlock(nextBlocksPosition[3], out this.nextBlocks[3]);
        BlockGenerator.inst.GenerateBlock(nextBlocksPosition[4], out this.nextBlocks[4]);
    }

    // pos��ġ�� ������ dp��ŭ �ű� ���� ���� �� �ִ��� Ȯ��
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
