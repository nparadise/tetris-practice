using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGenerator : MonoBehaviour {

    public static BlockGenerator inst;

    #region Properties

    #endregion


    #region Constant Variables

    readonly Vector3[] nextBlocksPosition = {
        new Vector3(17.0f, 20.0f, 0.0f),
        new Vector3(17.0f, 16.0f, 0.0f),
        new Vector3(17.0f, 12.0f, 0.0f),
        new Vector3(17.0f, 8.0f, 0.0f),
        new Vector3(17.0f, 4.0f, 0.0f)
    };

    #endregion


    #region Private Variables

    enum BlockType {D, I, L, J, S, Z};

    [SerializeField]
    private GameObject[] blockPrefab;

    #endregion


    #region Public Variables

    #endregion


    #region Private Methods

    void Start()
    {
        if (inst == null) {
            inst = this;
        } else {
            Destroy(this);
        }
    }

    #endregion


    #region Public Methods

    public void GenerateBlock(Vector3 pos, out GameObject blk) {
        GameObject created;

        int type = UnityEngine.Random.Range(0, 7);
        if (type <= 1) // block O and block I
            created = Instantiate(blockPrefab[type], pos, Quaternion.identity);
        else
            created = Instantiate(blockPrefab[type], pos - new Vector3(0.5f, 0.5f, 0.0f), Quaternion.identity);

        blk = created;
    }

    #endregion

}
