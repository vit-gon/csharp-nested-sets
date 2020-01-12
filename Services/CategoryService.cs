using DataStructureTest.Models;
using DataStructureTest.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataStructureTest.Services
{
    class CategoryService
    {
        public CategoryRepository categoryRepository;

        public CategoryService()
        {
            categoryRepository = new CategoryRepository();
        }

        public void AddRootNode()
        {
            Category category = new Category()
            {
                Lft = 1,
                Rgt = 2,
                Level = 1,
                ParentId = 0,
                Name = "root"
            };
            categoryRepository.InsertCategory(category);
        }

        public Category FindByName(string name)
        {
            return categoryRepository.FindByName(name);
        }

        public List<Category> FindAll()
        {
            return categoryRepository.FindAll();
        }

        public void InsertBefore(Category category, string name)
        {
            categoryRepository.InsertBefore(category, name);
        }

        internal void MoveBefore(Category moveBeforeCategory, Category category)
        {
            categoryRepository.MoveBefore(moveBeforeCategory, category);
        }

        public void PrependTo(Category category, string name)
        {
            categoryRepository.PrependTo(category, name);
        }

        public void DeleteLeafCategory(Category category)
        {
            categoryRepository.DeleteLeafCategory(category);
        }

        public void DeleteNode(Category category)
        {
            if (IsLeaf(category))
            {
                categoryRepository.DeleteLeafCategory(category);
            }
            else
            {
                categoryRepository.DeleteCategoryThatHasDescendants(category);
            }
        }

        public bool IsLeaf(Category category)
        {
            return category.Rgt - category.Lft == 1;
        }

        public void MoveAfter(Category moveAfterCategory, Category category)
        {
            categoryRepository.MoveAfter(moveAfterCategory, category);
        }
    }
}
