﻿using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class MethodologyParent
    {
        public Guid Id { get; set; }
        
        public List<PublicationMethodology> Publications { get; set; }
    }
}
