using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class MenuBar : HueHadesElement
{
    private const string ussMenuBar = "menu-bar";

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
                var menuBarItem = new MenuBarItemButton(window, attribute.categoryPath, classType);
                hierarchy.Add(menuBarItem);
                continue;
            }

            int separator = attribute.categoryPath.IndexOf('/');
            var leftoverPath = attribute.categoryPath.Substring(separator + 1, attribute.categoryPath.Length - (separator + 1));
            var category = splitPath[0];

            //find or create category button
            MainCategory mainCategoryButton = null;
            mainCategories.TryGetValue(category, out mainCategoryButton);
            if (mainCategoryButton == null)
            {
                mainCategoryButton = new MainCategory(window, category);
                mainCategories.Add(category, mainCategoryButton);
            }
            mainCategoryButton.AddFunction(leftoverPath, classType);

        }

        foreach (var (name, categoryButton) in mainCategories)
        {
            hierarchy.Add(categoryButton);
        }


    }
}
