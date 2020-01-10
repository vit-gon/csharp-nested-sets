using DataStructureTest.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataStructureTest.Services
{
    class NestedSetService
    {
        public Dictionary<int, Category> BuildTree(ref List<Category> categoriesList, int parentId = 0)
        {
            Dictionary<int, Category> dictionary = new Dictionary<int, Category>();

            for (int i = categoriesList.Count - 1; i >= 0; i--)
            {
                Category category = categoriesList[i];
                if (category.ParentId == parentId)
                {
                    Dictionary<int, Category> children = BuildTree(ref categoriesList, category.Id);

                    if (children.Count != 0)
                    {
                        category.Children = children;
                    }
                    dictionary[category.Id] = category;
                    categoriesList.RemoveAll(x => x.Id == category.Id);
                }
            }
            return dictionary;
        }

        public List<Category> DictionaryTreeToList(Dictionary<int, Category> tree)
        {
            List<Category> categories = new List<Category>();

            foreach (KeyValuePair<int, Category> keyValuePair in tree)
            {
                Category category = keyValuePair.Value;
                
                if (category.Children != null && category.Children.Count != 0)
                {
                    category.ChildrenList = DictionaryTreeToList(category.Children);
                }
                categories.Add(category);
            }
            return categories;
        }

        public void PrintTree(Dictionary<int, Category> tree)
        {
            foreach (KeyValuePair<int, Category> keyValuePair in tree)
            {
                Category category = keyValuePair.Value;
                int lvl = category.Level;
                string lvlPrefix = "";
                for (int i = 1; i < lvl; i++)
                {
                    lvlPrefix += "-";
                }
                Console.WriteLine(lvlPrefix + category.Name + " {" + category.Id + "}");
                if (category.Children != null && category.Children.Count != 0)
                {
                    PrintTree(category.Children);
                }
            }
        }

        public void PrintTree(List<Category> treeList)
        {
            foreach (Category category in treeList)
            {
                int lvl = category.Level;
                string lvlPrefix = "";
                for (int i = 1; i < lvl; i++)
                {
                    lvlPrefix += "-";
                }
                StringBuilder description = new StringBuilder();
                description.Append("{");
                description.Append($"lft: {category.Lft}, ");
                description.Append($"rgt: {category.Rgt}");
                //description.Append($"id: {category.Id}");
                description.Append("}");
                Console.WriteLine(lvlPrefix + category.Name + " " + description.ToString());
                if (category.ChildrenList != null && category.ChildrenList.Count != 0)
                {
                    PrintTree(category.ChildrenList);
                }
            }
        }

        public List<Category> SortTreeList(List<Category> treeList)
        {
            treeList.Sort((a, b) =>
            {
                if (a.Rgt > b.Rgt) return 1;
                if (a.Rgt < b.Rgt) return -1;
                return 0;
            });

            foreach (Category category in treeList)
            {
                if (category.ChildrenList != null && category.ChildrenList.Count > 1)
                {
                    List<Category> sortedChildList = SortTreeList(category.ChildrenList);
                    category.ChildrenList = sortedChildList;
                }
            }
            return treeList;
        }
    }
}
