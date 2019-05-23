using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.ComponentModel;

namespace DrawToolsLib
{
    /// <summary>
    /// Canvas used as host for DrawingVisual objects.
    /// Allows to draw graphics objects using mouse.
    /// </summary>
    public class DrawingCanvas : Canvas
    {
        #region Class Members

        // Collection contains instances of GraphicsBase-derived classes.
        private VisualCollection graphicsList;
        public bool IsMouseMoveOnGraphics;

        // Dependency properties
        public static readonly DependencyProperty LayerProperty;
        public static readonly DependencyProperty PageProperty;

        public static readonly DependencyProperty ToolProperty;
        public static readonly DependencyProperty ActualScaleProperty;
        public static readonly DependencyProperty IsDirtyProperty;

        public static readonly DependencyProperty LineWidthProperty;
        public static readonly DependencyProperty ObjectColorProperty;

        public static readonly DependencyProperty TextFontFamilyNameProperty;
        public static readonly DependencyProperty TextFontStyleProperty;
        public static readonly DependencyProperty TextFontWeightProperty;
        public static readonly DependencyProperty TextFontStretchProperty;
        public static readonly DependencyProperty TextFontSizeProperty;

        public static readonly DependencyProperty CanUndoProperty;
        public static readonly DependencyProperty CanRedoProperty;

        public static readonly DependencyProperty CanSelectAllProperty;
        public static readonly DependencyProperty CanUnselectAllProperty;
        public static readonly DependencyProperty CanDeleteProperty;
        public static readonly DependencyProperty CanDeleteAllProperty;
        public static readonly DependencyProperty CanMoveToFrontProperty;
        public static readonly DependencyProperty CanMoveToBackProperty;
        public static readonly DependencyProperty CanModifyGraphicsTextProperty;
        public static readonly DependencyProperty CanSetPropertiesProperty;

        public static readonly RoutedEvent IsDirtyChangedEvent;

        private Tool[] tools;                   // Array of tools
     
        ToolText toolText;
        ToolPointer toolPointer;

        private ContextMenu contextMenu;
        private UndoManager undoManager;

        #endregion Class Members
     
        #region Constructors

        public DrawingCanvas()
            : base()
        {
            graphicsList = new VisualCollection(this);
            
            CreateContextMenu();

            // create array of drawing tools
            tools = new Tool[(int)ToolType.Max];

            toolPointer = new ToolPointer();
            tools[(int)ToolType.Pointer] = toolPointer;

            tools[(int)ToolType.Rectangle] = new ToolRectangle();
            tools[(int)ToolType.Ellipse] = new ToolEllipse();
            tools[(int)ToolType.Line] = new ToolLine();
            tools[(int)ToolType.Arrow] = new ToolArrow();
            tools[(int)ToolType.Ruler] = new ToolRuler();
            tools[(int)ToolType.PolyLine] = new ToolPolyLine();

            toolText = new ToolText(this);
            tools[(int)ToolType.Text] = toolText;   // kept as class member for in-place editing

            // Create undo manager
            undoManager = new UndoManager(this);
            undoManager.StateChanged += new EventHandler(undoManager_StateChanged);
            
            this.FocusVisualStyle = null;

            this.Loaded += new RoutedEventHandler(DrawingCanvas_Loaded);
            this.MouseDown += new MouseButtonEventHandler(DrawingCanvas_MouseDown);            
            this.MouseMove += new MouseEventHandler(DrawingCanvas_MouseMove);
            this.MouseUp += new MouseButtonEventHandler(DrawingCanvas_MouseUp);
            this.KeyDown += new KeyEventHandler(DrawingCanvas_KeyDown);
            this.LostMouseCapture += new MouseEventHandler(DrawingCanvas_LostMouseCapture);
        }

        public void SetTransform(TransformGroup tsGroup, TranslateTransform _transTf, ScaleTransform _scaleTf)
        {
            _scale = _scaleTf;
            _translate = _transTf;
            tsGroup.Children.Add(_scale);
            tsGroup.Children.Add(_translate);
            this.RenderTransform = tsGroup;

            _transTf.Changed += new EventHandler(OnTranslateChanged);
            _scaleTf.Changed += new EventHandler(OnScaleChanged);
        }
        
        static DrawingCanvas()
        {
            // **********************************************************
            // Create dependency properties
            // **********************************************************

            PropertyMetadata metaData;

            // Tool
            metaData = new PropertyMetadata(ToolType.Pointer);

            ToolProperty = DependencyProperty.Register(
                "Tool", typeof(ToolType), typeof(DrawingCanvas),
                metaData);


            // ActualScale
            metaData = new PropertyMetadata(
                1.0,                                                        // default value
                new PropertyChangedCallback(ActualScaleChanged));           // change callback

            ActualScaleProperty = DependencyProperty.Register(
                "ActualScale", typeof(double), typeof(DrawingCanvas),
                metaData);

            
            // Layer
            metaData = new PropertyMetadata(
                0,                                                        // default value
                new PropertyChangedCallback(LayerChanged));           // change callback

            LayerProperty = DependencyProperty.Register(
                "Layer", typeof(int), typeof(DrawingCanvas),
                metaData);

            // Page
            metaData = new PropertyMetadata(
                0,                                                        // default value
                new PropertyChangedCallback(PageChanged));           // change callback

            PageProperty = DependencyProperty.Register(
                "Page", typeof(int), typeof(DrawingCanvas),
                metaData);


            // IsDirty
            metaData = new PropertyMetadata(false);

            IsDirtyProperty = DependencyProperty.Register(
                "IsDirty", typeof(bool), typeof(DrawingCanvas),
                metaData);

            // LineWidth
            metaData = new PropertyMetadata(
                1.0,
                new PropertyChangedCallback(LineWidthChanged));

            LineWidthProperty = DependencyProperty.Register(
                "LineWidth", typeof(double), typeof(DrawingCanvas),
                metaData);

            // ObjectColor
            metaData = new PropertyMetadata(
                Colors.Black,
                new PropertyChangedCallback(ObjectColorChanged));

            ObjectColorProperty = DependencyProperty.Register(
                "ObjectColor", typeof(Color), typeof(DrawingCanvas),
                metaData);


            // TextFontFamilyName
            metaData = new PropertyMetadata(
                Properties.Settings.Default.DefaultFontFamily,
                new PropertyChangedCallback(TextFontFamilyNameChanged));

            TextFontFamilyNameProperty = DependencyProperty.Register(
                "TextFontFamilyName", typeof(string), typeof(DrawingCanvas),
                metaData);

            // TextFontStyle
            metaData = new PropertyMetadata(
                FontStyles.Normal,
                new PropertyChangedCallback(TextFontStyleChanged));

            TextFontStyleProperty = DependencyProperty.Register(
                "TextFontStyle", typeof(FontStyle), typeof(DrawingCanvas),
                metaData);

            // TextFontWeight
            metaData = new PropertyMetadata(
                FontWeights.Normal,
                new PropertyChangedCallback(TextFontWeightChanged));

            TextFontWeightProperty = DependencyProperty.Register(
                "TextFontWeight", typeof(FontWeight), typeof(DrawingCanvas),
                metaData);

            // TextFontStretch
            metaData = new PropertyMetadata(
                FontStretches.Normal,
                new PropertyChangedCallback(TextFontStretchChanged));

            TextFontStretchProperty = DependencyProperty.Register(
                "TextFontStretch", typeof(FontStretch), typeof(DrawingCanvas),
                metaData);

            // TextFontSize
            metaData = new PropertyMetadata(
                12.0,
                new PropertyChangedCallback(TextFontSizeChanged));

            TextFontSizeProperty = DependencyProperty.Register(
                "TextFontSize", typeof(double), typeof(DrawingCanvas),
                metaData);

            // CanUndo
            metaData = new PropertyMetadata(false);

            CanUndoProperty = DependencyProperty.Register(
                "CanUndo", typeof(bool), typeof(DrawingCanvas),
                metaData);

            // CanRedo
            metaData = new PropertyMetadata(false);

            CanRedoProperty = DependencyProperty.Register(
                "CanRedo", typeof(bool), typeof(DrawingCanvas),
                metaData);

            // CanSelectAll
            metaData = new PropertyMetadata(false);

            CanSelectAllProperty = DependencyProperty.Register(
                "CanSelectAll", typeof(bool), typeof(DrawingCanvas),
                metaData);

            // CanUnselectAll
            metaData = new PropertyMetadata(false);

            CanUnselectAllProperty = DependencyProperty.Register(
                "CanUnselectAll", typeof(bool), typeof(DrawingCanvas),
                metaData);

            // CanDelete
            metaData = new PropertyMetadata(false);

            CanDeleteProperty = DependencyProperty.Register(
                "CanDelete", typeof(bool), typeof(DrawingCanvas),
                metaData);

            // CanDeleteAll
            metaData = new PropertyMetadata(false);

            CanDeleteAllProperty = DependencyProperty.Register(
                "CanDeleteAll", typeof(bool), typeof(DrawingCanvas),
                metaData);

            // CanMoveToFront
            metaData = new PropertyMetadata(false);

            CanMoveToFrontProperty = DependencyProperty.Register(
                "CanMoveToFront", typeof(bool), typeof(DrawingCanvas),
                metaData);

            // CanMoveToBack
            metaData = new PropertyMetadata(false);

            CanMoveToBackProperty = DependencyProperty.Register(
                "CanMoveToBack", typeof(bool), typeof(DrawingCanvas),
                metaData);

            // CanSetProperties
            metaData = new PropertyMetadata(false);

            CanSetPropertiesProperty = DependencyProperty.Register(
                "CanSetProperties", typeof(bool), typeof(DrawingCanvas),
                metaData);

            // CanModifyGraphicsText
            metaData = new PropertyMetadata(false);

            CanModifyGraphicsTextProperty = DependencyProperty.Register(
                "CanModifyGraphicsText", typeof(bool), typeof(DrawingCanvas),
                metaData);

            // **********************************************************
            // Create routed events
            // **********************************************************

            // IsDirtyChanged

            IsDirtyChangedEvent = EventManager.RegisterRoutedEvent("IsDirtyChangedChanged",
                RoutingStrategy.Bubble, typeof(DependencyPropertyChangedEventHandler), typeof(DrawingCanvas));

        }

        #endregion Constructor

        #region Dependency Properties

        #region Tool

        /// <summary>
        /// Currently active drawing tool
        /// </summary>
        public ToolType Tool
        {
            get
            {
                return (ToolType)GetValue(ToolProperty);
            }
            set
            {
                if ((int)value >= 0 && (int)value < (int)ToolType.Max)
                {
                    SetValue(ToolProperty, value);

                    if (value != ToolType.None)
                    {
                        // Set cursor immediately - important when tool is selected from the menu
                        tools[(int)Tool].SetCursor(this);
                    }
                }
            }
        }

        #endregion Tool

        #region ActualScale-Layer-Page

        /// <summary>
        /// Dependency property ActualScale.
        /// </summary>
        public double ActualScale
        {
            get
            {
                return (double)GetValue(ActualScaleProperty);
            }
            set
            {
                SetValue(ActualScaleProperty, value);
            }
        }

        /// <summary>
        /// Dependency property Layer.
        /// </summary>
        public int Layer
        {
            get
            {
                return (int)GetValue(LayerProperty);
            }
            set
            {
                SetValue(LayerProperty, value < 0 ? 0 : value);
            }
        }

        /// <summary>
        /// Dependency property Page.
        /// </summary>
        public int Page
        {
            get
            {
                return (int)GetValue(PageProperty);
            }
            set
            {
                SetValue(PageProperty, value < 0 ? 0 : value);    
            }
        }


        /// <summary>
        /// Callback function called when ActualScale dependency property is changed.
        /// </summary>
        static void ActualScaleChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            DrawingCanvas d = property as DrawingCanvas;

            double scale = d.ActualScale;

            foreach (GraphicsBase b in d.GraphicsList)
            {               
                  b.ActualScale = scale;
            }
        }


        /// <summary>
        /// Callback function called when Layer dependency property is changed.
        /// </summary>
        static void LayerChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            DrawingCanvas d = property as DrawingCanvas;

            int layer = d.Layer;

            foreach (GraphicsBase b in d.GraphicsList)
            {
         //       b.Layer = layer;
            }
        }

        /// <summary>
        /// Callback function called when Page dependency property is changed.
        /// </summary>
        static void PageChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            DrawingCanvas d = property as DrawingCanvas;

            int page = d.Page;

            foreach (GraphicsBase b in d.GraphicsList)
            {
                b.Rate = d.GetDrawRate(b,d.Page);                
                b.RefreshDrawing(b.Rate);
            }
        }
        #endregion ActualScale-Layer-Page

        #region IsDirty

        /// <summary>
        /// Returns true if document is changed
        /// </summary>
        public bool IsDirty
        {
            get
            {
                return (bool)GetValue(IsDirtyProperty);
            }
            internal set
            {
                SetValue(IsDirtyProperty, value);

                // Raise IsDirtyChanged event.
                RoutedEventArgs newargs = new RoutedEventArgs(IsDirtyChangedEvent);
                RaiseEvent(newargs);
            }
        }

        #endregion IsDirty

        #region CanUndo

        /// <summary>
        /// Return True if Undo operation is possible
        /// </summary>
        public bool CanUndo
        {
            get
            {
                return (bool)GetValue(CanUndoProperty);
            }
            internal set
            {
                SetValue(CanUndoProperty, value);
            }
        }

        #endregion CanUndo

        #region CanRedo

        /// <summary>
        /// Return True if Redo operation is possible
        /// </summary>
        public bool CanRedo
        {
            get
            {
                return (bool)GetValue(CanRedoProperty);
            }
            internal set
            {
                SetValue(CanRedoProperty, value);
            }
        }

        #endregion CanRedo

        #region CanSelectAll

        /// <summary>
        /// Return true if Select All function is available
        /// </summary>
        public bool CanSelectAll
        {
            get
            {
                return (bool)GetValue(CanSelectAllProperty);
            }
            internal set
            {
                SetValue(CanSelectAllProperty, value);
            }
        }

        #endregion CanSelectAll

        #region CanUnselectAll

        /// <summary>
        /// Return true if Unselect All function is available
        /// </summary>
        public bool CanUnselectAll
        {
            get
            {
                return (bool)GetValue(CanUnselectAllProperty);
            }
            internal set
            {
                SetValue(CanUnselectAllProperty, value);
            }
        }

        #endregion CanUnselectAll

        #region CanDelete

        /// <summary>
        /// Return true if Delete function is available
        /// </summary>
        public bool CanDelete
        {
            get
            {
                return (bool)GetValue(CanDeleteProperty);
            }
            internal set
            {
                SetValue(CanDeleteProperty, value);
            }
        }

        #endregion CanDelete

        #region CanDeleteAll

        /// <summary>
        /// Return true if Delete All function is available
        /// </summary>
        public bool CanDeleteAll
        {
            get
            {
                return (bool)GetValue(CanDeleteAllProperty);
            }
            internal set
            {
                SetValue(CanDeleteAllProperty, value);
            }
        }

        #endregion CanDeleteAll

        #region CanModifyGraphicsText
        public bool CanModifyGraphicsText
        {
            get
            {
                return (bool)GetValue(CanModifyGraphicsTextProperty);
            }
            internal set
            {
                SetValue(CanModifyGraphicsTextProperty, value);
            }
        }
        #endregion

        #region CanMoveToFront

        /// <summary>
        /// Return true if Move to Front function is available
        /// </summary>
        public bool CanMoveToFront
        {
            get
            {
                return (bool)GetValue(CanMoveToFrontProperty);
            }
            internal set
            {
                SetValue(CanMoveToFrontProperty, value);
            }
        }

        #endregion CanMoveToFront

        #region CanMoveToBack

        /// <summary>
        /// Return true if Move to Back function is available
        /// </summary>
        public bool CanMoveToBack
        {
            get
            {
                return (bool)GetValue(CanMoveToBackProperty);
            }
            internal set
            {
                SetValue(CanMoveToBackProperty, value);
            }
        }

        #endregion CanMoveToBack

        #region CanSetProperties

        /// <summary>
        /// Return true if currently active properties (line width, color etc.)
        /// can be applied to selected objects.
        /// </summary>
        public bool CanSetProperties
        {
            get
            {
                return (bool)GetValue(CanSetPropertiesProperty);
            }
            internal set
            {
                SetValue(CanSetPropertiesProperty, value);
            }
        }

        #endregion CanSetProperties

        #region LineWidth

        /// <summary>
        /// Line width of new graphics object.
        /// Setting this property is also applied to current selection.
        /// </summary>
        public double LineWidth
        {
            get
            {
                return (double)GetValue(LineWidthProperty);
            }
            set
            {
                SetValue(LineWidthProperty, value);

            }
        }

        /// <summary>
        /// Callback function called when LineWidth dependency property is changed
        /// </summary>
        static void LineWidthChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            DrawingCanvas d = property as DrawingCanvas;

            HelperFunctions.ApplyLineWidth(d, d.LineWidth, true);
        }

        #endregion LineWidth

        #region ObjectColor

        /// <summary>
        /// Color of new graphics object.
        /// Setting this property is also applied to current selection.
        /// </summary>
        public Color ObjectColor
        {
            get
            {
                return (Color)GetValue(ObjectColorProperty);
            }
            set
            {
                SetValue(ObjectColorProperty, value);

            }
        }

        /// <summary>
        /// Callback function called when ObjectColor dependency property is changed
        /// </summary>
        static void ObjectColorChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            DrawingCanvas d = property as DrawingCanvas;

            HelperFunctions.ApplyColor(d, d.ObjectColor, true);
        }

        #endregion ObjectColor

        #region TextFontFamilyName

        /// <summary>
        /// Font Family name of new graphics object.
        /// Setting this property is also applied to current selection.
        /// </summary>
        public string TextFontFamilyName
        {
            get
            {
                return (string)GetValue(TextFontFamilyNameProperty);
            }
            set
            {
                SetValue(TextFontFamilyNameProperty, value);

            }
        }

        /// <summary>
        /// Callback function called when TextFontFamilyName dependency property is changed
        /// </summary>
        static void TextFontFamilyNameChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            DrawingCanvas d = property as DrawingCanvas;

            HelperFunctions.ApplyFontFamily(d, d.TextFontFamilyName, true);
        }

        #endregion TextFontFamilyName

        #region TextFontStyle

        /// <summary>
        /// Font style of new graphics object.
        /// Setting this property is also applied to current selection.
        /// </summary>
        public FontStyle TextFontStyle
        {
            get
            {
                return (FontStyle)GetValue(TextFontStyleProperty);
            }
            set
            {
                SetValue(TextFontStyleProperty, value);

            }
        }

        /// <summary>
        /// Callback function called when TextFontStyle dependency property is changed
        /// </summary>
        static void TextFontStyleChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            DrawingCanvas d = property as DrawingCanvas;

            HelperFunctions.ApplyFontStyle(d, d.TextFontStyle, true);
        }

        #endregion TextFontStyle

        #region TextFontWeight

        /// <summary>
        /// Font weight of new graphics object.
        /// Setting this property is also applied to current selection.
        /// </summary>
        public FontWeight TextFontWeight
        {
            get
            {
                return (FontWeight)GetValue(TextFontWeightProperty);
            }
            set
            {
                SetValue(TextFontWeightProperty, value);

            }
        }

        /// <summary>
        /// Callback function called when TextFontWeight dependency property is changed
        /// </summary>
        static void TextFontWeightChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            DrawingCanvas d = property as DrawingCanvas;

            HelperFunctions.ApplyFontWeight(d, d.TextFontWeight, true);
        }

        #endregion TextFontWeight

        #region TextFontStretch

        /// <summary>
        /// Font stretch of new graphics object.
        /// Setting this property is also applied to current selection.
        /// </summary>
        public FontStretch TextFontStretch
        {
            get
            {
                return (FontStretch)GetValue(TextFontStretchProperty);
            }
            set
            {
                SetValue(TextFontStretchProperty, value);

            }
        }

        /// <summary>
        /// Callback function called when TextFontStretch dependency property is changed
        /// </summary>
        static void TextFontStretchChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            DrawingCanvas d = property as DrawingCanvas;

            HelperFunctions.ApplyFontStretch(d, d.TextFontStretch, true);
        }

        #endregion TextFontStretch

        #region TextFontSize

        /// <summary>
        /// Font size of new graphics object.
        /// Setting this property is also applied to current selection.
        /// </summary>
        public double TextFontSize
        {
            get
            {
                return (double)GetValue(TextFontSizeProperty);
            }
            set
            {
                SetValue(TextFontSizeProperty, value);

            }
        }

        /// <summary>
        /// Callback function called when TextFontSize dependency property is changed
        /// </summary>
        static void TextFontSizeChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            DrawingCanvas d = property as DrawingCanvas;

            HelperFunctions.ApplyFontSize(d, d.TextFontSize, true);
        }

        #endregion TextFontSize

        #endregion Dependency Properties

        #region Routed Events

        /// <summary>
        /// IsDirtyChanged event.
        /// 
        /// If client binds to IsDirty property, this event is not required.
        /// But if client knows when IsDirty changed without binding, 
        /// IsDirtyChanged is needed.
        /// </summary>
        public event RoutedEventHandler IsDirtyChanged
        {
            add { AddHandler(IsDirtyChangedEvent, value); }
            remove { RemoveHandler(IsDirtyChangedEvent, value); }
        }

        #endregion Routed Events

        #region Public Functions

        /// <summary>
        /// Return list of graphic objects.
        /// Used if client program needs to make its own usage of
        /// graphics objects, like save them in some persistent storage.
        /// </summary>
        public PropertiesGraphicsBase[] GetListOfGraphicObjects()
        {
            PropertiesGraphicsBase[] result = new PropertiesGraphicsBase[graphicsList.Count];

            int i = 0;

            foreach(GraphicsBase g in graphicsList)
            {
                result[i++] = g.CreateSerializedObject();
            }

            return result;
        }

        /// <summary>
        /// Draw all graphics objects to DrawingContext supplied by client.
        /// Can be used for printing or saving image together with graphics
        /// as single bitmap.
        /// 
        /// Selection tracker is not drawn.
        /// </summary>
        public void Draw(DrawingContext drawingContext)
        {
            Draw(drawingContext, false);
        }

        /// <summary>
        /// Draw all graphics objects to DrawingContext supplied by client.
        /// Can be used for printing or saving image together with graphics
        /// as single bitmap.
        /// 
        /// withSelection = true - draw selected objects with tracker.
        /// </summary>
        public void Draw(DrawingContext drawingContext, bool withSelection)
        {
            bool oldSelection = false;

            foreach (GraphicsBase b in graphicsList)
            {
                // ---------------------------- //
                b.Rate = GetDrawRate(b,b.Page);

                if ( ! withSelection )
                {
                    // Keep selection state and unselect
                    oldSelection = b.IsSelected;
                    b.IsSelected = false;
                }

                b.Draw(drawingContext, b.Rate);

                if ( ! withSelection )
                {
                    // Restore selection state
                    b.IsSelected = oldSelection;
                }
            }
        }

        /// <summary>
        /// Get drawing rate for graphics objects.
        /// </summary>
        public double GetDrawRate(GraphicsBase gb, int currentPage)
        {
            double rate=0;
            switch (gb.Page)
            {
                case 0:
                    {
                        switch (currentPage)
                        {
                            case 0:
                                rate = 1;
                                break;
                            case 1:
                                rate = 0.25;
                                break;
                            case 2:
                                rate = 0.0625;
                                break;
                        }
                        break;
                    }

                case 1:
                    {
                        switch (currentPage)
                        {
                            case 0:
                                rate = 4;
                                break;
                            case 1:
                                rate = 1;
                                break;
                            case 2:
                                rate = 0.25;
                                break;
                        }
                        break;
                    }
                case 2:
                    {
                        switch (currentPage)
                        {
                            case 0:
                                rate = 16;
                                break;
                            case 1:
                                rate = 4;
                                break;
                            case 2:
                                rate = 1;
                                break;
                        }
                        break;
                    }
            }

            return rate;
        }


        /// <summary>
        /// Clear graphics list
        /// </summary>
        public void Clear()
        {
            graphicsList.Clear();            
            ClearHistory();
            UpdateState(true);
        }  


        public VirtualSlideInfo CheckMatchSlide(string fileName)
        {
            VirtualSlideInfo vsInfo = new VirtualSlideInfo();

            try
            {
                SerializationHelper helper;

                XmlSerializer xml = XmlSerializer.FromTypes(new[] { typeof(SerializationHelper) })[0];

                using (Stream stream = new FileStream(fileName,
                    FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    helper = (SerializationHelper)xml.Deserialize(stream);
                }

                if (helper.mSlideInfo == null)
                {
                    throw new DrawingCanvasException(Properties.Settings.Default.NoInfoInXMLFile);
                }

                return helper.mSlideInfo;              
            }
            catch (IOException e)
            {
                throw new DrawingCanvasException(e.Message, e);
            }
            catch (InvalidOperationException e)
            {
                throw new DrawingCanvasException(e.Message, e);
            }
            catch (ArgumentNullException e)
            {
                throw new DrawingCanvasException(e.Message, e);
            }
        }

        /// <summary>
        /// Load graphics from XML file.
        /// Throws: DrawingCanvasException.
        /// </summary>
        public void Load(string fileName)
        {
            try
            {
                SerializationHelper helper;

                XmlSerializer xml = XmlSerializer.FromTypes(new[] { typeof(SerializationHelper) })[0];

                using (Stream stream = new FileStream(fileName,
                    FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    helper = (SerializationHelper)xml.Deserialize(stream);
                }

                if (helper.Graphics == null)
                {
                    throw new DrawingCanvasException(Properties.Settings.Default.NoInfoInXMLFile);
                }

                graphicsList.Clear();

                foreach (PropertiesGraphicsBase g in helper.Graphics)
                {
                    Visual m = g.CreateGraphics();
                    graphicsList.Add(g.CreateGraphics());
                }
        
                // Update clip for all loaded objects.
                RefreshClip();

                ClearHistory();
                UpdateState(true);
            }
            catch (IOException e)
            {
                throw new DrawingCanvasException(e.Message, e);
            }
            catch (InvalidOperationException e)
            {
                throw new DrawingCanvasException(e.Message, e);
            }
            catch(ArgumentNullException e)
            {
                throw new DrawingCanvasException(e.Message, e);
            }
        }
           

        /// <summary>
        /// Save graphics to XML file.
        /// Throws: DrawingCanvasException.
        /// </summary>
        public void Save(string fileName, VirtualSlideInfo vsInfo)
        {
            try
            {
                SerializationHelper helper = new SerializationHelper(graphicsList);
                helper.mSlideInfo = vsInfo;
     
                XmlSerializer xml = XmlSerializer.FromTypes(new[] { typeof(SerializationHelper) })[0];

                using (Stream stream = new FileStream(fileName,
                    FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    xml.Serialize(stream, helper);
                    ClearHistory();
                    UpdateState(true);
                }
            }
            catch (IOException e)
            {
                throw new DrawingCanvasException(e.Message, e);
            }
            catch (InvalidOperationException e)
            {
                throw new DrawingCanvasException(e.Message, e);
            }
        }

  
        /// <summary>
        /// Select all
        /// </summary>
        public void SelectAll()
        {
            HelperFunctions.SelectAll(this);
            UpdateState(false);
        }

        /// <summary>
        /// Unselect all
        /// </summary>
        public void UnselectAll()
        {
            HelperFunctions.UnselectAll(this);
            UpdateState(false);
        }

        /// <summary>
        /// Delete selection
        /// </summary>
        public void Delete()
        {
            HelperFunctions.DeleteSelection(this);
            UpdateState(true);
        }

        /// <summary>
        /// Delete all
        /// </summary>
        public void DeleteAll()
        {
            HelperFunctions.DeleteAll(this);
            UpdateState(true);
        }

        /// <summary>
        /// Move selection to the front of Z-order
        /// </summary>
        public void MoveToFront()
        {
            HelperFunctions.MoveSelectionToFront(this);
            UpdateState(true);
        }

        /// <summary>
        /// Move selection to the end of Z-order
        /// </summary>
        public void MoveToBack()
        {
            HelperFunctions.MoveSelectionToBack(this);
            UpdateState(true);
        }

        public void ModifyGraphicsText()
        {
            // Enumerate all text objects
            for (int i = graphicsList.Count - 1; i >= 0; i--)
            {
                GraphicsText t = graphicsList[i] as GraphicsText;

                if (t != null)
                {
                    toolText.CreateTextBox(t, this);
                    return;
                }
            }
        }

        /// <summary>
        /// Apply currently active properties to selected objects
        /// </summary>
        public void SetProperties()
        {
            HelperFunctions.ApplyProperties(this);
            UpdateState(false);
        }

        /// <summary>
        /// Set clip for all graphics objects.
        /// </summary>
        public void RefreshClip()
        {
            foreach (GraphicsBase b in graphicsList)
            {
                b.Clip = new RectangleGeometry(new Rect(0, 0, this.ActualWidth, this.ActualHeight));

                // Good chance to refresh actual scale
                b.ActualScale = this.ActualScale;

            }
        }  

        /// <summary>
        /// Remove clip for all graphics objects.
        /// </summary>
        public void RemoveClip()
        {
            foreach (GraphicsBase b in graphicsList)
            {
                b.Clip = null;
            }
        }

        /// <summary>
        /// Undo
        /// </summary>
        public void Undo()
        {
            undoManager.Undo();
            UpdateState(true);
        }

        /// <summary>
        /// Redo
        /// </summary>
        public void Redo()
        {
            undoManager.Redo();
            UpdateState(true);
        }

        #endregion Public Functions

        #region Internal Properties (-> Public Properties)

        /// <summary>
        /// Get graphic object by index
        /// </summary>
        public GraphicsBase this[int index]
        {
            get
            {
                if ( index >= 0  &&  index < Count )
                {
                    return (GraphicsBase)graphicsList[index];
                }

                return null;
            }
        }

        /// <summary>
        /// Get number of graphic objects
        /// </summary>
        public int Count
        {
            get
            {
                return graphicsList.Count;
            }
        }

        public int startIndexOfSelection = 0;

         /// <summary>
        /// Get number of selected graphic objects
        /// </summary>
        public int SelectionStartIndex
        {
            get
            {
                if (SelectionCount > 0)
                    return startIndexOfSelection;
                else
                    return -1;
            }
        }
        /// <summary>
        /// Get number of selected graphic objects
        /// </summary>
		public int SelectionCount
		{
			get
			{
				int n = 0;
                int m = 0;
				foreach (GraphicsBase g in this.graphicsList)
				{
					if ( g.IsSelected )
					{
                        if (n == 0)
                            startIndexOfSelection = m;

						n++;
					}
                    m++;
				}

				return n;
			}
		}

        /// <summary>
        /// Return list of graphics
        /// </summary>
        public VisualCollection GraphicsList
        {
            get
            {
                return graphicsList;
            }
        }

        /// <summary>
        /// Returns INumerable which may be used for enumeration
        /// of selected objects        /// </summary>
        public IEnumerable<GraphicsBase> Selection
        {
            get
            {
                foreach (GraphicsBase o in graphicsList)
                {
                    if (o.IsSelected)
                    {
                        yield return o;
                    }
                    
                }
            }

        }

        #endregion Internal Properties

        #region Visual Children Overrides

        /// <summary>
        /// Get number of children: VisualCollection count.
        /// If in-place editing textbox is active, add 1.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get 
            { 
                int n = graphicsList.Count; 

                if ( toolText.TextBox != null )
                {
                    n++;
                }

                return n;
            }
        }

        /// <summary>
        /// Get visual child - one of GraphicsBase objects
        /// or in-place editing textbox, if it is active.
        /// </summary>
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= graphicsList.Count )
            {
                if (toolText.TextBox != null && index == graphicsList.Count )
                {
                    return toolText.TextBox;
                }

                throw new ArgumentOutOfRangeException("index");
            }

            return graphicsList[index];
        }

        #endregion Visual Children Overrides

        #region Mouse Event Handlers
        /// <summary>
         /// Mouse down.
         /// Left button down event is passed to active tool.
         /// Right button down event is handled in this class.
         /// </summary>
        void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (tools[(int)Tool] == null)
            {
                return;
            }

            this.Focus();

            if (e.ChangedButton == MouseButton.Left)
            {
                /*
                if (e.ClickCount == 2)
                {
                    HandleDoubleClick(e);        // special case for GraphicsText
                    // ��Ϊ�Ҽ� Modify Text
                }
                else
                {
                    tools[(int)Tool].OnMouseDown(this, e);
                }
                */

                tools[(int)Tool].OnMouseDown(this, e);
                UpdateState(false);
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                bool isGraphicsText = false;
                Point point = e.GetPosition(this);

                // Enumerate all text objects
                for (int i = graphicsList.Count - 1; i >= 0; i--)
                {
                    GraphicsText t = graphicsList[i] as GraphicsText;

                    if (t != null)
                    {
                        if (t.Contains(point))
                        {
                            isGraphicsText = true;                         
                        }
                    }
                }

                if (IsMouseMoveOnGraphics)
                {
                    // Change current selection if necessary
                    point = e.GetPosition(this);
                    ChangeCurrentSelection(point);

                    MenuItem item;

                    /// Enable/disable menu items.
                    foreach (object obj in contextMenu.Items)
                    {
                        item = obj as MenuItem;

                        if (item != null)
                        {
                            ContextMenuCommand command = (ContextMenuCommand)item.Tag;

                            switch (command)
                            {
                                case ContextMenuCommand.SelectAll:
                                    item.IsEnabled = CanSelectAll;
                                    break;
                                case ContextMenuCommand.UnselectAll:
                                    item.IsEnabled = CanUnselectAll;
                                    break;
                                case ContextMenuCommand.Delete:
                                    item.IsEnabled = CanDelete;
                                    break;
                                case ContextMenuCommand.DeleteAll:
                                    item.IsEnabled = CanDeleteAll;
                                    break;
                                case ContextMenuCommand.MoveToFront:
                                    item.IsEnabled = CanMoveToFront;
                                    break;
                                case ContextMenuCommand.MoveToBack:
                                    item.IsEnabled = CanMoveToBack;
                                    break;
                                case ContextMenuCommand.Undo:
                                    item.IsEnabled = CanUndo;
                                    break;
                                case ContextMenuCommand.Redo:
                                    item.IsEnabled = CanRedo;
                                    break;
                                case ContextMenuCommand.ModifyGraphicsText:
                                    if(isGraphicsText)
                                        item.IsEnabled = !CanModifyGraphicsText;
                                    else
                                        item.IsEnabled = CanModifyGraphicsText;

                                    break;
                                case ContextMenuCommand.SetProperties:
                                    item.IsEnabled = CanSetProperties; 
                                    break;
                            }
                        }
                    }

                    contextMenu.IsOpen = true;
                }
            
            }
        }

        /// <summary>
        /// Mouse move.
        /// Moving without button pressed or with left button pressed
        /// is passed to active tool.
        /// </summary>
        void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (tools[(int)Tool] == null)
            {
                return;
            }

            if ( e.MiddleButton == MouseButtonState.Released  &&  e.RightButton == MouseButtonState.Released )
            {
                tools[(int)Tool].OnMouseMove(this, e);

                UpdateState(true);
            }
            else
            {
                this.Cursor = HelperFunctions.DefaultCursor;
            }
        }

        /// <summary>
        /// Mouse up event.
        /// Left button up event is passed to active tool.
        /// </summary>
        void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (tools[(int)Tool] == null)
            {
                return;
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                tools[(int)Tool].OnMouseUp(this, e);

                UpdateState(true);
            }
        }
        #endregion Mouse Event Handlers

        #region Other Event Handlers
        /// <summary>
        /// Initialization after control is loaded
        /// </summary>
        void DrawingCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focusable = true;      // to handle keyboard messages
        }

        /// <summary>
        /// Context menu item is clicked
        /// </summary>
        void contextMenuItem_Click(object sender, RoutedEventArgs e)
        {      
            MenuItem item = sender as MenuItem;

            if ( item == null )
            {
                return;
            }

            ContextMenuCommand command = (ContextMenuCommand)item.Tag;

            switch ( command )
            {
                case ContextMenuCommand.SelectAll:
                    SelectAll();
                    break;
                case ContextMenuCommand.UnselectAll:
                    UnselectAll();
                    break;
                case ContextMenuCommand.Delete:
                    Delete();
                    break;
                case ContextMenuCommand.DeleteAll:
                    DeleteAll();
                    break;
                case ContextMenuCommand.MoveToFront:
                    MoveToFront();
                    break;
                case ContextMenuCommand.MoveToBack:
                    MoveToBack();
                    break;
                case ContextMenuCommand.ModifyGraphicsText:
                    ModifyGraphicsText();
                    break;
                case ContextMenuCommand.Undo:
                    Undo();
                    break;
                case ContextMenuCommand.Redo:
                    Redo();
                    break;
                case ContextMenuCommand.SetProperties:
                    SetProperties();
                    break;
            }


        }

        /// <summary>
        /// Mouse capture is lost
        /// </summary>
        void DrawingCanvas_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if ( this.IsMouseCaptured )
            {
                CancelCurrentOperation();
                UpdateState(false);
            }
        }

        /// <summary>
        /// Handle keyboard input
        /// </summary>
        void DrawingCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            // Esc key stops currently active operation            
            if (e.Key == Key.Escape)
            {
                if (this.IsMouseCaptured)
                {
                    CancelCurrentOperation();
                    UpdateState(false);
                }
            }         
        }

        /// <summary>
        /// UndoManager state is changed.
        /// Refresh CanUndo, CanRedo and IsDirty properties.
        /// </summary>
        void undoManager_StateChanged(object sender, EventArgs e)
        {
            this.CanUndo = undoManager.CanUndo;
            this.CanRedo = undoManager.CanRedo;

            // Set IsDirty only if it is actually changed.
            // Setting IsDirty raises event for client.
            if (undoManager.IsDirty != this.IsDirty)
            {
                this.IsDirty = undoManager.IsDirty;
            }
        }


        #endregion Other Event Handlers

        #region Other Functions

        /// <summary>
        /// Create context menu
        /// </summary>
        void CreateContextMenu()
        {
            MenuItem menuItem;

            contextMenu = new ContextMenu();

            menuItem = new MenuItem();
            menuItem.Header = "Select All";
            menuItem.Tag = ContextMenuCommand.SelectAll;
            menuItem.Click += new RoutedEventHandler(contextMenuItem_Click);
            contextMenu.Items.Add(menuItem);

            menuItem = new MenuItem();
            menuItem.Header = "Unselect All";
            menuItem.Tag = ContextMenuCommand.UnselectAll;
            menuItem.Click += new RoutedEventHandler(contextMenuItem_Click);
            contextMenu.Items.Add(menuItem);

            menuItem = new MenuItem();
            menuItem.Header = "Delete";
            menuItem.Tag = ContextMenuCommand.Delete;
            menuItem.Click += new RoutedEventHandler(contextMenuItem_Click);
            contextMenu.Items.Add(menuItem);

            menuItem = new MenuItem();
            menuItem.Header = "Delete All";
            menuItem.Tag = ContextMenuCommand.DeleteAll;
            menuItem.Click += new RoutedEventHandler(contextMenuItem_Click);
            contextMenu.Items.Add(menuItem);

            contextMenu.Items.Add(new Separator());

            menuItem = new MenuItem();
            menuItem.Header = "Move to Front";
            menuItem.Tag = ContextMenuCommand.MoveToFront;
            menuItem.Click += new RoutedEventHandler(contextMenuItem_Click);
            contextMenu.Items.Add(menuItem);

            menuItem = new MenuItem();
            menuItem.Header = "Move to Back";
            menuItem.Tag = ContextMenuCommand.MoveToBack;
            menuItem.Click += new RoutedEventHandler(contextMenuItem_Click);
            contextMenu.Items.Add(menuItem);

            contextMenu.Items.Add(new Separator());

            menuItem = new MenuItem();
            menuItem.Header = "Modify Text";
            menuItem.Tag = ContextMenuCommand.ModifyGraphicsText;
            menuItem.Click += new RoutedEventHandler(contextMenuItem_Click);
            contextMenu.Items.Add(menuItem);

            contextMenu.Items.Add(new Separator());

            menuItem = new MenuItem();
            menuItem.Header = "Undo";
            menuItem.Tag = ContextMenuCommand.Undo;
            menuItem.Click += new RoutedEventHandler(contextMenuItem_Click);
            contextMenu.Items.Add(menuItem);

            menuItem = new MenuItem();
            menuItem.Header = "Redo";
            menuItem.Tag = ContextMenuCommand.Redo;
            menuItem.Click += new RoutedEventHandler(contextMenuItem_Click);
            contextMenu.Items.Add(menuItem);

            menuItem = new MenuItem();
            menuItem.Header = "Set Properties";
            menuItem.Tag = ContextMenuCommand.SetProperties;
            menuItem.Click += new RoutedEventHandler(contextMenuItem_Click);
            contextMenu.Items.Add(menuItem);
        }

        public void ChangeCurrentSelection(Point point)
        {
            // Change current selection if necessary
            GraphicsBase o = null;

            for (int i = graphicsList.Count - 1; i >= 0; i--)
            {
                if (((GraphicsBase)graphicsList[i]).MakeHitTest(point) == 0)
                {
                    o = (GraphicsBase)graphicsList[i];
                    break;
                }
            }

            if (o != null)
            {
                if (!o.IsSelected)
                {
                    UnselectAll();
                }

                // Select clicked object
                o.IsSelected = true;
            }
            else
            {
                UnselectAll();
            }

            UpdateState(false);
        }

        /// <summary>
        /// Show context menu.
        /// </summary>
        void ShowContextMenu(MouseButtonEventArgs e)
        {
            // Change current selection if necessary
            Point point = e.GetPosition(this);
            ChangeCurrentSelection(point);

            MenuItem item;

            /// Enable/disable menu items.
            foreach(object obj in contextMenu.Items)
            {
                item = obj as MenuItem;

                if ( item != null )
                {
                    ContextMenuCommand command = (ContextMenuCommand)item.Tag;

                    switch (command)
                    {
                        case ContextMenuCommand.SelectAll:
                            item.IsEnabled = CanSelectAll;
                            break;
                        case ContextMenuCommand.UnselectAll:
                            item.IsEnabled = CanUnselectAll;
                            break;
                        case ContextMenuCommand.Delete:
                            item.IsEnabled = CanDelete;
                            break;
                        case ContextMenuCommand.DeleteAll:
                            item.IsEnabled = CanDeleteAll;
                            break;
                        case ContextMenuCommand.MoveToFront:
                            item.IsEnabled = CanMoveToFront;
                            break;
                        case ContextMenuCommand.MoveToBack:
                            item.IsEnabled = CanMoveToBack;
                            break;
                        case ContextMenuCommand.Undo:
                            item.IsEnabled = CanUndo;
                            break;
                        case ContextMenuCommand.Redo:
                            item.IsEnabled = CanRedo;
                            break;
                        case ContextMenuCommand.ModifyGraphicsText:
                            item.IsEnabled = CanModifyGraphicsText;
                            break;
                        case ContextMenuCommand.SetProperties:
                            item.IsEnabled = CanSetProperties;
                            break;
                    }
                }
            }

            contextMenu.IsOpen = true;
        }

        /// <summary>
        /// Cancel currently executed operation:
        /// add new object or group selection.
        /// 
        /// Called when mouse capture is lost or Esc is pressed.
        /// </summary>
        void CancelCurrentOperation()
        {
            if (Tool == ToolType.Pointer)
            {
                if (graphicsList.Count > 0)
                {
                    if (graphicsList[graphicsList.Count - 1] is GraphicsSelectionRectangle)
                    {
                        // Delete selection rectangle if it exists
                        graphicsList.RemoveAt(graphicsList.Count - 1);
                    }
                    else
                    {
                        // Pointer tool moved or resized graphics object.
                        // Add this action to the history
                        toolPointer.AddChangeToHistory(this);
                    }
                }
            }
            else if (Tool > ToolType.Pointer && Tool < ToolType.Max)
            {
                // Delete last graphics object which is currently drawn
                if (graphicsList.Count > 0)
                {
                    graphicsList.RemoveAt(graphicsList.Count - 1);
                }
            }

            Tool = ToolType.Pointer;

            this.ReleaseMouseCapture();
            this.Cursor = HelperFunctions.DefaultCursor;
        }

        /// <summary>
        /// Hide in-place editing textbox.
        /// Called from TextTool, when user pressed Enter or Esc,
        /// or from this class, when user clicks on the canvas.
        /// 
        /// graphicsText passed to this function can be new text added by
        /// ToolText, or existing text opened for editing.
        /// If ToolText.OldText is empty, this is new object.
        /// If not, this is existing object.
        /// </summary>
        internal void HideTextbox(GraphicsText graphicsText)
        {
            if (toolText.TextBox == null )
            {
                return;
            }

            graphicsText.IsSelected = true;   // restore selection which was removed for better textbox appearance


            if (toolText.TextBox.Text.Trim().Length == 0)
            {
                // Textbox is empty: remove text object.

                if ( ! String.IsNullOrEmpty(toolText.OldText) )  // existing text was edited
                {
                    // Since text object is removed now,
                    // Add Delete command to the history
                    undoManager.AddCommandToHistory(new CommandDelete(this));
                }

                // Remove empty text object
                graphicsList.Remove(graphicsText);
            }
            else
            {
                if (!String.IsNullOrEmpty(toolText.OldText))  // existing text was edited
                {
                    if ( toolText.TextBox.Text.Trim() != toolText.OldText )     // text was really changed
                    {
                        // Create command
                        CommandChangeState command = new CommandChangeState(this);

                        // Make change in the text object
                        graphicsText.Text = toolText.TextBox.Text.Trim();
                        graphicsText.UpdateRectangle();

                        // Keep state after change and add command to the history
                        command.NewState(this);
                        undoManager.AddCommandToHistory(command);
                    }
                }
                else                                          // new text was added
                {
                    // Make change in the text object
                    graphicsText.Text = toolText.TextBox.Text.Trim();
                    graphicsText.UpdateRectangle();
                        
                    // Add command to the history
                    undoManager.AddCommandToHistory(new CommandAdd(graphicsText));
                }
            }

            // Remove textbox and set it to null.
            this.Children.Remove(toolText.TextBox);
            toolText.TextBox = null;

            // This enables back all ApplicationCommands,
            // which are disabled while textbox is active.
            this.Focus();
        }

        /// <summary>
        /// Open in-place edit box if GraphicsText is clicked
        /// </summary>
        void HandleDoubleClick(MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(this);

            // Enumerate all text objects
            for(int i = graphicsList.Count - 1; i >= 0; i--)
            {
                GraphicsText t = graphicsList[i] as GraphicsText;

                if ( t != null )
                {
                    if ( t.Contains(point) )
                    {
                        toolText.CreateTextBox(t, this);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Add command to history.
        /// </summary>
        internal void AddCommandToHistory(CommandBase command)
        {
            undoManager.AddCommandToHistory(command);
        }

        /// <summary>
        /// Clear Undo history.
        /// </summary>
        void ClearHistory()
        {
            undoManager.ClearHistory();
        }

        /// <summary>
        /// Update state of Can* dependency properties
        /// used for Edit commands.
        /// This function calls after any change in drawing canvas state,
        /// caused by user commands.
        /// Helps to keep client controls state up-to-date, in the case
        /// if Can* properties are used for binding.
        /// </summary>
        void UpdateState(bool isGraphicListChanged)
        {
            bool hasObjects = (this.Count > 0);
            bool hasSelectedObjects = (this.SelectionCount > 0);

            CanSelectAll = hasObjects;
            CanUnselectAll = hasObjects;
            CanDelete = hasSelectedObjects;
            CanDeleteAll = hasObjects;
            CanMoveToFront = hasSelectedObjects;
            CanMoveToBack = hasSelectedObjects;

            if (isGraphicListChanged && this.graphicsList.Count >= 0)
                this.OnGraphicListChanged(null);

            CanSetProperties = HelperFunctions.CanApplyProperties(this);
        }


        #endregion Other Functions

        #region TranslateTransform and ScaleTransform
        Size _smallScrollIncremen;
        TranslateTransform _translate;
        ScaleTransform _scale;
        Size _extent;
        Size _viewPortSize;
        ScrollViewer _owner;

        /// <summary>
        /// Set the scroll amount for the scroll bar arrows.
        /// </summary>
        public Size SmallScrollIncrement
        {
            get { return _smallScrollIncremen; }
            set {  _smallScrollIncremen = value; }
        }


        /// <summary>
        /// Return the height of the current viewport that is visible in the ScrollViewer.
        /// </summary>
        public double ViewportHeight
        {
            get { return _viewPortSize.Height; }
            set { _viewPortSize.Height = value; }
        }

        /// <summary>
        /// Return the width of the current viewport that is visible in the ScrollViewer.
        /// </summary>
        public double ViewportWidth
        {
            get { return _viewPortSize.Width; }
            set { _viewPortSize.Width = value; }
        }

        /// <summary>
        /// Return the ScrollViewer that contains this object.
        /// </summary>
        public ScrollViewer ScrollOwner
        {
            get { return _owner; }
            set { _owner = value; }
        }


        /// <summary>
        /// The width of the canvas to be scrolled.
        /// </summary>
        public double ExtentWidth
        {
            get { return _extent.Width * _scale.ScaleX; }
            set { _extent.Width = value;}
        }

        /// <summary>
        /// The height of the canvas to be scrolled.
        /// </summary>
        public double ExtentHeight
        {
            get { return _extent.Height * _scale.ScaleY; }
            set { _extent.Height = value; }
        }

        /// <summary>
        /// The current zoom transform.
        /// </summary>
        public ScaleTransform Scale
        {
            get { return _scale; }
        }

        /// <summary>
        /// The current translate transform.
        /// </summary>
        public TranslateTransform Translate
        {
            get { return _translate; }
        }

        /// <summary>
        /// Scroll to the given absolute horizontal scroll position.
        /// </summary>
        /// <param name="offset">The horizontal position to scroll to</param>
        public void SetHorizontalOffset(double offset)
        {
            double xoffset = Math.Max(Math.Min(offset, ExtentWidth - ViewportWidth), 0);
            _translate.X = -xoffset;
            OnScrollChanged();
        }

        /// <summary>
        /// Scroll to the given absolute vertical scroll position.
        /// </summary>
        /// <param name="offset">The vertical position to scroll to</param>
        public void SetVerticalOffset(double offset)
        {
            double yoffset = Math.Max(Math.Min(offset, ExtentHeight - ViewportHeight), 0);
            _translate.Y = -yoffset;
            OnScrollChanged();
        }

        /// <summary>
        /// Get the current horizontal scroll position.
        /// </summary>
        public double HorizontalOffset
        {
            get { return -_translate.X; }
        }

        /// <summary>
        /// Return the current vertical scroll position.
        /// </summary>
        public double VerticalOffset
        {
            get { return -_translate.Y; }
        }


        /// <summary>
        /// Callback whenever the current TranslateTransform is changed.
        /// </summary>
        /// <param name="sender">TranslateTransform</param>
        /// <param name="e">noop</param>
        void OnTranslateChanged(object sender, EventArgs e)
        {
            OnScrollChanged();
        }

        /// <summary>
        /// Callback whenever the current ScaleTransform is changed.
        /// </summary>
        /// <param name="sender">ScaleTransform</param>
        /// <param name="e">noop</param>
        void OnScaleChanged(object sender, EventArgs e)
        {
            OnScrollChanged();
        }


        private void  OnScrollChanged()
        {
            InvalidateScrollInfo();
        }

        /// <summary>
        /// Tell the ScrollViewer to update the scrollbars because, extent, zoom or translate has changed.
        /// </summary>
        public void InvalidateScrollInfo()
        {
            if (_owner != null)
            {
                _owner.InvalidateScrollInfo();
            }
        }

        /// <summary>
        /// Scroll down one small scroll increment.
        /// </summary>
        public void LineDown()
        {
      
            SetVerticalOffset(VerticalOffset + (_smallScrollIncremen.Height* _scale.ScaleX));
        }

        /// <summary>
        /// Scroll left by one small scroll increment.
        /// </summary>
        public void LineLeft()
        {
            SetHorizontalOffset(HorizontalOffset - (_smallScrollIncremen.Width * _scale.ScaleX));
        }

        /// <summary>
        /// Scroll right by one small scroll increment
        /// </summary>
        public void LineRight()
        {
            SetHorizontalOffset(HorizontalOffset + (_smallScrollIncremen.Width * _scale.ScaleX));
        }

        /// <summary>
        /// Scroll up by one small scroll increment
        /// </summary>
        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset - (_smallScrollIncremen.Height * _scale.ScaleX));
        }
        #endregion

        #region GraphicListChangedEvent
        /// <summary>
        ///   Occurs when the GraphicList property is changed
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler<EventArgs> GraphicListChanged;

        public void OnGraphicListChanged(object sender)
        {
            if (GraphicListChanged != null)
            {
                GraphicListChanged(sender, EventArgs.Empty);
            }
        }


        #endregion
    }
}
