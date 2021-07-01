using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    enum RotateDirection { CCW, CW };
    public enum State { STOP, MOVE };
    public enum Type { O, I, J, L, S, Z, T };

    #region Constant Variables

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

    #endregion


    #region Properties

    private State _state = State.STOP;
    public State state {
        get {
            return _state;
        }
        set {
            _state = value;
        }
    }

    // 0: Spawn State, 1: CW rotation from 0, 2: 2 successive rotations from 0, 3: CCW rotation from 0
    int rotationState = 0;
    public int RotationState {
        get {
            return rotationState;
        }
        set {
            rotationState = value;
        }
    }

    #endregion


    #region Static Variables

    #endregion


    #region Private Variables

    Transform[] childTransforms = new Transform[4];

    bool[,] availableField;

    #endregion


    #region Public Variables

    public Type type;

    #endregion


    #region Private Methods

    void Start()
    {
        // Load each child's position.
        int i = 0;
        foreach (Transform child in this.GetComponentsInChildren<Transform>()) {
            if (i != 0) {
                childTransforms[i - 1] = child;
            }
            i++;
        }

        GameManager.inst.GetAvailablePositions(out availableField);
    }

    void Update() {

    }

    //private void FixedUpdate() {
    //    int i = 0;
    //    foreach (Transform tf in childTransforms) {
    //        Debug.LogFormat("Child {0} position: ({1}, {2})", i++, tf.position.x, tf.position.y);
    //    }
    //}

    #endregion


    #region Public Methods

    #endregion
}
