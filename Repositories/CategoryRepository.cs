using DataStructureTest.Data;
using DataStructureTest.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace DataStructureTest.Repositories
{
    class CategoryRepository
    {
        private readonly SQLiteConnection connection;

        public CategoryRepository()
        {
            connection = DatabaseConnection.GetConnection();
        }

        public void InsertCategory(Category category)
        {
            var cmd = new SQLiteCommand(connection);

            cmd.CommandText = @"INSERT INTO [Categories] (Lft, Rgt, Level, ParentId, Name) VALUES (
                @lft, @rgt, @level, @parent_id, @name
            )";
            cmd.Parameters.AddWithValue("@lft", category.Lft);
            cmd.Parameters.AddWithValue("@rgt", category.Rgt);
            cmd.Parameters.AddWithValue("@level", category.Level);
            cmd.Parameters.AddWithValue("@parent_id", category.ParentId);
            cmd.Parameters.AddWithValue("@name", category.Name);
            cmd.ExecuteNonQuery();
        }

        public Category FindByName(string name)
        {
            var cmd = new SQLiteCommand(connection);
            cmd.CommandText = "select * from [Categories] where Name = @name";
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Prepare();

            SQLiteDataReader dr = cmd.ExecuteReader();
            dr.Read();
            Category cat = new Category();
            cat.Id = dr.GetInt32(0);
            cat.Lft = dr.GetInt32(1);
            cat.Rgt = dr.GetInt32(2);
            cat.Level = dr.GetInt32(3);
            cat.ParentId = dr.GetInt32(4);
            cat.Name = dr.GetString(5);
            return cat;
        }

        internal void DeleteCategoryThatHasDescendants(Category category)
        {
            var cmd = new SQLiteCommand(connection);
            int rgt = category.Rgt;
            int lft = category.Lft;

            cmd.CommandText = @"UPDATE [Categories] SET Level = Level - 1, ParentId = @ParentId WHERE Level = @Level AND Lft > @rootLft AND Rgt < @rootRgt";
            cmd.Parameters.AddWithValue("@Level", category.Level + 1);
            cmd.Parameters.AddWithValue("@ParentId", category.ParentId);
            cmd.Parameters.AddWithValue("@rootLft", lft);
            cmd.Parameters.AddWithValue("@rootRgt", rgt);
            cmd.ExecuteNonQuery();

            // update descendants left and right
            cmd.CommandText = @"UPDATE [Categories] SET Lft = Lft - 1 WHERE Lft >= @rootLft AND Rgt <= @rootRgt";
            cmd.Parameters.AddWithValue("@rootLft", lft);
            cmd.Parameters.AddWithValue("@rootRgt", rgt);
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"UPDATE [Categories] SET Rgt = Rgt - 1 WHERE Lft >= @rootLft AND Rgt <= @rootRgt";
            cmd.Parameters.AddWithValue("@rootLft", lft);
            cmd.Parameters.AddWithValue("@rootRgt", rgt);
            cmd.ExecuteNonQuery();

            // update all further nodes that are to the right of deleted node
            cmd.CommandText = @"UPDATE [Categories] SET Lft = Lft - 2 WHERE Lft >= @num";
            cmd.Parameters.AddWithValue("@num", rgt);
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"UPDATE [Categories] SET Rgt = Rgt - 2 WHERE Rgt >= @num";
            cmd.Parameters.AddWithValue("@num", rgt);
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"DELETE FROM [Categories] WHERE Id  = @Id";
            cmd.Parameters.AddWithValue("@Id", category.Id);
            cmd.ExecuteNonQuery();
        }

        public List<Category> FindAll()
        {
            var cmd = new SQLiteCommand(connection);
            cmd.CommandText = "select * from [Categories]";

            SQLiteDataReader dr = cmd.ExecuteReader();

            List<Category> categories = new List<Category>();
            while (dr.Read())
            {
                Category category = new Category();
                category.Id = dr.GetInt32(0);
                category.Lft = dr.GetInt32(1);
                category.Rgt = dr.GetInt32(2);
                category.Level = dr.GetInt32(3);
                category.ParentId = dr.GetInt32(4);
                category.Name = dr.GetString(5);
                categories.Add(category);
            }
            return categories;
        }

        public void InsertBefore(Category category, string name)
        {
            var cmd = new SQLiteCommand(connection);
            int rgt = category.Rgt;
            int lft = category.Lft;

            cmd.CommandText = @"UPDATE [Categories] SET Lft = Lft + 2 WHERE Lft >= @num";
            cmd.Parameters.AddWithValue("@num", lft);
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"UPDATE [Categories] SET Rgt = Rgt + 2 WHERE Rgt >= @num";
            cmd.Parameters.AddWithValue("@num", rgt);
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"INSERT INTO [Categories] (Lft, Rgt, Level, ParentId, Name) VALUES (
                @lft, @rgt, @level, @parent_id, @name
            )";
            cmd.Parameters.AddWithValue("@lft", lft);
            cmd.Parameters.AddWithValue("@rgt", rgt);
            cmd.Parameters.AddWithValue("@level", category.Level);
            cmd.Parameters.AddWithValue("@parent_id", category.ParentId);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.ExecuteNonQuery();
        }

        public void PrependTo(Category category, string name)
        {
            var cmd = new SQLiteCommand(connection);
            int rgt = category.Rgt;

            cmd.CommandText = @"UPDATE [Categories] SET Lft = Lft + 2 WHERE Lft > @num";
            cmd.Parameters.AddWithValue("@num", rgt);
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"UPDATE [Categories] SET Rgt = Rgt + 2 WHERE Rgt > @num";
            cmd.Parameters.AddWithValue("@num", rgt);
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"UPDATE [Categories] SET Rgt = Rgt + 2 WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", category.Id);
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"INSERT INTO [Categories] (Lft, Rgt, Level, ParentId, Name) VALUES (
                @lft, @rgt, @level, @parent_id, @name
            )";
            cmd.Parameters.AddWithValue("@lft", rgt);
            cmd.Parameters.AddWithValue("@rgt", rgt + 1);
            cmd.Parameters.AddWithValue("@level", category.Level + 1);
            cmd.Parameters.AddWithValue("@parent_id", category.Id);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.ExecuteNonQuery();
        }

        public void DeleteLeafCategory(Category category)
        {
            var cmd = new SQLiteCommand(connection);
            int rgt = category.Rgt;
            int lft = category.Lft;

            cmd.CommandText = @"UPDATE [Categories] SET `Lft` = `Lft` - 2 WHERE `Lft` > @num";
            cmd.Parameters.AddWithValue("@num", lft);
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"UPDATE [Categories] SET `Rgt` = `Rgt` - 2 WHERE `Rgt` > @num";
            cmd.Parameters.AddWithValue("@num", rgt);
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"DELETE FROM [Categories] WHERE `Id` = @id";
            cmd.Parameters.AddWithValue("@id", category.Id);
            cmd.ExecuteNonQuery();
        }
    }
}
