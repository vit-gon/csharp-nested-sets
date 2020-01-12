using System;
using System.Collections.Generic;
using System.Text;

namespace DataStructureTest.Models
{
    public class Category
    {
        public int Id { get; set; }
        public int Lft { get; set; }
        public int Rgt { get; set; }
        public int Level { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }

        public Dictionary<int, Category> Children { get; set; }
        public List<Category> ChildrenList { get; set; }
    }
}
