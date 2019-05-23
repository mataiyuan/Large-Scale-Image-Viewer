using System;
using System.Collections.Generic;
using System.Text;

namespace DrawToolsLib
{
    /// <summary>
    /// Defines drawing tool
    /// </summary>
    public enum ToolType
    {
        None,
        Pointer,
        Rectangle,
        Ellipse,
        Line,
        Arrow,
        Ruler,
        PolyLine,
        Text,
        Max
    };

    /// <summary>
    /// Context menu command types
    /// </summary>
    internal enum ContextMenuCommand
    {
        SelectAll,
        UnselectAll,
        Delete, 
        DeleteAll,
        MoveToFront,
        MoveToBack,
        ModifyGraphicsText,
        Undo,
        Redo,
        SetProperties
    };
}
