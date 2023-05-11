using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuBar : HueHadesElement
{
    private const string ussMenuBar = "menu-bar";
    private const string ussMenuBarLeftConatiner = "menu-bar-left-container";
    private const string ussMenuBarButtons = "menu-bar-buttons";
    private const string ussMenuBarQuickAccess = "menu-bar-quick-access";
    private const string ussMenuBarCanvases = "menu-bar-canvases";
    private Dictionary<IMenuBarElement, int> _elementsToAdd = new Dictionary<IMenuBarElement, int>();

    private VisualElement _leftContainer;
    private VisualElement _buttonsContainer;
    private VisualElement _quickAccessContainer;
    private VisualElement _canvasesContainer;

    public MenuBar(HueHadesWindow window) : base(window)
    {
        AddToClassList(ussMenuBar);

        _leftContainer = new VisualElement();
        _leftContainer.AddToClassList(ussMenuBarLeftConatiner);
        hierarchy.Add(_leftContainer);

        _canvasesContainer = new VisualElement();
        _canvasesContainer.AddToClassList(ussMenuBarCanvases);
        hierarchy.Add(_canvasesContainer);

        _buttonsContainer = new VisualElement();
        _buttonsContainer.AddToClassList(ussMenuBarButtons);
        _leftContainer.Add(_buttonsContainer);

        _quickAccessContainer = new VisualElement();
        _quickAccessContainer.AddToClassList(ussMenuBarQuickAccess);
        _leftContainer.Add(_quickAccessContainer);

        var types =
            from t in Assembly.GetExecutingAssembly().GetTypes().AsParallel()
            let attributes = t.GetCustomAttributes<MenuBarItemAttribute>(false)
            where attributes != null && attributes.Count() > 0
            select new { Type = t, Attribute = attributes.Cast<MenuBarItemAttribute>().Single() };

        Dictionary<string, MainCategory> mainCategories = new Dictionary<string, MainCategory>();
        List<(Type, MenuBarItemAttribute)> quickAccessTypes = new List<(Type, MenuBarItemAttribute)>();

        foreach (var t in types)
        {
            //get type and attribute
            var classType = t.Type;
            var attribute = t.Attribute;

            if (t.Attribute.quickAccess)
            {
                quickAccessTypes.Add((t.Type,t.Attribute));
            }

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

                var menuBarItem = new MenuBarItemButton(window, functionName, attribute, classType);
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
            mainCategoryButton.AddFunction(leftoverPath, classType, attribute);

        }
        var sortedElements = _elementsToAdd.OrderBy((p) => { return p.Value; } );

        foreach (var (element, order) in sortedElements)
        {
            _buttonsContainer.Add(element.Element);
            element.InitializeMenu();
        }
        _elementsToAdd.Clear();

        var quicks = quickAccessTypes.OrderBy((q) => q.Item2.categoryPath);

        foreach (var (type, attribute) in quicks)
        {
            _quickAccessContainer.Add(new QuickAccessButton(window, attribute, type));
        }

        quickAccessTypes.Clear();


        ApplicationManager.CanvasCreated += RedrawCanvasIcons;
        ApplicationManager.CanvasClosed += RedrawCanvasIcons;
    }

    /// <summary>
    /// When canvas count changes in the Application, reload all canvases
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RedrawCanvasIcons(object sender, ApplicationManager.CanvasChangeEventArgs e)
    {
        _canvasesContainer.Clear();


        foreach (var canvas in ApplicationManager.Instance.GetCanvases())
        {
            var canvasElement = new MenuCanvasItem(window, canvas);

            _canvasesContainer.Add(canvasElement);

        }
    }
}
