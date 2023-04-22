using Codice.Client.BaseCommands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Video.VideoPlayer;

public class MainCategory : HueHadesElement, IMenuBarElement
{
    private Dictionary<string, SubCategory> subCategories = new Dictionary<string, SubCategory>();

    private GroupBox _subCategoryGroupBox;
    private VisualElement _categoryListOverlay;
    private const string ussMainCategoryButton = "main-category-button";
    private const string ussCategoryBoxParent = "category-box-parent";
    private CategoryButton _categoryButton;
    private Dictionary<IMenuBarElement, int> _elementsToAdd = new Dictionary<IMenuBarElement, int>();

    public VisualElement Element => this;

    public void AddFunction(string path, Type classType)
    {
        //get necessary string parts
        var splitPath = path.Split('/');
        if (splitPath.Length <= 1)
        {
            var orderSplit = path.Split('_');
            var functionName = path;
            int order = int.MaxValue;
            if (orderSplit.Length > 1 && int.TryParse(orderSplit[1], out order))
            {
                functionName = orderSplit[0];
            }

            var menuBarItem = new MenuBarItemButton(window, functionName, classType);
            _elementsToAdd.Add(menuBarItem, order);
            menuBarItem.LoseMouse += OnLoseMouse;
            return;
        }


        int separator = path.IndexOf('/');
        var leftoverPath = path.Substring(separator + 1, path.Length - (separator + 1));
        var categoryName = splitPath[0];
        var category = categoryName.Split('_');
        int categoryOrder = int.MaxValue;
        if (category.Length > 1 && int.TryParse(category[1], out categoryOrder))
        {
            categoryName = category[0];
        }

        //find or create category button
        SubCategory subCategoryButton = null;
        subCategories.TryGetValue(categoryName, out subCategoryButton);
        if (subCategoryButton == null)
        {
            subCategoryButton = new SubCategory(window, categoryName);
            subCategoryButton.LoseMouse += OnLoseMouse;
            subCategories.Add(categoryName, subCategoryButton);
            _elementsToAdd.Add(subCategoryButton, categoryOrder);
        }
        else
        {
            _elementsToAdd[subCategoryButton] = Mathf.Min(categoryOrder, _elementsToAdd[subCategoryButton]);
        }
        subCategoryButton.AddFunction(leftoverPath, classType);
    }


    public MainCategory(HueHadesWindow window, string name) : base(window)
    {
        _categoryButton = new CategoryButton(window);
        _categoryButton.text = name;
        hierarchy.Add(_categoryButton);
        _categoryButton.LoseMouse += OnLoseMouse;
        _categoryButton.GetMouse += ShowCategory;

        _categoryListOverlay = new VisualElement();
        _categoryListOverlay.AddToClassList(ussCategoryBoxParent);
        _subCategoryGroupBox = new GroupBox();
        _categoryListOverlay.Add(_subCategoryGroupBox);

        AddToClassList(ussMainCategoryButton);
    }

    private void ShowCategory()
    {
        window.ShowOverlay(_categoryListOverlay, this, OverlayPlacement.Bottom);
    }

    private void OnLoseMouse(IEventHandler eventHandler)
    {
        if (eventHandler == _categoryButton) return;
        foreach (var child in _subCategoryGroupBox.Children())
        {
            if (child == eventHandler || (child.childCount > 0 && child.Children().First() == eventHandler))
            {
                return;
            }
        }
        foreach (var (cname, category) in subCategories)
        {
            if (category == eventHandler || category.Children().First() == eventHandler)
            {
                return;
            }
        }
        window.HideOverlay(_categoryListOverlay);
    }

    public void InitializeMenu()
    {
        var sortedElements = _elementsToAdd.OrderBy((p) => { return p.Value; });
        foreach (var (element, order) in sortedElements)
        {
            _subCategoryGroupBox.Add(element.Element);
            element.InitializeMenu();
        }
        _elementsToAdd.Clear();
    }
}