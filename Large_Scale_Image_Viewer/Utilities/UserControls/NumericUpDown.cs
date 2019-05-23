using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Utilities
{
    [TemplatePart(Name = "PART_TextBox", Type = typeof (TextBox))]
    [TemplatePart(Name = "PART_IncreaseButton", Type = typeof (RepeatButton))]
    [TemplatePart(Name = "PART_DecreaseButton", Type = typeof (RepeatButton))]
    public class NumericUpDown : Control
    {
        #region Properties

        #region Value

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof (Decimal), typeof (NumericUpDown),
                                        new PropertyMetadata(0m, OnValueChanged, CoerceValue));

        public Decimal Value
        {
            get { return (Decimal) GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject element,
                                           DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericUpDown) element;

            control.TextBox.UndoLimit = 0;
            control.TextBox.UndoLimit = 1;
        }

        private static object CoerceValue(DependencyObject element, object baseValue)
        {
            var control = (NumericUpDown) element;
            var value = (Decimal) baseValue;

            control.CoerceValueToBounds(ref value);

            // Get the text representation of Value
            var valueString = value.ToString(control.Culture);

            // Count all decimal places
            var decimalPlaces = control.GetDecimalPlacesCount(valueString);

            if (decimalPlaces > control.DecimalPlaces)
            {
                if (control.IsDecimalPointDynamic)
                {
                    // Assigning DecimalPlaces will coerce the number
                    control.DecimalPlaces = decimalPlaces;

                    // If the specified number of decimal places is still too much
                    if (decimalPlaces > control.DecimalPlaces)
                    {
                        value = control.TruncateValue(valueString, control.DecimalPlaces);
                    }
                }
                else
                {
                    // Remove all overflowing decimal places
                    value = control.TruncateValue(valueString, decimalPlaces);
                }
            }
            else if (control.IsDecimalPointDynamic)
            {
                control.DecimalPlaces = decimalPlaces;
            }

            if (control.IsThousandSeparatorVisible)
            {
                if (control.TextBox != null)
                {
                    control.TextBox.Text = value.ToString("N", control.Culture);
                }
            }
            else
            {
                if (control.TextBox != null)
                {
                    control.TextBox.Text = value.ToString("F", control.Culture);
                }
            }

            return value;
        }

        #endregion

        #region MaxValue

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof (Decimal), typeof (NumericUpDown),
                                        new PropertyMetadata(100000000m, OnMaxValueChanged,
                                                             CoerceMaxValue));

        public Decimal MaxValue
        {
            get { return (Decimal) GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        private static void OnMaxValueChanged(DependencyObject element,
                                              DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericUpDown) element;
            var maxValue = (Decimal) e.NewValue;

            // If maxValue steps over MinValue, shift it
            if (maxValue < control.MinValue)
            {
                control.MinValue = maxValue;
            }

            if (maxValue <= control.Value)
            {
                control.Value = maxValue;
            }
        }

        private static object CoerceMaxValue(DependencyObject element, Object baseValue)
        {
            var maxValue = (Decimal) baseValue;

            if (maxValue == Decimal.MaxValue)
            {
                return DependencyProperty.UnsetValue;
            }

            return maxValue;
        }

        #endregion

        #region MinValue

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof (Decimal), typeof (NumericUpDown),
                                        new PropertyMetadata(0m, OnMinValueChanged,
                                                             CoerceMinValue));

        public Decimal MinValue
        {
            get { return (Decimal) GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        private static void OnMinValueChanged(DependencyObject element,
                                              DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericUpDown) element;
            var minValue = (Decimal) e.NewValue;

            // If minValue steps over MaxValue, shift it
            if (minValue > control.MaxValue)
            {
                control.MaxValue = minValue;
            }

            if (minValue >= control.Value)
            {
                control.Value = minValue;
            }
        }

        private static object CoerceMinValue(DependencyObject element, Object baseValue)
        {
            var minValue = (Decimal) baseValue;

            if (minValue == Decimal.MinValue)
            {
                return DependencyProperty.UnsetValue;
            }

            return minValue;
        }

        #endregion

        #region DecimalPlaces

        public static readonly DependencyProperty DecimalPlacesProperty =
            DependencyProperty.Register("DecimalPlaces", typeof (Int32), typeof (NumericUpDown),
                                        new PropertyMetadata(0, OnDecimalPlacesChanged,
                                                             CoerceDecimalPlaces));

        public Int32 DecimalPlaces
        {
            get { return (Int32) GetValue(DecimalPlacesProperty); }
            set { SetValue(DecimalPlacesProperty, value); }
        }

        private static void OnDecimalPlacesChanged(DependencyObject element,
                                                   DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericUpDown) element;
            var decimalPlaces = (Int32) e.NewValue;

            control.Culture.NumberFormat.NumberDecimalDigits = decimalPlaces;

            if (control.IsDecimalPointDynamic)
            {
                control.IsDecimalPointDynamic = false;
                control.InvalidateProperty(ValueProperty);
                control.IsDecimalPointDynamic = true;
            }
            else
            {
                control.InvalidateProperty(ValueProperty);
            }
        }

        private static object CoerceDecimalPlaces(DependencyObject element, Object baseValue)
        {
            var decimalPlaces = (Int32) baseValue;
            var control = (NumericUpDown) element;

            if (decimalPlaces < control.MinDecimalPlaces)
            {
                decimalPlaces = control.MinDecimalPlaces;
            }
            else if (decimalPlaces > control.MaxDecimalPlaces)
            {
                decimalPlaces = control.MaxDecimalPlaces;
            }

            return decimalPlaces;
        }

        #endregion

        #region MaxDecimalPlaces

        public static readonly DependencyProperty MaxDecimalPlacesProperty =
            DependencyProperty.Register("MaxDecimalPlaces", typeof (Int32), typeof (NumericUpDown),
                                        new PropertyMetadata(28, OnMaxDecimalPlacesChanged,
                                                             CoerceMaxDecimalPlaces));

        public Int32 MaxDecimalPlaces
        {
            get { return (Int32) GetValue(MaxDecimalPlacesProperty); }
            set { SetValue(MaxDecimalPlacesProperty, value); }
        }

        private static void OnMaxDecimalPlacesChanged(DependencyObject element,
                                                      DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericUpDown) element;

            control.InvalidateProperty(DecimalPlacesProperty);
        }

        private static object CoerceMaxDecimalPlaces(DependencyObject element, Object baseValue)
        {
            var maxDecimalPlaces = (Int32) baseValue;
            var control = (NumericUpDown) element;

            if (maxDecimalPlaces > 28)
            {
                maxDecimalPlaces = 28;
            }
            else if (maxDecimalPlaces < 0)
            {
                maxDecimalPlaces = 0;
            }
            else if (maxDecimalPlaces < control.MinDecimalPlaces)
            {
                control.MinDecimalPlaces = maxDecimalPlaces;
            }

            return maxDecimalPlaces;
        }

        #endregion

        #region MinDecimalPlaces

        public static readonly DependencyProperty MinDecimalPlacesProperty =
            DependencyProperty.Register("MinDecimalPlaces", typeof (Int32), typeof (NumericUpDown),
                                        new PropertyMetadata(0, OnMinDecimalPlacesChanged,
                                                             CoerceMinDecimalPlaces));

        public Int32 MinDecimalPlaces
        {
            get { return (Int32) GetValue(MinDecimalPlacesProperty); }
            set { SetValue(MinDecimalPlacesProperty, value); }
        }

        private static void OnMinDecimalPlacesChanged(DependencyObject element,
                                                      DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericUpDown) element;

            control.InvalidateProperty(DecimalPlacesProperty);
        }

        private static object CoerceMinDecimalPlaces(DependencyObject element, Object baseValue)
        {
            var minDecimalPlaces = (Int32) baseValue;
            var control = (NumericUpDown) element;

            if (minDecimalPlaces < 0)
            {
                minDecimalPlaces = 0;
            }
            else if (minDecimalPlaces > 28)
            {
                minDecimalPlaces = 28;
            }
            else if (minDecimalPlaces > control.MaxDecimalPlaces)
            {
                control.MaxDecimalPlaces = minDecimalPlaces;
            }

            return minDecimalPlaces;
        }

        #endregion

        #region IsDecimalPointDynamic

        public static readonly DependencyProperty IsDecimalPointDynamicProperty =
            DependencyProperty.Register("IsDecimalPointDynamic", typeof (Boolean), typeof (NumericUpDown),
                                        new PropertyMetadata(false));

        public Boolean IsDecimalPointDynamic
        {
            get { return (Boolean) GetValue(IsDecimalPointDynamicProperty); }
            set { SetValue(IsDecimalPointDynamicProperty, value); }
        }

        #endregion

        #region MinorDelta

        public static readonly DependencyProperty MinorDeltaProperty =
            DependencyProperty.Register("MinorDelta", typeof (Decimal), typeof (NumericUpDown),
                                        new PropertyMetadata(1m, OnMinorDeltaChanged,
                                                             CoerceMinorDelta));

        public Decimal MinorDelta
        {
            get { return (Decimal) GetValue(MinorDeltaProperty); }
            set { SetValue(MinorDeltaProperty, value); }
        }

        private static void OnMinorDeltaChanged(DependencyObject element,
                                                DependencyPropertyChangedEventArgs e)
        {
            var minorDelta = (Decimal) e.NewValue;
            var control = (NumericUpDown) element;

            if (minorDelta > control.MajorDelta)
            {
                control.MajorDelta = minorDelta;
            }
        }

        private static object CoerceMinorDelta(DependencyObject element, Object baseValue)
        {
            var minorDelta = (Decimal) baseValue;

            return minorDelta;
        }

        #endregion

        #region MajorDelta

        public static readonly DependencyProperty MajorDeltaProperty =
            DependencyProperty.Register("MajorDelta", typeof (Decimal), typeof (NumericUpDown),
                                        new PropertyMetadata(10m, OnMajorDeltaChanged,
                                                             CoerceMajorDelta));

        public Decimal MajorDelta
        {
            get { return (Decimal) GetValue(MajorDeltaProperty); }
            set { SetValue(MajorDeltaProperty, value); }
        }

        private static void OnMajorDeltaChanged(DependencyObject element,
                                                DependencyPropertyChangedEventArgs e)
        {
            var majorDelta = (Decimal) e.NewValue;
            var control = (NumericUpDown) element;

            if (majorDelta < control.MinorDelta)
            {
                control.MinorDelta = majorDelta;
            }
        }

        private static object CoerceMajorDelta(DependencyObject element, Object baseValue)
        {
            var majorDelta = (Decimal) baseValue;

            return majorDelta;
        }

        #endregion

        #region IsThousandSeparatorVisible

        public static readonly DependencyProperty IsThousandSeparatorVisibleProperty =
            DependencyProperty.Register("IsThousandSeparatorVisible", typeof (Boolean), typeof (NumericUpDown),
                                        new PropertyMetadata(false, OnIsThousandSeparatorVisibleChanged));

        public Boolean IsThousandSeparatorVisible
        {
            get { return (Boolean) GetValue(IsThousandSeparatorVisibleProperty); }
            set { SetValue(IsThousandSeparatorVisibleProperty, value); }
        }

        private static void OnIsThousandSeparatorVisibleChanged(DependencyObject element,
                                                                DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericUpDown) element;

            control.InvalidateProperty(ValueProperty);
        }

        #endregion

        #region IsAutoSelectionActive

        public static readonly DependencyProperty IsAutoSelectionActiveProperty =
            DependencyProperty.Register("IsAutoSelectionActive", typeof (Boolean), typeof (NumericUpDown),
                                        new PropertyMetadata(false));

        public Boolean IsAutoSelectionActive
        {
            get { return (Boolean) GetValue(IsAutoSelectionActiveProperty); }
            set { SetValue(IsAutoSelectionActiveProperty, value); }
        }

        #endregion

        #region IsValueWrapAllowed

        public static readonly DependencyProperty IsValueWrapAllowedProperty =
            DependencyProperty.Register("IsValueWrapAllowed", typeof (Boolean), typeof (NumericUpDown),
                                        new PropertyMetadata(false));

        public Boolean IsValueWrapAllowed
        {
            get { return (Boolean) GetValue(IsValueWrapAllowedProperty); }
            set { SetValue(IsValueWrapAllowedProperty, value); }
        }

        #endregion

        #endregion

        #region Fields

        protected readonly CultureInfo Culture;
        protected RepeatButton DecreaseButton;
        protected RepeatButton IncreaseButton;
        protected TextBox TextBox;

        #endregion

        #region Commands

        private readonly RoutedUICommand _minorDecreaseValueCommand =
            new RoutedUICommand("MinorDecreaseValue", "MinorDecreaseValue", typeof (NumericUpDown));

        private readonly RoutedUICommand _minorIncreaseValueCommand =
            new RoutedUICommand("MinorIncreaseValue", "MinorIncreaseValue", typeof (NumericUpDown));

        private readonly RoutedUICommand _majorDecreaseValueCommand =
            new RoutedUICommand("MajorDecreaseValue", "MajorDecreaseValue", typeof (NumericUpDown));

        private readonly RoutedUICommand _majorIncreaseValueCommand =
            new RoutedUICommand("MajorIncreaseValue", "MajorIncreaseValue", typeof (NumericUpDown));

        private readonly RoutedUICommand _updateValueStringCommand =
            new RoutedUICommand("UpdateValueString", "UpdateValueString", typeof (NumericUpDown));

        private readonly RoutedUICommand _cancelChangesCommand =
            new RoutedUICommand("CancelChanges", "CancelChanges", typeof (NumericUpDown));

        #endregion

        #region Constructors

        static NumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (NumericUpDown),
                                                     new FrameworkPropertyMetadata(
                                                         typeof (NumericUpDown)));
        }

        public NumericUpDown()
        {
            Culture = (CultureInfo) CultureInfo.CurrentCulture.Clone();

            Culture.NumberFormat.NumberDecimalDigits = DecimalPlaces;

            Loaded += OnLoaded;
        }

        #endregion

        #region Event handlers

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            AttachToVisualTree();
            AttachCommands();
        }

        private void TextBoxOnLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            UpdateValue();
        }

        private void TextBoxOnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (IsAutoSelectionActive)
            {
                TextBox.SelectAll();
            }
        }

        private bool occur = false;

        public String Text
        {
            get
            {
                return TextBox.Text;
            }
            set
            {
                TextBox.Text = LeaveOnlyNumbers(value);
            }
        }

        protected void TextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = sender as TextBox;
            string text = LeaveOnlyNumbers(txt.Text);
        }

        private string LeaveOnlyNumbers(String inString)
        {
            occur = false;
            System.Text.StringBuilder ans = new System.Text.StringBuilder();

            for (int i = 0; i < inString.Length; ++i)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(inString[i].ToString(), "^[0-9//.]*$"))
                {
                    if (inString[i] == '.')
                    {
                        if (!occur)
                        {
                            occur = true; ans.Append(inString[i]);
                        }
                    }
                    else ans.Append(inString[i]);
                }
            }
            return ans.ToString();
        }

        private void TextBoxOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var txt = sender as TextBox;
            e.Handled = !IsNumberKeyOrDot(e.Key) && !IsDelOrBackspaceOrTabKey(e.Key);

    
        }

        private bool IsNumberKeyOrDot(Key inKey)
        {
            if (!occur)
            {
                if (inKey == Key.Decimal)
                    return true;
                if (inKey == Key.OemPeriod)
                    return true;
            }
            if (inKey < Key.D0 || inKey > Key.D9)
            {
                if (inKey < Key.NumPad0 || inKey > Key.NumPad9)
                {
                    return false;
                }
            }
            return true;
        }
        private bool IsDelOrBackspaceOrTabKey(Key inKey)
        {
            return inKey == Key.Delete || inKey == Key.Back || inKey == Key.Tab || inKey == Key.Up || inKey == Key.Down;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            InvalidateProperty(ValueProperty);
        }

        private void ButtonOnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            Value = 0;
        }

        #endregion

        #region Utility Methods

        #region Attachment

        private void AttachToVisualTree()
        {
            AttachTextBox();
            AttachIncreaseButton();
            AttachDecreaseButton();
        }

        private void AttachTextBox()
        {
            var textBox = GetTemplateChild("PART_TextBox") as TextBox;

            // A null check is advised
            if (textBox != null)
            {
                TextBox = textBox;
                TextBox.LostFocus += TextBoxOnLostFocus;
                TextBox.PreviewMouseLeftButtonUp += TextBoxOnPreviewMouseLeftButtonUp;
                TextBox.PreviewKeyDown += new KeyEventHandler(TextBoxOnPreviewKeyDown);
                TextBox.TextChanged += new TextChangedEventHandler(TextBoxOnTextChanged);

                TextBox.UndoLimit = 1;
                TextBox.IsUndoEnabled = true;
            }
        }

        private void AttachIncreaseButton()
        {
            var increaseButton = GetTemplateChild("PART_IncreaseButton") as RepeatButton;
            if (increaseButton != null)
            {
                IncreaseButton = increaseButton;
                IncreaseButton.Focusable = false;
                IncreaseButton.Command = _minorIncreaseValueCommand;
                IncreaseButton.PreviewMouseLeftButtonDown += (sender, args) => RemoveFocus();
                IncreaseButton.PreviewMouseRightButtonDown += ButtonOnPreviewMouseRightButtonDown;
            }
        }

        private void AttachDecreaseButton()
        {
            var decreaseButton = GetTemplateChild("PART_DecreaseButton") as RepeatButton;
            if (decreaseButton != null)
            {
                DecreaseButton = decreaseButton;
                DecreaseButton.Focusable = false;
                DecreaseButton.Command = _minorDecreaseValueCommand;
                DecreaseButton.PreviewMouseLeftButtonDown += (sender, args) => RemoveFocus();
                DecreaseButton.PreviewMouseRightButtonDown += ButtonOnPreviewMouseRightButtonDown;
            }
        }

        private void AttachCommands()
        {
            CommandBindings.Add(new CommandBinding(_minorIncreaseValueCommand, (a, b) => IncreaseValue(true)));
            CommandBindings.Add(new CommandBinding(_minorDecreaseValueCommand, (a, b) => DecreaseValue(true)));
            CommandBindings.Add(new CommandBinding(_majorIncreaseValueCommand, (a, b) => IncreaseValue(false)));
            CommandBindings.Add(new CommandBinding(_majorDecreaseValueCommand, (a, b) => DecreaseValue(false)));
            CommandBindings.Add(new CommandBinding(_updateValueStringCommand, (a, b) => UpdateValue()));
            CommandBindings.Add(new CommandBinding(_cancelChangesCommand, (a, b) => CancelChanges()));

            CommandManager.RegisterClassInputBinding(typeof (TextBox),
                                                     new KeyBinding(_minorIncreaseValueCommand, new KeyGesture(Key.Up)));
            CommandManager.RegisterClassInputBinding(typeof (TextBox),
                                                     new KeyBinding(_minorDecreaseValueCommand, new KeyGesture(Key.Down)));
            CommandManager.RegisterClassInputBinding(typeof (TextBox),
                                                     new KeyBinding(_majorIncreaseValueCommand,
                                                                    new KeyGesture(Key.PageUp)));
            CommandManager.RegisterClassInputBinding(typeof (TextBox),
                                                     new KeyBinding(_majorDecreaseValueCommand,
                                                                    new KeyGesture(Key.PageDown)));
            CommandManager.RegisterClassInputBinding(typeof (TextBox),
                                                     new KeyBinding(_updateValueStringCommand, new KeyGesture(Key.Enter)));
            CommandManager.RegisterClassInputBinding(typeof (TextBox),
                                                     new KeyBinding(_cancelChangesCommand, new KeyGesture(Key.Escape)));
        }

        #endregion

        #region Data retrieval and deposit

        private Decimal ParseStringToDecimal(String source)
        {
            Decimal value;
            Decimal.TryParse(source, out value);

            return value;
        }

        public Int32 GetDecimalPlacesCount(String valueString)
        {
            return valueString.SkipWhile(c => c.ToString(Culture)
                                              != Culture.NumberFormat.NumberDecimalSeparator).Skip(1).Count();
        }

        private Decimal TruncateValue(String valueString, Int32 decimalPlaces)
        {
            var endPoint = valueString.Length - (decimalPlaces - DecimalPlaces);
            endPoint++;

            var tempValueString = valueString.Substring(0, endPoint);

            return Decimal.Parse(tempValueString, Culture);
        }

        #endregion

        #region SubCoercion

        private void CoerceValueToBounds(ref Decimal value)
        {
            if (value < MinValue)
            {
                value = MinValue;
            }
            else if (value > MaxValue)
            {
                value = MaxValue;
            }
        }

        #endregion

        #endregion

        #region Methods

        private void UpdateValue()
        {
            Value = ParseStringToDecimal(TextBox.Text);
        }

        private void CancelChanges()
        {
            TextBox.Undo();
        }
        
        private void RemoveFocus()
        {
            // Passes focus here and then just deletes it
            Focusable = true;
            Focus();
            Focusable = false;
        }

        private void IncreaseValue(Boolean minor)
        {
            // Get the value that's currently in the _textBox.Text
            decimal value = ParseStringToDecimal(TextBox.Text);

            // Coerce the value to min/max
            CoerceValueToBounds(ref value);

            // Only change the value if it has any meaning
            if (value >= MinValue)
            {
                if (minor)
                {
                    if (IsValueWrapAllowed && value + MinorDelta > MaxValue)
                    {
                        value = MinValue;
                    }
                    else
                    {
                        value += MinorDelta;
                    }
                }
                else
                {
                    if (IsValueWrapAllowed && value + MajorDelta > MaxValue)
                    {
                        value = MinValue;
                    }
                    else
                    {
                        value += MajorDelta;
                    }
                }
            }

            Value = value;
        }

        private void DecreaseValue(Boolean minor)
        {
            // Get the value that's currently in the _textBox.Text
            decimal value = ParseStringToDecimal(TextBox.Text);

            // Coerce the value to min/max
            CoerceValueToBounds(ref value);
            
            // Only change the value if it has any meaning
            if (value <= MaxValue)
            {
                if (minor)
                {
                    if (IsValueWrapAllowed && value - MinorDelta < MinValue)
                    {
                        value = MaxValue;
                    }
                    else
                    {
                        value -= MinorDelta;
                    }
                }
                else
                {
                    if (IsValueWrapAllowed && value - MajorDelta < MinValue)
                    {
                        value = MaxValue;
                    }
                    else
                    {
                        value -= MajorDelta;
                    }
                }
            }

            Value = value;
        }

        #endregion
    }
}