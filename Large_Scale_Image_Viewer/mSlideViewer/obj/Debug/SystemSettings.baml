   M S B A M L   `   `   `  ����K  GUtilities, Version=1.0.6824.33436, Culture=neutral, PublicKeyToken=null8*clr-namespace:Utilities;assembly=Utilities	Utilities  5   	   G CmSlideViewer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:mSlideViewermSlideViewer 5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U<clr-namespace:System.Windows.Input;assembly=PresentationCoreSystem.Windows.Input 5   	      mSlideViewer.OptionDialog   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   ;util*clr-namespace:Utilities;assembly=Utilities     5   	   ,localclr-namespace:mSlideViewer     5   	   Ninput<clr-namespace:System.Windows.Input;assembly=PresentationCore     5   	     � Title$  Preferences��5   	   $	��600��6   $	��550��6*    � Icon$, &Resources/Icons/mSlideViewer-16X16.ico��66    � 
ResizeMode$ NoResize=�6d   .��5      �  � RowDefinitions 5	   
   �� 5
      $
��521* �6   �� 5      $	��48* �6   .+�5   
   �� -   5         ��Name$ tabControlOptions��5      $��2��60   .@�5      �� $��General��6   .��5      � ���	#FFE5E5E5$��	0,-1,0,51q�6.   .+�5         Utilities.NumericUpDown  $
��Left=�6)   $��
32,196,0,0q�6D   $	��Top=�6X   $��20��6p   $��90��6|    J� RenderTransformOrigin$ 0.5,0.5&�6�    �� IsInputMethodEnabled .  5      �� 5      -   5         ��Name$ button1��5      $��Button��6.   $
��Left=�6?   $��
32,171,0,0q�6Z   $	��Top=�6n   $��75��6�   �� 5      -   5         ��Name$ cbSaveSnapShotWhenExit��5      $��Save SnapShot When Exit.��6?   $
��Left=�6b   $��
32,117,0,0q�6}   $	��Top=�6�   �� 5      -   5        $ cbCTRLforScale��5      $)��#CTRL is used to scale image viewer.��67   $
��Left=�6e   $��
32,135,0,0q�6�   $	��Top=�6�   5      5      �� 5      $��
Annotation��6   .��5      � ���	#FFE5E5E5.+�5      �� -   5        $ checkBox��5      $
��Left=�61   $��	10,40,0,0q�6L   $	��Top=�6_   $	��487��6w   $��35��6�   .��5      �� $
��Wrap=�6$   $v��pWhen open the slide Image, opening the relative annotation file with the same filename in the same file folder. ��5      5      �� 5      -   5        $ 	checkBox1��5      $
��Left=�62   $��	10,80,0,0q�6M   $	��Top=�6`   $	��487��6x   $��35��6�   .��5      �� $
��Wrap=�6$   $S��MWhen application closed, save the relative annotation for the current slide. ��5       5!      5#      5$      �� 5%      $��SMTP Server��6   .��5&      � ���	#FFE5E5E56   5'      �� 5)      $��
HTTP Proxy��6   .��5*      � ���	#FFE5E5E56   5+      �� 5,      -   5        	 ��Name$	 
mSlideTalk��5,      $��
mSlideTalk��6+   .��5-      � ���	#FFE5E5E5.+�5.      �� -   5        
 ��Name$
 gbTalkServer��5.      $��Server Configuration��65   $
��Left=�6S   $	��190��6n   $��	15,14,0,0q�6{   $	��Top=�6�   $	��507��6�   .��5/      � $
��Left=�6   $	��175��6:   $��
4,-6,0,-16q�6G   $	��Top=�6[   $	��470��6s    � ColumnDefinitions 50      �� 51   "   .+�53      �� -	   5         ��Name$ labelTalkApplication��53   $   $��mSlideTalk Application:��6B   $
��Left=�6d   $��	13,10,0,0q�6   $	��Top=�6�   $	��145��6�   �� 54      -
   5         ��Name$ txTalkApplication��54   &   $
��Left=�6A   $��23��6\   $��	13,36,0,0q�6h   $	��Top=�6{   $	��420��6�    �� 
IsReadOnly . 6�   �� 55      -   5        $ tbTalkApplicationPath��55   %   $
��Left=�6D   $��20��6_   $��
435,38,0,0q�6k   $	��Top=�6   $��20��6�   $��0q�6�   .��56   "   �� $!��Resources/Images/exe_24.png��6(   57      �� 58      -   5        $ labelLoadParameters��58   $   $��Load Parameters:��6A   $
��Left=�6\   $��	13,59,0,0q�6w   $	��Top=�6�   $	��145��6�   $ 0.448,7.462&�6�   �� 59      -   5        $ tbTalkLoadParameters��59   &   $
��Left=�6D   $��23��6_   $��	13,85,0,0q�6k    �� TextWrapping$
 Wrap=�6~   $	��Top=�6�   $	��420��6�   �� 5:      -   5        $ labelTalkServer��5:   $   $��mSlideTalk Server:��6=   $
��Left=�6Z   $��
13,108,0,0q�6u   $	��Top=�6�   $	��145��6�       Utilities.IPAddressControl  5;      -   5          Name$ txTalkServerIP��5;   4    � ToolTip$ IP Address: 192.168.0.1��6M   $��13,134,298,18q�6o   $��1q�6�   #��� ��6�   $��10(�6�   �� 5<      -   5        $ tbServerConnectionTest��5<   %   $��Server Connection Test��6E   $
��Left=�6g   $��23��6�   $��200,134,0,0q�6�   $	��Top=�6�   $	��140��6�   6�   �� 5=      -   5        $ labelConnectionTestResult��5=   $   $��Result��6G   $
��Left=�6X   $��23��6s   $��357,134,0,0q�6   $	��Top=�6�   $��45��6�   5>      5?      � 5@      $
��Left=�6   $	��180��66   $��
10,340,0,0q�6C   $	��Top=�6W   $	��512��6o   .+�5A      �� -   5        $ button��5A   !   $��Button��61   $
��Left=�6B   $��	27,10,0,0q�6]   $	��Top=�6p   $��75��6�   5B      �� 5C      -   5        $
 groupBox��5C      $��GroupBox��61   $
��Left=�6C   $��
15,230,0,0q�6^   $	��Top=�6r   $��85��6�   $	��507��6�   5D      5E      �� 5F      $��Pathology Glossary��6   .��5G      � ���	#FFE5E5E5$��0,0,0,48q�6.    5H      �� 5I      $	��10* �6(   �� 5J      $��26 �6(   �� 5K      $��26 �6(   �� 5L      $
��209* �6(   .+�5P      �� -   5        $" cbFavoriteItemsLoadOnStartup��5P      $:��4Load the default favorite glossary items on startup.��6E   $��
40,9,159,0q�6�   $��1��6�   �� 5Q      -   5        $ cbFavoriteItemsSaveOnExit��5Q      $3��-Save the favorite glossary items on app exit.��6B   $��
40,5,159,0q�6z   $��2��6�   5R      5S      5V      �� 5X   
   -   5        $
 btOk��5X      $��OK��6   $��356,10,112,15q�6,    �� 	IsDefault . $��1��6T   6a   �� 5Y   
   -   5        $ btCancel��5Y      $��Cancel��6$   $��444,10,0,15q�65    �� IsCancel . $��1��6Z   $
��Left=�6g   $��74��6�   5[      5\      