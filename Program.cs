using DataStructureTest.Data;
using DataStructureTest.Models;
using DataStructureTest.Services;
using System;
using System.Collections.Generic;

namespace DataSctructureTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            InitDatabase initDatabase = new InitDatabase();
            initDatabase.Init();

            CategoryService categoryService = new CategoryService();
            Category articles = categoryService.FindByName("articles");
            categoryService.DeleteNode(articles);
            List<Category> categories = categoryService.FindAll();

            NestedSetService nestedSetService = new NestedSetService();
            Dictionary<int, Category> categoriesTree = nestedSetService.BuildTree(ref categories);
            List<Category> categoriesTreeList = nestedSetService.DictionaryTreeToList(categoriesTree);
            nestedSetService.PrintTree(categoriesTree);
            Console.WriteLine("\n\n");
            nestedSetService.PrintTree(categoriesTreeList);
            Console.WriteLine("\n\n");
            List<Category> sortedTree = nestedSetService.SortTreeList(categoriesTreeList);
            nestedSetService.PrintTree(sortedTree);
        }
    }
}
