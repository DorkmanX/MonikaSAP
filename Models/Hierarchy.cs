﻿namespace MonikaSAP.Models
{
    public class Hierarchy
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public short HierachyLevel { get; set; }
        public short HierarchyType { get; set; }
        public string ReferenceBatchNumber { get; set; }
        public string OverlordSuborderNumber { get; set; }
    }
}
