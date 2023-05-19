﻿using NLog;
using System.Linq;
using NWConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;


// See https://aka.ms/new-console-template for more information
string path = Directory.GetCurrentDirectory() + "\\nlog.config";

// create instance of Logger
var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
logger.Info("Program started");

try
{
    var db = new NWContext();
    string choice;
    do
    {
        Console.WriteLine("1) Display Categories");
        Console.WriteLine("2) Add Category");
        Console.WriteLine("3) Display Category and related products");
        Console.WriteLine("4) Display all Categories and their related products");
        Console.WriteLine("5) Add a new Product.");
        Console.WriteLine("\"q\" to quit");
        choice = Console.ReadLine();
        Console.Clear();
         logger.Info($"Option {choice} selected");
        if (choice == "1")
        {
            var query = db.Categories.OrderBy(p => p.CategoryName);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName} - {item.Description}");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
        else if (choice == "2")
        {
            Category category = new Category();
            Console.WriteLine("Enter Category Name:");
            category.CategoryName = Console.ReadLine();
            Console.WriteLine("Enter the Category Description:");
            category.Description = Console.ReadLine();
            ValidationContext context = new ValidationContext(category, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(category, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                    db.AddCategory(category);
                    logger.Info("Category added - {name}", category.CategoryName);
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }
        }
         else if (choice == "3")
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);

            Console.WriteLine("Select the category whose products you want to display:");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            int id = int.Parse(Console.ReadLine());
            Console.Clear();
            logger.Info($"CategoryId {id} selected");
            Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
            //Category category = db.Categories.FirstOrDefault(c => c.CategoryId == id);
            Console.WriteLine($"{category.CategoryName} - {category.Description}");
            foreach (Product p in category.Products)
            {
                //Console.WriteLine(p.ProductName);
                Console.WriteLine($"\t{p.ProductName}");
            }
            
        }
        else if (choice == "4")
        {
            var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName}");
                foreach (Product p in item.Products)
                {
                    Console.WriteLine($"\t{p.ProductName}");
                }
            }
        }
        else if (choice == "5")
        {
            Product product = new Product();
            Console.WriteLine("Select Category for Product:");
            var category = GetCategory(db, logger);
            if(category != null){
            product.Category = category;
            product.CategoryId = category.CategoryId;
            Console.WriteLine("Select Supplier of Product:");
                var supplier = GetSupplier(db, logger);
                if(supplier != null){
            product.Supplier = supplier;
            product.SupplierId = supplier.SupplierId;
            Console.WriteLine("Enter Product Name:");
            product.ProductName = Console.ReadLine();
            Console.WriteLine("Enter quantity per unit:");
            product.QuantityPerUnit = Console.ReadLine();
            Console.WriteLine("Enter Unit Price:");
            product.UnitPrice = Convert.ToDecimal(Console.ReadLine());
            Console.WriteLine("Enter current Stock:");
            product.UnitsInStock = Convert.ToInt16(Console.ReadLine());
            Console.WriteLine("Enter Units on Order:");
            product.UnitsOnOrder = Convert.ToInt16(Console.ReadLine());
            Console.WriteLine("Enter Reorder Level:");
            product.ReorderLevel = Convert.ToInt16(Console.ReadLine());
            Console.WriteLine("Is this Discontinued? (y/n):");
            var discAnswer = Console.ReadLine();
            if (discAnswer == "y"){
                product.Discontinued = true;
            }
            else if (discAnswer == "n"){
                product.Discontinued = false;
            }
            else{
                logger.Error("Input is invalid");
            }

            ValidationContext context = new ValidationContext(product, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(product, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Products.Any(p => p.ProductName == product.ProductName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "ProductName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                    db.AddProduct(product);
                }
            
        }
                }
            }
      
        } 

          Console.WriteLine();
}while (choice.ToLower() != "q");
}

catch (Exception ex)
{
    logger.Error(ex.Message);
}

logger.Info("Program ended");

static Category GetCategory(NWContext db, Logger logger)
{
    // display all categories
    var categories = db.Categories.OrderBy(b => b.CategoryId);
    foreach (Category c in categories)
    {
        Console.WriteLine($"{c.CategoryId}: {c.CategoryName}");
    }
    if (int.TryParse(Console.ReadLine(), out int BlogId))
    {
        Category category = db.Categories.FirstOrDefault(c => c.CategoryId == BlogId);
        if (category != null)
        {
            return category;
        }
    }
    logger.Error("Invalid Category ID");
    return null;
}
static Supplier GetSupplier(NWContext db, Logger logger)
{
    // display all suppliers
    var suppliers = db.Suppliers.OrderBy(b => b.SupplierId);
    foreach (Supplier s in suppliers)
    {
        Console.WriteLine($"{s.SupplierId}: {s.SupplierId}");
    }
    if (int.TryParse(Console.ReadLine(), out int SupplierId))
    {
        Supplier supplier = db.Suppliers.FirstOrDefault(c => c.SupplierId == SupplierId);
        if (supplier != null)
        {
            return supplier;
        }
    }
    logger.Error("Invalid Supplier ID");
    return null;
}