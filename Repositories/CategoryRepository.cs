using DataStructureTest.Data;
using DataStructureTest.Extensions;
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

        internal void MoveAfter(Category moveAfterCategory, Category category)
        {
            var cmd = new SQLiteCommand(connection);
            // for 'category' node and all its descendants
            int lftAndRgtIncrement = moveAfterCategory.Rgt - category.Rgt;
            // for all child nodes of 'moveAfterCategory' that are to the right of category
            int lftAndRgtDecrement = (category.Rgt - category.Lft) + 1;

            cmd = new SQLiteCommand(connection);
            cmd.CommandText = @"SELECT Id FROM [Categories] WHERE Lft >= @Lft AND Rgt <= @Rgt";
            cmd.Parameters.AddWithValue("@Lft", category.Lft);
            cmd.Parameters.AddWithValue("@Rgt", category.Rgt);
            List<int> categoryAndItsChildsIds = getIds(cmd);

            string cmdText;
            string parameterPrefix = "MemberId";
            if (moveAfterCategory.Rgt - category.Rgt > 1)
            {
                cmd = new SQLiteCommand(connection);
                cmd.CommandText = @"SELECT Id FROM [Categories] WHERE Lft > @Lft AND Rgt < @Rgt";
                cmd.Parameters.AddWithValue("@Lft", category.Rgt);
                cmd.Parameters.AddWithValue("@Rgt", moveAfterCategory.Rgt);
                List<int> moveAfterCategoryChildsIds = getIds(cmd);

                cmd = new SQLiteCommand(connection);
                cmdText = @"UPDATE [Categories]
                            SET Lft = Lft - @LftAndRgtDecrement,
                                Rgt = Rgt - @LftAndRgtDecrement
                            WHERE Id IN ({0})";
                cmd.CommandText = cmdText;
                cmdText = SQLiteWhereInParametersExtension.BuildWhereInClause(cmdText, parameterPrefix, moveAfterCategoryChildsIds);
                cmd.CommandText = cmdText;
                cmd.AddParamsToCommand<int>(parameterPrefix, moveAfterCategoryChildsIds);
                cmd.Parameters.AddWithValue("@LftAndRgtDecrement", lftAndRgtDecrement);
                cmd.ExecuteNonQuery();
            }

            cmd = new SQLiteCommand(connection);
            cmdText = @"UPDATE [Categories]
                        SET Lft = Lft + @LftAndRgtIncrement,
                            Rgt = Rgt + @LftAndRgtIncrement,
                            Level = Level + (@Level)
                        WHERE Id IN ({0})";
            cmdText = SQLiteWhereInParametersExtension.BuildWhereInClause(cmdText, parameterPrefix, categoryAndItsChildsIds);
            cmd.CommandText = cmdText;
            cmd.AddParamsToCommand<int>(parameterPrefix, categoryAndItsChildsIds);
            cmd.Parameters.AddWithValue("@LftAndRgtIncrement", lftAndRgtIncrement);
            cmd.Parameters.AddWithValue("@Level", moveAfterCategory.Level - category.Level);
            cmd.ExecuteNonQuery();

            cmd = new SQLiteCommand(connection);
            cmd.CommandText = @"UPDATE [Categories] SET Rgt = Rgt - @LftAndRgtDecrement WHERE Name = @Name";
            cmd.Parameters.AddWithValue("@LftAndRgtDecrement", lftAndRgtDecrement);
            cmd.Parameters.AddWithValue("@Name", moveAfterCategory.Name);
            cmd.ExecuteNonQuery();

            cmd = new SQLiteCommand(connection);
            cmd.CommandText = @"UPDATE [Categories] SET ParentId = @ParentId WHERE Name = @Name";
            cmd.Parameters.AddWithValue("@Name", category.Name);
            cmd.Parameters.AddWithValue("@ParentId", moveAfterCategory.ParentId);
            cmd.ExecuteNonQuery();
        }

        internal void MoveBefore(Category moveBeforeCategory, Category category)
        {
            var cmd = new SQLiteCommand(connection);
            // for 'category' node and all its descendants
            int lftAndRgtDecrement = category.Lft - moveBeforeCategory.Lft;
            // for all child nodes of 'moveAfterCategory' that are to the left of category
            int lftAndRgtIncrement = (category.Rgt - category.Lft) + 1;

            cmd = new SQLiteCommand(connection);
            cmd.CommandText = @"SELECT Id FROM [Categories] WHERE Lft >= @Lft AND Rgt <= @Rgt";
            cmd.Parameters.AddWithValue("@Lft", category.Lft);
            cmd.Parameters.AddWithValue("@Rgt", category.Rgt);
            List<int> categoryAndItsChildsIds = getIds(cmd);

            string cmdText;
            string parameterPrefix = "MemberId";
            if (category.Lft - moveBeforeCategory.Lft > 1)
            {
                cmd = new SQLiteCommand(connection);
                cmd.CommandText = @"SELECT Id FROM [Categories] WHERE Rgt < @Rgt AND Lft > @Lft";
                cmd.Parameters.AddWithValue("@Lft", moveBeforeCategory.Lft);
                cmd.Parameters.AddWithValue("@Rgt", category.Lft);
                // childs of 'moveBeforeCategory' that to the left of 'category'
                List<int> moveBeforeCategoryChildsIds = getIds(cmd);

                cmd = new SQLiteCommand(connection);
                cmdText = @"UPDATE [Categories]
                            SET Lft = Lft + @LftAndRgtIncrement,
                                Rgt = Rgt + @LftAndRgtIncrement
                            WHERE Id IN ({0})";
                cmd.CommandText = cmdText;
                cmdText = SQLiteWhereInParametersExtension.BuildWhereInClause(cmdText, parameterPrefix, moveBeforeCategoryChildsIds);
                cmd.CommandText = cmdText;
                cmd.AddParamsToCommand<int>(parameterPrefix, moveBeforeCategoryChildsIds);
                cmd.Parameters.AddWithValue("@LftAndRgtIncrement", lftAndRgtIncrement);
                cmd.ExecuteNonQuery();
            }

            cmd = new SQLiteCommand(connection);
            cmdText = @"UPDATE [Categories]
                        SET Lft = Lft - @LftAndRgtDecrement,
                            Rgt = Rgt - @LftAndRgtDecrement,
                            Level = Level + (@Level)
                        WHERE Id IN ({0})";
            cmdText = SQLiteWhereInParametersExtension.BuildWhereInClause(cmdText, parameterPrefix, categoryAndItsChildsIds);
            cmd.CommandText = cmdText;
            cmd.AddParamsToCommand<int>(parameterPrefix, categoryAndItsChildsIds);
            cmd.Parameters.AddWithValue("@LftAndRgtDecrement", lftAndRgtDecrement);
            cmd.Parameters.AddWithValue("@Level", moveBeforeCategory.Level - category.Level);
            cmd.ExecuteNonQuery();

            cmd = new SQLiteCommand(connection);
            cmd.CommandText = @"UPDATE [Categories] SET Lft = Lft + @LftAndRgtIncrement WHERE Name = @Name";
            cmd.Parameters.AddWithValue("@LftAndRgtIncrement", lftAndRgtIncrement);
            cmd.Parameters.AddWithValue("@Name", moveBeforeCategory.Name);
            cmd.ExecuteNonQuery();

            cmd = new SQLiteCommand(connection);
            cmd.CommandText = @"UPDATE [Categories] SET ParentId = @ParentId WHERE Name = @Name";
            cmd.Parameters.AddWithValue("@Name", category.Name);
            cmd.Parameters.AddWithValue("@ParentId", moveBeforeCategory.ParentId);
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

        public List<int> getIds(SQLiteCommand sqlCmd)
        {
            SQLiteDataReader reader = sqlCmd.ExecuteReader();
            List<int> ids = new List<int>();

            while (reader.Read())
            {
                ids.Add(reader.GetInt32(0));
            }
            reader.Close();
            return ids;
        }
    }
}
