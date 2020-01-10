using DataStructureTest.Models;
using DataStructureTest.Services;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace DataStructureTest.Data
{
    class InitDatabase
    {
        private readonly SQLiteConnection connection;
        private CategoryService categoryService;

        public InitDatabase()
        {
            connection = DatabaseConnection.GetConnection();
            categoryService = new CategoryService();
        }

        public void Init()
        {
            CreateTable();
            categoryService.AddRootNode();
            Category rootCategory = categoryService.FindByName("root");
            categoryService.PrependTo(rootCategory, "articles");
            rootCategory = categoryService.FindByName("root");
            categoryService.PrependTo(rootCategory, "books");

            Category articlesCategory = categoryService.FindByName("articles");
            categoryService.PrependTo(articlesCategory, "observation");
            articlesCategory = categoryService.FindByName("articles");
            categoryService.PrependTo(articlesCategory, "research articles");

            Category booksCategory = categoryService.FindByName("books");
            categoryService.PrependTo(booksCategory, "fiction");
            booksCategory = categoryService.FindByName("books");
            categoryService.PrependTo(booksCategory, "science");
        }

        public void CreateTable()
        {
            SQLiteCommand cmd = new SQLiteCommand(connection)
            {
                CommandText = "DROP TABLE IF EXISTS [Categories]"
            };
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"CREATE TABLE [Categories] (
                [Id] INTEGER PRIMARY KEY,
                [Lft] INT, rgt INT,
                [Level] int,
                [ParentId] int,
                [Name] TEXT
            )";
            cmd.ExecuteNonQuery();
        }
    }
}
