﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Interface
{
    public interface ITransaction<T>
    {
        void Execute();
    }
}
