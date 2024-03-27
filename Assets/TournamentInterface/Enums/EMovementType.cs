namespace YokaiNoMori.Enumeration
{


    [System.Flags]
    public enum EMovementType
    {
        FORWARD_LEFT = 1 << 0,
        FORWARD_MID = 1 << 1,
        FORWARD_RIGHT = 1 << 2,
        MID_LEFT = 1 << 3,
        MID_RIGHT = 1 << 4,
        BACK_LEFT = 1 << 5,
        BACK_MID = 1 << 6,
        BACK_RIGHT = 1 << 7
    }
}
