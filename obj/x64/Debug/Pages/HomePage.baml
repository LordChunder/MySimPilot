   M S B A M L   `   `   `  ����E  AMySimPilot, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:MySimPilot.PagesMySimPilot.Pages  5      'clr-namespace:MySimPilot
MySimPilot  5      [ WSystem.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089Y@clr-namespace:System.Windows.Forms;assembly=System.Windows.FormsSystem.Windows.Forms 5	          MySimPilot.Pages.HomePage   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      0localclr-namespace:MySimPilot.Pages     5      b ^MaterialDesignThemes.Wpf, Version=4.2.0.2657, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5      )
mySimPilotclr-namespace:MySimPilot  5      Lforms@clr-namespace:System.Windows.Forms;assembly=System.Windows.Forms  5	         MaterialDesignBody#���   5
      $��Regular"�5      $��13(�5      &   System.Windows.Media.TextOptions    TextFormattingMode$  Ideal=�5         TextRenderingMode$
 Auto=�5       ! MaterialDesignLightBackground N� 
Background# �  5        MaterialDesignFont N� 
FontFamily# �  5       N� Title$ 
MySimPilot��5      $	��480��6   $	��800��6'   $	��800��63   $	��480��6B   .��5      � .+�5      �� $	��220��6   $��
0,11,0,-11q�6$   $��23��68   $
��Left=�6D    �� TextAlignment$ Right=�6_   $	��Top=�5      
Connection5      #  MaterialDesignThemes.Wpf.Card  5      $��Center=�6"   $	��Top=�6?   $	��320��6W   $��64,16,0,-11q�6c   $	��128��5   "   
�������.��5      �� $	��300��6   $	��110��6$   ��61   �� *SUserAircraftMetaInfo+5        5      $��Center=�6"   $	��Top=�6?   $	��320��6W   $��64,164,0,-11q�6c   $	��128��5   "   
�������.��5      �� $	��300��6   $	��110��6$   ��5      �� *SUserSimulationInfo+5      �� 5       -   5        $��
BtnConnect��5       
�������
�������$
��Left=�5!      $��	29,68,0,0q�60   $	��Top=�6C   $	��200��6[   $��48��6g   ��5    L   �� *SConnectButtonLabel+��6t   �� *CmdToggleConnect+5!   g   A� 5$      $
��Left=�6   $��18��61   $��
224,10,0,0q�6=   
���   �$	��Top=�6`   $��18��5%      ��5&      �� 5'       �� 
TargetType" A�6    �� Triggers 5(      �� 5)       �� Binding 6*   �� *
BConnected+	 �� Value		 false6I   .a�5*   "   �� 
 �� Property
 � �� �� Value �	#FFFF503069   5+      �� 5,       6*   �� *
BConnected+	 true6I   .a�5-   "   �� 
 � �� �	#FF50FF3069   5.      �� 5/       6+   �� *BSimVarTickOdd+	 true6N   .a�51   "   �� 
 � �� �	#FF45E2F469   52      54      56      �� 5:      $��Messages��6   $��Stretch=�6'   $
��Left=�6L   $��
10,180,0,0q�5;      $	��Top=�5<      $��23��6,   $��83��5=        5>      $	��236��6"   
�������$
��Left=�5?   "   $��10,205,0,10q�6=   .��5@      ��  J� RenderTransformOrigin$ 0.5,0.5&�68   $��Disabled=�5A      ��5@      �� *	LMessages+��5C      �� 5D      " ��6   .�5E      �� 
 � |� .  66   5F      ��5H      �� 5I       �� DataType$ forms:Message��6#   .R�5J      � $	��0,2q�6   .+�5K      �� $
��Wrap=�6_   ��6(   �� *Body+��6>   �� *	TextColor+6_   5L      5M      5O      5P      5R      5S      