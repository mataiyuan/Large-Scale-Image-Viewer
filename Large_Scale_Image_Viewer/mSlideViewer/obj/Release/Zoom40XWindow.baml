   M S B A M L   `   `   `  ���� G  CmSlideViewer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:mSlideViewermSlideViewer       mSlideViewer.Zoom40XWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     ,localclr-namespace:mSlideViewer       � Title$
  Zoom��$	��155��$��82�� � 
ResizeMode$ NoResize=� � Topmost .  � ShowInTaskbar .   � SizeToContent$ WidthAndHeight=� � WindowStyle$ 
ToolWindow=�.���  � RowDefinitions �� $
��120* ��� $��7* � � ColumnDefinitions �� $	��35* ��� $	��45* �.+��� $��2��.+��� -    ��Name$ 
btnZoomFit��$	��Fit��$��16��$��25��$��10(���u� 
	 u� Top$	 3Q�
 u� Bottom$
 1Q��� -   $ 	btnZoom1x��$��1x��$��16��$��25��$��10(���u� $
 1Q��� -   $ 	btnZoom5x��$��5x��$��16��$��25��$��10(���u� $
 1Q��� -   $ 
btnZoom10x��$	��10x��$��16��$��25��$��10(���u� $
 1Q��� -   $ 
btnZoom20x��$	��20x��$��16��$��25��$��10(���u� $
 1Q��� -   $ 
btnZoom30x��$	��30x��$��16��$��25��$��10(���u� $
 1Q��� -   $ 
btnZoom40x��$	��40x��$��16��$��25��$��10(���u� $
 3Q��� -    ��Name$ 
sliderZoom��$��1��$
��Left=�$��5,3,0,0q�$	��Top=�$��32�� �� Orientation$ Vertical=�$	��120�� �� TickPlacement$ BottomRight=� �� AutoToolTipPlacement$ BottomRight=� �� IsDirectionReversed .  � Maximum$ 40Q�$��2��