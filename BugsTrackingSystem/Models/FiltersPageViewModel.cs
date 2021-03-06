﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugsTrackingSystem.Models
{
    public class FiltersPageViewModel
    {
        public NewDefectViewModel Select { get; set; }

        public IEnumerable<FilterSimpleViewModel> FilterInfo { get; set; }

        public PageInfo PageInfo { get; set; }
    }
}
