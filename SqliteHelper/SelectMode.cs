namespace SqliteHelper
{
    using System;

    public enum SelectMode
    {
        AndEqual = 1,
        AndFirstLike = 4,
        AndLastLike = 8,
        AndLike = 0x10,
        AndNoequal = 2,
        OrEqual = 0x20,
        OrFirstLike = 0x80,
        OrLastLike = 0x100,
        OrLike = 0x200,
        OrNoequal = 0x40,
        UnKnow = 0
    }
}

