using Codice.CM.SEIDInfo;
using System;
using System.Collections;
using System.Collections.Generic;
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

    public void AddFunction(string path, Type classType)
    {
        //get necessary string parts
        var splitPath = path.Split('/');
        if (splitPath.Length <= 1)
        {
            var menuBarItem = new MenuBarItemButton(window, path, classType);
            _subCategoryGroupBox.Add(menuBarItem);
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
            subCategories.Add(category, subCategoryButton);
            _subCategoryGroupBox.Add(subCategoryButton);
        }
        subCategoryButton.AddFunction(leftoverPath, classType);
    }

    public SubCategory(HueHadesWindow window, string name) : base(window)
    {
        var button = new CategoryButton(window);

        var label = new Label(name);
        var arrow = new Label(">");
        label.AddToClassList(ussCategoryLabel);
        arrow.AddToClassList(ussCategoryLabel);
        arrow.AddToClassList(ussSubCategoryArrow);

        button.Add(label);
        button.Add(arrow);
        hierarchy.Add(button);
        button.HideCategoryEvent += HideCategory;
        button.clicked += ShowCategory;
        button.RegisterCallback<MouseOverEvent>(OnMouseOverButton);


        _categoryListOverlay = new VisualElement();
        _categoryListOverlay.AddToClassList(ussCategoryBoxParent);
        _subCategoryGroupBox = new GroupBox();
        _categoryListOverlay.Add(_subCategoryGroupBox);

        button.AddToClassList(ussSubCategoryButton);
    }

    private void OnMouseOverButton(MouseOverEvent mouseOver)
    {
        ShowCategory();
    }




    private void ShowCategory()
    {
        window.ShowOverlay(_categoryListOverlay, this, OverlayPlacement.Right);
    }

    private void HideCategory(object sender, EventArgs e)
    {
        window.HideOverlay(_categoryListOverlay);
    }
}