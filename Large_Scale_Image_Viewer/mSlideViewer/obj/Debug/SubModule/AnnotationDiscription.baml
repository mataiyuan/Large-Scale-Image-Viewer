   M S B A M L   `   `   `  ����O  Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem  5   	    mSlideViewer(   "mSlideViewer.AnnotationDiscription   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   6sys&clr-namespace:System;assembly=mscorlib     5   	     � Title$  Annotation Discription��5   	   $	��345��6(   $	��770��65   .��5      �  � RowDefinitions 5   
   �� 5      $��90 �6   �� 5	      $	��185 �6   �� 5
      $��40 �6   .+�5   
   �� $��Information:��6   $
��Left=�6'   $	��Top=�6B   $��26��6Z   $��75��6f   $��5,0,0,0q�6q   �� 5   
   -   5         ��Name$
 btOK��5      $��OK��6   $
��Left=�6,   $��	549,6,0,0q�6G   $��2��6Z   $	��Top=�6g   $��75��6   $��20��6�    J� RenderTransformOrigin$ 	0.493,1.1&�6�   �� 5   
   -   5        $ btClose��5      $��Close��6"   $
��Left=�62   $��	648,6,0,0q�6M   $��2��6`   $	��Top=�6m   $��75��6�   $��20��6�   � 5   
   $
��Left=�6   $	��167��6*   $��0,10,0,0q�67   $��1��6I   $	��Top=�6V   $	��753��6n   .+�5      �� $��Report:��6   $
��Left=�6&   $��	164,8,0,0q�6A   $	��Top=�6T   }�. $��23��6�   $��55��6�   (  "System.Windows.Controls.DatePicker  5      -   5          Name$ 
dateReport��5      
 �� Now   SelectedDate# Z 6.   $
��Left=�6Y   $��
592,93,0,0q�6t   $	��Top=�6�   $	��151��6�   $��22��6�   �� 5      -   5         ��Name$ txReport��5      $
��Left=�6(   $��
168,31,0,0q�6C    �� TextWrapping$
 Wrap=�6W   $	��410��6k   $	��126��6w   $	��Top=�6�   $��Microsoft YaHei)�6�   �� 5      $��Report Date:��6   $
��Left=�6+   $��
592,67,0,0q�6F   $	��Top=�6Z   $��26��6r   $��94��6~   �� 5      $��	Reporter:��6   $
��Left=�6(   $��	592,5,0,0q�6C   $	��Top=�6V   $��26��6n   $��94��6z   �� 5      -   5        $ 
txReporter��5      $
��Left=�6*   $��
592,31,0,0q�6E   $
 Wrap=�6Y   $	��151��6m   $��22��6y   $	��Top=�6�   $��Microsoft YaHei)�6�   �� 5      $��Favorite Items: ��6   $
��Left=�6/   $��10,8,0,0q�6J   $	��Top=�6\   $��98��6t   �� 5      -   5        	 ��Name$	 favorite��5      $
��Left=�6(   $	��126��6C   $��	10,31,0,0q�6P   $	��Top=�6c   $	��153��6{   ��5      �� 5      
 �� 
TargetType"
 ��6    �� BasedOn# [B��6>   .�5      -   5        5      5      5      �� 5    
   -   5         ��Name$ tbAnnotationInfo��5       $
��Left=�6.   $��	10,26,0,0q�6I   $
��Wrap=�6\   $	��Top=�6p   $��54��6�   $	��740��6�   #��� ��6�   �� 5!   
   -	   5        $ button��5!      $��Add to Report��6!   $
��Left=�69   $��
52,171,0,0q�6T   $	��Top=�6h   $	��106��6�   $��1��6�   $��2��6�   6�   5'      5(      