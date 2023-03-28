using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class SubCategory : HueHadesElement
{
    private Dictionary<string, SubCategory> subCategories = new Dictionary<string, SubCategory>();
    private GroupBox _subCategoryGroupBox;
    private VisualElement _categoryListOverlay;
    private const string ussSubCategoryButton = "sub-category-button";
    private const string ussCategoryBoxParent = "category-box-parent";
    private const string ussCategoryLabel = "category-label";
    private const string ussSubCategoryArrow = "sub-category-arrow";
    public Action OnClose;
    private CategoryButton _categoryButton;

    public void AddFunction(string path, Type classType)
    {
        //get necessary string parts
        var splitPath = path.Split('/');
        if (splitPath.Length <= 1)
        {
            var menuBarItem = new MenuBarItemButton(window, path, classType);
            _subCategoryGroupBox.Add(menuBarItem);
            menuBarItem.LoseMouse += OnLoseMouse;
            return;
        }

        int separator = path.IndexOf('/');
        var leftoverPath = path.Substring(separator + 1, path.Length - (separator + 1));
        var category = splitPath[0];

        //find or create category button
        SubCategory subCategoryButton = null;
        subCategories.TryGetValue(category, out subCategoryButton);
        if (subCategoryButton == null)
        {
            subCategoryButton = new SubCategory(window, category);
            subCategoryButton.LoseMouse += OnLoseMouse;
            subCategories.Add(category, subCategoryButton);
            _subCategoryGroupBox.Add(subCategoryButton);
        }
        subCategoryButton.AddFunction(leftoverPath, classType);
    }

    public SubCategory(HueHadesWindow window, string name) : base(window)
    {
        _categoryButton = new CategoryButton(window);

        var label = new Label(name);
        var arrow = new Label(">");
        label.AddToClassList(ussCategoryLabel);
        arrow.AddToClassList(ussCategoryLabel);
        arrow.AddToClassList(ussSubCategoryArrow);

        _categoryButton.Add(label);
        _categoryButton.Add(arrow);
        hierarchy.Add(_categoryButton);
        _categoryButton.LoseMouse += OnLoseMouse;
        _categoryButton.clicked += ShowCategory;
        _categoryButton.GetMouse += ShowCategory;

        _categoryListOverlay = new VisualElement();
        _categoryListOverlay.AddToClassList(ussCategoryBoxParent);
        _subCategoryGroupBox = new GroupBox();
        _categoryListOverlay.Add(_subCategoryGroupBox);

        _categoryButton.AddToClassList(ussSubCategoryButton);
    }

    public Action<IEventHandler> LoseMouse;

    private void ShowCategory()
    {
        window.ShowOverlay(_categoryListOverlay, this, OverlayPlacement.Right);
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
        LoseMouse?.Invoke(eventHandler);
    }
}