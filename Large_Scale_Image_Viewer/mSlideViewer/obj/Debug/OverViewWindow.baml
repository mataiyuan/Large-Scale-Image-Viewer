   M S B A M L   `   `   `  ����G  CmSlideViewer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:mSlideViewermSlideViewer  5   	   !    mSlideViewer.OverViewWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ,localclr-namespace:mSlideViewer     5   	   -   5          � Title$  OverView��5   	    � 
ResizeMode$ NoResize=�6    � WindowStyle$ 
ToolWindow=�5	   '    � ShowInTaskbar .   � ShowActivated .  $	��196��5
   L   $	��264��6X   $��Center=�6e   $��Center=�6�    � 	Resources 5      (   "mSlideViewer.ColorToBrushConverter   colorToBrushConverter �� DataType    mSlideViewer.RectangleData �� 	Converter��%�   &	        (��      5      *, +)7     5   
   5      �� 5   
   "  5      .R�5      �� ��5      �� *Width+��5      �� *Height+��5      �� *Color+8   5      5!      .��5#      � .+�5%   
   �� -   5         ��Name$ OverViewCanvas��5%      $	��220��6)   $	��180��66   $��0q�6B   $��Center=�6M   $��Center=�6j   ��5&      �� 5'      -   5        OverViewImageBrush��	 j� Stretch$
	 None=�5'   9   
 �� ImageSource$
 OverView.png��6H   .+�5*      n� -   5         n�Name$ overviewZoomRectThumb��5*       �� Left$ 10��5+      
 �� Top$ 10��5,      $��60��5-      $��38��6    J� Opacity$	 0.5Q�5.      ��5/      �� 50       �� 
TargetType" n�6&   .R�51      �� 
���   �$��1q�66   
��� ��� �� CornerRadius$ 1��6^   53      55      57      59      5:      