﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityPhone.Model
{
    public interface IModifyable
    {
        /// <summary>
        /// Tracks the persistance status of an object.
        /// null - Newly created, not yet persisted
        /// false - persisted remotely, local copy unchanged
        /// true - persisted only locally OR the local copy has been changed
        /// </summary>
        bool? IsModified { get; set; }
    }
}