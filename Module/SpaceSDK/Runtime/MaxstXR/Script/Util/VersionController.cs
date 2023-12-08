using Maxst.Passport;
using Maxst.Settings;
using MaxstXR.Place;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VersionController
{
    public enum Mode
    {
        Legacy,
        Modern
    }

    private static VersionController _instance;
    private static readonly object _lock = new object();
    public Mode CurrentMode { get; private set; } = Mode.Modern;
    public SpaceStep CurrentSpaceStep { get; private set; } = SpaceStep.PUBLIC;

    private VersionController() { }

    public static VersionController Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new VersionController();
                }
                return _instance;
            }
        }
    }

    public void SwitchMode(Mode mode, SpaceStep step = SpaceStep.PUBLIC, EnvType envType = EnvType.Prod)
    {
        EnvAdmin.Instance.SetConfiguration(envType, DomainType.maxst, LngType.ko,
                    () =>
                    {
                        TokenRepo.Instance.ClearSavedToken();
                    });
        CurrentMode = mode;
        CurrentSpaceStep = step;
    }

    public void SwitchMode(Mode mode)
    {
        CurrentMode = mode;
    }
}
