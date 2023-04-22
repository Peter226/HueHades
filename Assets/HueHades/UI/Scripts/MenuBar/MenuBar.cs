using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuBar : HueHadesElement
{
    private const string ussMenuBar = "menu-bar";
    private Dictionary<IMenuBarElement, int> _elementsToAdd = new Dictionary<IMenuBarElement, int>();

    public MenuBar(HueHadesWindow window) : base(window)
    {
        AddToClassList(ussMenuBar);

        var types =
            from t in Assembly.GetExecutingAssembly().GetTypes().AsParallel()
            let attributes = t.GetCustomAttributes<MenuBarItemAttribute>(false)
            where attributes != null && attributes.Count() > 0
            select new { Type = t, Attribute = attributes.Cast<MenuBarItemAttribute>().Single() };

        Dictionary<string, MainCategory> mainCategories = new Dictionary<string, MainCategory>();

        foreach (var t in types)
        {
            //get type and attribute
            var classType = t.Type;
            var attribute = t.Attribute;

            //get necessary string parts
            var splitPath = attribute.categoryPath.Split('/');
            if (splitPath.Length <= 1)
            {
                var orderSplit = attribute.categoryPath.Split('_');
                var functionName = attribute.categoryPath;
                int order = int.MaxValue;
                if (orderSplit.Length > 1 && int.TryParse(orderSplit[1], out order))
                {
                    functionName = orderSplit[0];
                }

                var menuBarItem = new MenuBarItemButton(window, functionName, classType);
                _elementsToAdd.Add(menuBarItem, order);
                continue;
            }

            int separator = attribute.categoryPath.IndexOf('/');
            var leftoverPath = attribute.categoryPath.Substring(separator + 1, attribute.categoryPath.Length - (separator + 1));
            var categoryName = splitPath[0];
            var category = categoryName.Split('_');
            int categoryOrder = int.MaxValue;
            if (category.Length > 1 && int.TryParse(category[1], out categoryOrder))
            {
                categoryName = category[0];
            }

            //find or create category button
            MainCategory mainCategoryButton = null;
            mainCategories.TryGetValue(categoryName, out mainCategoryButton);
            if (mainCategoryButton == null)
            {
                mainCategoryButton = new MainCategory(window, categoryName);
                mainCategories.Add(categoryName, mainCategoryButton);
                _elementsToAdd.Add(mainCategoryButton, categoryOrder);
            }
            else
            {
                _elementsToAdd[mainCategoryButton] = Mathf.Min(categoryOrder, _elementsToAdd[mainCategoryButton]);
            }
            mainCategoryButton.AddFunction(leftoverPath, classType);

        }
        var sortedElements = _elementsToAdd.OrderBy((p) => { return p.Value; } );

        foreach (var (element, order) in sortedElements)
        {
            hierarchy.Add(element.Element);
            element.InitializeMenu();
        }
        _elementsToAdd.Clear();

    }
}
