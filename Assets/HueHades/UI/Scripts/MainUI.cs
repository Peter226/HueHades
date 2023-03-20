using HueHades.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using HueHades.UI;

[RequireComponent(typeof(UIDocument))]
public class MainUI : MonoBehaviour
{
    private UIDocument _uIDocument;
    private HueHadesWindow _window;
    public int2 initialCanvasDimensions = new int2(3508, 2480);

    private void Awake()
    {
        ApplicationManager.OnApplicationLoaded += ApplicationLoaded;
    }

    private void OnDestroy()
    {
        ApplicationManager.OnApplicationLoaded -= ApplicationLoaded;
    }

    private void ApplicationLoaded(object sender, ApplicationManager.LifeTimeEventArgs e)
    {
        _uIDocument = GetComponent<UIDocument>();
        _window = _uIDocument.rootVisualElement.Q<HueHadesWindow>();
        ApplicationManager.Instance.CreateCanvas(initialCanvasDimensions, Color.white, RenderTextureFormat.ARGBFloat);
        ApplicationManager.Instance.CreateCanvas(initialCanvasDimensions, Color.white, RenderTextureFormat.ARGBFloat);
        ApplicationManager.Instance.CreateCanvas(initialCanvasDimensions, Color.white, RenderTextureFormat.ARGBFloat);
        ApplicationManager.Instance.CreateCanvas(initialCanvasDimensions, Color.white, RenderTextureFormat.ARGBFloat);
        ApplicationManager.Instance.CreateCanvas(initialCanvasDimensions, Color.white, RenderTextureFormat.ARGBFloat);
    }
}

[MenuBarItem("Edit/Tools/Brush Editor")]
public class EditBrushMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("executed");
        BrushEditorWindow brushEditorWindow = new BrushEditorWindow(window);
        brushEditorWindow.Open();
    }
}


[MenuBarItem("Image/Mirror horizontal")]
public class MirrorHorizontalMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Mirror horizontal");
    }
}

[MenuBarItem("Image/Mirror vertical")]
public class MirrorVerticalMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Mirror vertical");
    }
}


[MenuBarItem("Image/Resize...")]
public class ResizeMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Resizing image");
    }
}

[MenuBarItem("Image/Resize canvas...")]
public class ResizeCanvasMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Resizing canvas");
    }
}


[MenuBarItem("Edit/Undo")]
public class UndoMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Undo");
    }
}

[MenuBarItem("Edit/Redo")]
public class RedoMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Redo");
    }
}


[MenuBarItem("File/Save")]
public class SaveMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Saving file");
    }
}

[MenuBarItem("File/Save as...")]
public class SaveAsMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Saving as file");
    }
}

public interface IMenuBarFunction
{
    public abstract void Execute(HueHadesWindow window);
}

public class MenuBarItemAttribute : Attribute
{
    public string categoryPath;
    public int orderInCategory;

    public MenuBarItemAttribute(string categoryPath, int orderInCategory = 0)
    {
        this.categoryPath = categoryPath;
        this.orderInCategory = orderInCategory;
    }

}

public enum OverlayPlacement
{
    Bottom,
    Right
}

public class HueHadesWindow : VisualElement
{
    VisualElement _popupElement;
    VisualElement _freeDockElement;
    VisualElement _popupWindowlement;
    private ToolsWindow _toolsWindow;
    public ToolsWindow ToolsWindow { get { return _toolsWindow; } }


    public VisualElement FreeDockElement
    {
        get
        {
            if (_freeDockElement == null)
            {
                _freeDockElement = this.Q<VisualElement>("FreeDock");
            }
            return _freeDockElement;
        }
    }


    public VisualElement PopupWindowParentElement
    {
        get
        {
            if (_popupWindowlement == null)
            {
                _popupWindowlement = this.Q<VisualElement>("PopupWindows");
            }
            return _popupWindowlement;
        }
    }



    private Dictionary<ImageCanvas, ImageOperatingWindow> _operatingWindows = new Dictionary<ImageCanvas, ImageOperatingWindow>();
    private DockingWindow _dockingWindow;

    public new class UxmlFactory : UxmlFactory<HueHadesWindow, UxmlTraits> { }

    

    public HueHadesWindow()
    {
        var menuBar = new MenuBar(this);
        hierarchy.Insert(0, menuBar);
        _dockingWindow = new DockingWindow(this, true);
        hierarchy.Insert(1, _dockingWindow);
        if (Application.isPlaying) ApplicationManager.OnCanvasCreated += OnCanvasCreated;
        
    }

    //if this throws an error, set script execution order to ApplicationManager run first
    private void OnCanvasCreated(object sender, ApplicationManager.CanvasChangeEventArgs args)
    {
        ImageOperatingWindow imageOperatingWindow = new ImageOperatingWindow(this, args.Canvas);
        imageOperatingWindow.Dock(_dockingWindow.Handle);

        if (_toolsWindow == null)
        {
            _toolsWindow = new ToolsWindow(this);
            _toolsWindow.Dock(_dockingWindow.Handle, DockType.Left);
        }

    }


    public void ShowOverlay(VisualElement overlay, VisualElement forElement = null, OverlayPlacement placement = OverlayPlacement.Bottom, bool isBackground = false)
    {
        if (_popupElement == null)
        {
            _popupElement = this.Q<VisualElement>("PopupOverlays");
            _popupElement.pickingMode = PickingMode.Ignore;
        }

        if (!isBackground)
        {
            _popupElement.Add(overlay);
        }
        else
        {
            _popupElement.Insert(0, overlay);
        }
        
        if (forElement != null)
        {
            Vector2 point;
            switch (placement)
            {
                case OverlayPlacement.Bottom:
                    point = _popupElement.WorldToLocal(forElement.LocalToWorld(new Vector2(0, forElement.layout.height)));
                    overlay.style.left = point.x;
                    overlay.style.top = point.y;
                    break;
                case OverlayPlacement.Right:
                    point = _popupElement.WorldToLocal(forElement.LocalToWorld(new Vector2(forElement.layout.width, 0)));
                    overlay.style.left = point.x;
                    overlay.style.top = point.y;
                    break;
            }
                
        }
    }
    public void HideOverlay(VisualElement overlay)
    {
        if (_popupElement == null)
        {
            _popupElement = this.Q<VisualElement>("PopupOverlays");
        }
        _popupElement.Remove(overlay);
    }
}


public abstract class MenuBarCategory
{
    public readonly int order;
    public readonly string name; 
        
    public MenuBarCategory(string name, int order)
    {
        this.name = name;
        this.order = order;
    }
}


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

public class HueHadesElement : VisualElement
{
    protected HueHadesWindow window;
    public HueHadesWindow HueHadesWindowIn { get { return window; }  }

    public HueHadesElement(HueHadesWindow window)
    {
        this.window = window;
    }
}
public class HueHadesButton : Button
{
    protected HueHadesWindow window;
    public HueHadesButton(HueHadesWindow window)
    {
        this.window = window;
    }
}


public class CategoryButton : HueHadesButton
{

    private const string ussCategoryButton = "category-button";

    public CategoryButton(HueHadesWindow window) : base(window)
    {
        this.RegisterCallback<FocusOutEvent>(FocusOutCallback);
        AddToClassList(ussCategoryButton);
    }

    public EventHandler HideCategoryEvent;

    private void FocusOutCallback(FocusOutEvent e)
    {
        HideCategoryEvent?.Invoke(this, new EventArgs());
    }

}


public class MainCategory : HueHadesElement
{
    private Dictionary<string, SubCategory> subCategories = new Dictionary<string, SubCategory>();

    private GroupBox _subCategoryGroupBox;
    private VisualElement _categoryListOverlay;
    private const string ussMainCategoryButton = "main-category-button";
    private const string ussCategoryBoxParent = "category-box-parent";

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


    public MainCategory(HueHadesWindow window, string name) : base(window)
    {
        var button = new CategoryButton(window);
        button.text = name;
        hierarchy.Add(button);
        button.HideCategoryEvent += HideCategory;
        button.clicked += ShowCategory;

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

    private void HideCategory(object sender, EventArgs e)
    {
        window.HideOverlay(_categoryListOverlay);
    }
}

public class SubCategory : HueHadesElement
{
    private Dictionary<string, SubCategory> subCategories = new Dictionary<string, SubCategory>();
    private GroupBox _subCategoryGroupBox;
    private VisualElement _categoryListOverlay;
    private const string ussSubCategoryButton = "sub-category-button";
    private const string ussCategoryBoxParent = "category-box-parent";

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
        button.text = name + "      >";
        hierarchy.Add(button);
        button.HideCategoryEvent += HideCategory;
        button.clicked += ShowCategory;

        _categoryListOverlay = new VisualElement();
        _categoryListOverlay.AddToClassList(ussCategoryBoxParent);
        _subCategoryGroupBox = new GroupBox();
        _categoryListOverlay.Add(_subCategoryGroupBox);

        AddToClassList(ussSubCategoryButton);
    }


    private void ShowCategory()
    {
        window.ShowOverlay(_categoryListOverlay);
    }

    private void HideCategory(object sender, EventArgs e)
    {
        window.HideOverlay(_categoryListOverlay);
    }
}

public class MenuBarItemButton : HueHadesButton
{
    private Type _menuBarFunction;
    private const string ussItemButton = "item-button";

    public MenuBarItemButton(HueHadesWindow window, string name, Type menuBarFunction) : base(window)
    {
        _menuBarFunction = menuBarFunction;
        text = name;
        AddToClassList(ussItemButton);
        clicked += () => {
            Debug.Log("clicked button");
            (menuBarFunction as IMenuBarFunction)?.Execute(window);
        };
    }
}