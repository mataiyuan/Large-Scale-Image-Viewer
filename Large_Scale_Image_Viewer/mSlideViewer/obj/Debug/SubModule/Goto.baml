   M S B A M L   `   `   `  ����W  SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U<clr-namespace:System.Windows.Input;assembly=PresentationCoreSystem.Windows.Input  5   	    mSlideViewer   mSlideViewer.GotoDialog   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml      5   	   Ninput<clr-namespace:System.Windows.Input;assembly=PresentationCore      5   	     � Title$(  "Go to the custom position on image��5   	   $��219.696��64   $��445.78��6E    � Icon$D >/mSlideViewer;component/Resources/Icons/mSlideViewer-16X16.ico��6T   .��5      �  � RowDefinitions 5   
   �� 5      $	��91* �6   �� 5	      $	��57* �6   �� 5
      $	��41* �6   .+�5   
   �� -   5         ��Name$ tbXPosition��5      $
��Left=�6'   $��22��6B   $��	57,30,0,0q�6N    �� TextWrapping$
 Wrap=�6a   $	��Top=�6v   $��80��6�   $��1��6�    �� IsInputMethodEnabled .  6�   �� 5   
   -   5        $ tbYPosition��5      $
��Left=�6'   $��22��6B   $��
187,30,0,0q�6N   $
 Wrap=�6b   $	��Top=�6w   $��80��6�   $��1��6�    .  6�   �� 5   
   -   5         ��Name$ btGoto��5      $��Goto Position��6!   $
��Left=�69   $��
250,11,0,0q�6T   $	��Top=�6h   $��82��6�   $��2��6�   $��22��6�   �� 5   
   -   5        $ btClose��5      $��Close��6"   $
��Left=�62   $��
347,11,0,0q�6M   $	��Top=�6a   $��82��6y   $��2��6�   $��22��6�   �� 5   
   -   5         ��Name$ tbImageInfo��5      $
��Left=�6)   $��	10,10,0,0q�6D   $
��Wrap=�6W   $��Image Information��6k   $	��Top=�6�   $	��418��6�   $��60��6�   �� 5   
   $��Y:��6   $
��Left=�6   $��
163,26,0,0q�68   $	��Top=�6L   $��1��6d   $��26��6q   $��19��6}   �� 5   
   $��X:��6   $
��Left=�6   $��	32,26,0,0q�68   $	��Top=�6K   $��1��6c   $��26��6p   $��20��6|   �� 5   
   $��Position On Image:��6   $
��Left=�6-   $��10,1,0,0q�6H   $	��Top=�6Z   $��1��6r   $��26��6   $	��107��6�   �� 5   
   $
��Mag:��6   $
��Left=�6   $��
278,27,0,0q�6:   $��1��6N   $	��Top=�6[   �� 5   
   -   5         ��Name$ cbMag��5      $
��Left=�6"   $��
320,30,0,0q�6=   $��1��6Q   $	��Top=�6^   $��80��6v   6�   5      5      