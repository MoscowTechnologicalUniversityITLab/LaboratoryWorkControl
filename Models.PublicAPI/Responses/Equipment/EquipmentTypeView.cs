﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Models.PublicAPI.Responses.Equipment
{
    public class EquipmentTypeView
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}