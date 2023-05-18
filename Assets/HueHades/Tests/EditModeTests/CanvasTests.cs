using HueHades.Core;
using Unity.Mathematics;
using NUnit.Framework;
using UnityEngine;
using HueHades.Utilities;

public class CanvasTests
{

    [OneTimeSetUp]
    public void InitStatic()
    { 
        RenderTextureUtilities.Initialize();
        RenderTextureUtilities.InitializePool();
    }

    [OneTimeTearDown]
    public void CleanupStatic()
    {
        RenderTextureUtilities.Dispose();
    }

    /// <summary>
    /// Test if canvas creation is working
    /// </summary>
    [Test]
    public void Canvas_Create()
    {
        ImageCanvas imageCanvas = new ImageCanvas(new int2(1024,1024), RenderTextureFormat.ARGBFloat, Color.white);
        Assert.IsNotNull(imageCanvas, "The canvas couldn't be created");
        Assert.IsTrue(imageCanvas.PreviewTexture.texture.IsCreated(), "The preview texture of the canvas was not created");
        imageCanvas.Dispose();
    }

    /// <summary>
    /// Test if canvas releases it's resources when requested
    /// </summary>
    [Test]
    public void Canvas_Release()
    {
        ImageCanvas imageCanvas = new ImageCanvas(new int2(1024, 1024), RenderTextureFormat.ARGBFloat, Color.white);
        imageCanvas.Dispose();
        Assert.IsFalse(imageCanvas.PreviewTexture.texture.IsCreated(), "The canvas's preview wasn't disposed of");
    }

    public class HistoryTests
    {

        [OneTimeSetUp]
        public void InitStatic()
        {
            RenderTextureUtilities.Initialize();
            RenderTextureUtilities.InitializePool();
        }

        [OneTimeTearDown]
        public void CleanupStatic()
        {
            RenderTextureUtilities.Dispose();
        }

        private ImageCanvas _imageCanvas;
        [SetUp]
        public void Init()
        {
            _imageCanvas = new ImageCanvas(new int2(1024, 512), RenderTextureFormat.ARGBFloat, Color.white);
        }

        [TearDown]
        public void Cleanup()
        {
            _imageCanvas.Dispose();
        }

        /// <summary>
        /// Test if Undo works (for adding layers)
        /// </summary>
        [Test]
        public void AddLayer_Undo()
        {
            var layer = _imageCanvas.AddLayer(0,1, Color.white);
            _imageCanvas.History.AddRecord(new NewLayerHistoryRecord(0, 1, layer.GlobalIndex, Color.white));
            _imageCanvas.History.Undo();
            Assert.AreEqual(1, _imageCanvas.Layers.Count, "Undo was unsuccessful, expected layer count does not match");
        }

        /// <summary>
        /// Test if Redo works (for adding layers)
        /// </summary>
        [Test]
        public void AddLayer_Redo()
        {
            var layer = _imageCanvas.AddLayer(0, 1, Color.white);
            _imageCanvas.History.AddRecord(new NewLayerHistoryRecord(0, 1, layer.GlobalIndex, Color.white));
            _imageCanvas.History.Undo();
            _imageCanvas.History.Redo();
            Assert.AreEqual(2, _imageCanvas.Layers.Count, "Redo was unsuccessful, expected layer count does not match");
        }
    }



    public class SelectionTests
    {

        [OneTimeSetUp]
        public void InitStatic()
        {
            RenderTextureUtilities.Initialize();
            RenderTextureUtilities.InitializePool();
        }

        [OneTimeTearDown]
        public void CleanupStatic()
        {
            RenderTextureUtilities.Dispose();
        }


        private ImageCanvas _imageCanvas;
        [SetUp]
        public void Init()
        {
            _imageCanvas = new ImageCanvas(new int2(1024, 1024), RenderTextureFormat.ARGBFloat, Color.white);
        }

        [TearDown]
        public void Cleanup()
        {
            _imageCanvas.Dispose();
        }


        /// <summary>
        /// Tests if the selection is initialized
        /// </summary>
        [Test]
        public void Canvas_Selection_Create()
        {
            Assert.IsNotNull(_imageCanvas.Selection, "The selection of the canvas was not created");
            Assert.IsTrue(_imageCanvas.Selection.SelectionTexture.texture.IsCreated(), "The selection's texture was not created");
        }

        /// <summary>
        /// Tests if the initialized selection is empty
        /// </summary>
        [Test]
        public void Selection_Area_Empty()
        {
            Assert.Zero(_imageCanvas.Selection.SelectedArea, "The selection wasn't empty");
        }

        /// <summary>
        /// Tests if we are able to draw a rectangle on the selection, and the associated selection stats are correct
        /// </summary>
        [Test]
        public void Selection_Area_Rectangle()
        {
            var selectionBuffer = RenderTextureUtilities.GetTemporary(_imageCanvas.Dimensions.x, _imageCanvas.Dimensions.y, _imageCanvas.Format);
            RenderTextureUtilities.ClearTexture(selectionBuffer, Color.clear);
            RenderTextureUtilities.Selection.DrawRectangle(selectionBuffer, new Vector2(8.5f, 8.5f), new Vector2(2, 2));
            var baseBuffer = RenderTextureUtilities.GetTemporary(_imageCanvas.Dimensions.x, _imageCanvas.Dimensions.y, _imageCanvas.Format);
            RenderTextureUtilities.ClearTexture(baseBuffer, Color.clear);

            RenderTextureUtilities.Selection.LayerSelectionArea(Vector2.zero, baseBuffer, _imageCanvas.Selection.SelectionTexture, 0, 0, 500, 500, selectionBuffer, SelectMode.Fresh);

            _imageCanvas.Selection.SetDirty();

            RenderTextureUtilities.ReleaseTemporary(baseBuffer);
            RenderTextureUtilities.ReleaseTemporary(selectionBuffer);

            Assert.NotZero(_imageCanvas.Selection.SelectedArea, "Nothing was selected");
            Assert.AreEqual(16, _imageCanvas.Selection.SelectedArea, $"The selection area ({_imageCanvas.Selection.SelectedArea}) does not match required size (16)");
            Assert.AreEqual(new int4(7, 7, 10, 10), _imageCanvas.Selection.SelectedAreaBounds, $"The selection area bounds ({_imageCanvas.Selection.SelectedAreaBounds}) does not match the expected bounds (7,7,10,10)");
        }
    }



    public class LayerTests
    {

        [OneTimeSetUp]
        public void InitStatic()
        {
            RenderTextureUtilities.Initialize();
            RenderTextureUtilities.InitializePool();
        }

        [OneTimeTearDown]
        public void CleanupStatic()
        {
            RenderTextureUtilities.Dispose();
        }


        private ImageCanvas _imageCanvas;

        [SetUp]
        public void Init()
        {
            _imageCanvas = new ImageCanvas(new int2(1024, 1024), RenderTextureFormat.ARGBFloat, Color.white);
        }

        [TearDown]
        public void Cleanup()
        {
            _imageCanvas.Dispose();
        }

        /// <summary>
        /// Tests if the created canvas has a layer initialized
        /// </summary>
        [Test]
        public void Canvas_Layer_Create()
        {
            var imageCanvas = new ImageCanvas(new int2(1024, 1024), RenderTextureFormat.ARGBFloat, Color.white);
            Assert.Greater(imageCanvas.Layers.Count, 0, "The first layer was not added");
            Assert.IsNotNull(imageCanvas.Layers[0], "The canvas did not create the layer");
            Assert.IsTrue(imageCanvas.Layers[0].Texture.texture.IsCreated(), "The texture of the layer wasn't created");
            imageCanvas.Dispose();
        }

        /// <summary>
        /// Tests if the canvas releases it's layers when disposed of
        /// </summary>
        [Test]
        public void Canvas_Layer_Release()
        {
            var imageCanvas = new ImageCanvas(new int2(1024, 1024), RenderTextureFormat.ARGBFloat, Color.white);
            var layer = imageCanvas.Layers[0];
            imageCanvas.Dispose();
            Assert.IsFalse(layer.Texture.texture.IsCreated(), "The layer's texture wasn't disposed");
        }

        /// <summary>
        /// Tests if we are able to add additional layers by specifying color
        /// </summary>
        [Test]
        public void Layer_Add()
        {
            var layer = _imageCanvas.AddLayer(0, 1, Color.clear);
            Assert.Greater(_imageCanvas.Layers.Count, 1, "The new layer was not added");
            Assert.IsTrue(_imageCanvas.Layers[1] == layer, "The layer wasn't added in the correct spot");
        }

        /// <summary>
        /// Tests if we can manually add a layer
        /// </summary>
        [Test]
        public void Layer_Manual_Add()
        {
            ImageLayer imageLayer = new ImageLayer(_imageCanvas.Dimensions, _imageCanvas.Format, Color.clear);
            _imageCanvas.AddLayer(imageLayer, 0, 1);
            Assert.Greater(_imageCanvas.Layers.Count, 1, "The new layer was not added");
            Assert.IsTrue(_imageCanvas.Layers[1] == imageLayer, "The layer wasn't added in the correct spot");
        }

        /// <summary>
        /// Tests if moving layers behaves as expected
        /// </summary>
        [Test]
        public void Layer_Move()
        {
            ImageLayer imageLayer = new ImageLayer(_imageCanvas.Dimensions, _imageCanvas.Format, Color.clear);
            _imageCanvas.AddLayer(imageLayer, 0, 1);
            _imageCanvas.MoveLayer(2,0,0);
            Assert.IsTrue(_imageCanvas.Layers[0] == imageLayer, "The layer wasn't moved as expected");
        }

        /// <summary>
        /// Tests if removing layers behaves as expected
        /// </summary>
        [Test]
        public void Layer_Remove()
        {
            ImageLayer imageLayer = new ImageLayer(_imageCanvas.Dimensions, _imageCanvas.Format, Color.clear);
            _imageCanvas.AddLayer(imageLayer, 0, 1);
            _imageCanvas.RemoveLayer(1);
            Assert.Less(_imageCanvas.Layers.Count, 2, "The layer wasn't removed");
            Assert.IsTrue(_imageCanvas.Layers[0] == imageLayer, "The incorrect layer was removed");
        }
    }



}
