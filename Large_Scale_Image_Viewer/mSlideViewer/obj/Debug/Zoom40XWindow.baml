   M S B A M L   `   `   `  ����G  CmSlideViewer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:mSlideViewermSlideViewer  5   	        mSlideViewer.Zoom40XWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ,localclr-namespace:mSlideViewer     5   	     � Title$
  Zoom��5   	   $��160         ��6   $��82��5	       � 
ResizeMode$ NoResize=�6    � Topmost .  � ShowInTaskbar .   � SizeToContent$ WidthAndHeight=�6Q    � WindowStyle$ 
ToolWindow=�6p   .��5
      �  � RowDefinitions 5   
   �� 5      $
��120* �6   �� 5      $��7* �6    � ColumnDefinitions 5   
   �� 5      $	��35* �6   �� 5      $	��45* �6   .+�5   
   �� $��2��6   .+�5      �� -   5         ��Name$ 
btnZoomFit��5      $	��Fit��6)   $��16��67   $��25��6C   $��10(�6N   ��5      u� 5      
	 u� Top$	 3Q�6    
 u� Bottom$
 1Q�6(   5      �� 5      -   5        $ 	btnZoom1x��5      $��1x��6(   $��16��65   $��25��6A   $��10(�6L   ��5      u� 5      $
 1Q�6    5      �� 5      -   5        $ 	btnZoom5x��5      $��5x��6(   $��16��65   $��25��6A   $��10(�6L   ��5      u� 5       $
 1Q�6    5"      �� 5#      -   5        $ 
btnZoom10x��5#      $	��10x��6)   $��16��67   $��25��6C   $��10(�6N   ��5$      u� 5%      $
 1Q�6    5'      �� 5(      -   5        $ 
btnZoom20x��5(      $	��20x��6)   $��16��67   $��25��6C   $��10(�6N   ��5)      u� 5*      $
 1Q�6    5,      �� 5-      -   5        $ 
btnZoom30x��5-      $	��30x��6)   $��16��67   $��25��6C   $��10(�6N   ��5.      u� 5/      $
 1Q�6    51      �� 52      -   5        $ 
btnZoom40x��52      $	��40x��6)   $��16��67   $��25��6C   $��10(�6N   ��53      u� 54      $
 3Q�6    56      57      �� 58   
   -   5         ��Name$ 
sliderZoom��58      $��1��6%   $
��Left=�65   $��5,3,0,0q�6P   $	��Top=�6a   $��32��6y    �� Orientation$ Vertical=�6�   $	��120��6�    �� AutoToolTipPlacement$ BottomRight=�6�    �� IsDirectionReversed .  � Maximum$	 256Q�6�   $��2��6�    � Minimum$	 4.5Q�6,   � SmallChange$	 0.5Q�6:   �� TickPlacement$ BottomRight=�6L   � Value$	 256Q�6h   �� TickFrequency$ 5Q�6t  5:      5;      