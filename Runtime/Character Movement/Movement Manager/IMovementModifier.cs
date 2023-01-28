﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMovementModifier 
{
    Vector3 MovementVector { get; }
    bool MovementPaused { get;  }

    void PauseMovement(bool paused);
}
