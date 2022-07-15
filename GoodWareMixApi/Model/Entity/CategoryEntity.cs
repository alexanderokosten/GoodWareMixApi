﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodWareMixApi.Model
{
    public class CategoryEntity
    {
        public string? VenderId  { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? SubCategories { get; set; }
    }
}
