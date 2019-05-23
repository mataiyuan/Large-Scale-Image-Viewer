using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Utilities;
using DrawToolsLib;

namespace mSlideViewer
{
    /// <summary>
    /// Application settings class.
    /// Loaded when application starts, saved when application ends.
    /// 
    /// Contains all persistent application settings.
    /// 
    /// All class members should meet XML serialization requirements.
    /// 
    /// Can be used directly, but recommended way is to use this class
    /// together with SettingsManager class.
    /// See also notes in SettingsManager.
    /// 
    /// Since class is serialized, it is public.
    /// </summary>
    public class Settings
    {
        #region Class Members

        WindowStateInfo mainWindowStateInfo;
        MruInfo recentFilesList;

        string initialDirectory;

        // Last choice of object properties
        double lineWidth;
        Color objectColor;
        string textFontFamilyName;
        string textFontStyle;
        string textFontWeight;
        string textFontStretch;
        double textFontSize;

        // Other members go here.
        // For every member create public property.
        string _mSlideTalkAppPath;
        string _mSlideTalkServerIP;

        bool _isFavoriteItemsLoadOnStartup;
        bool _isFavoriteItemsSaveOnExit;

        bool _isSaveSnapShotOnExit;
        bool _isCTRLforScale;
        #endregion Class Members

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// Creates instances of all internal classes and sets all default values.
        /// 
        /// This prevents exception when client cannot load Settings instance from
        /// XML file - in this case default Settings instance is created.
        /// Default Settings instance should always contain valid default values.
        /// </summary>
        public Settings()
        {
            mainWindowStateInfo = new WindowStateInfo();
            recentFilesList = new MruInfo();
            initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            lineWidth = 1;
            objectColor = Colors.Black;
            textFontFamilyName = "Tahoma";
            textFontStyle = FontConversions.FontStyleToString(FontStyles.Normal);
            textFontWeight = FontConversions.FontWeightToString(FontWeights.Normal);
            textFontStretch = FontConversions.FontStretchToString(FontStretches.Normal);
            textFontSize = 12;

            _isFavoriteItemsLoadOnStartup = false;
            _isFavoriteItemsSaveOnExit = false;
            _isSaveSnapShotOnExit = false;
            _isCTRLforScale = false;
            // Set default values for other members here
            // ...
        }

        #endregion Constructor

        #region Properties

        public WindowStateInfo MainWindowStateInfo
        {
            get { return mainWindowStateInfo; }
            set { mainWindowStateInfo = value; }
        }

        public MruInfo RecentFilesList
        {
            get { return recentFilesList; }
            set { recentFilesList = value; }
        }

        public string InitialDirectory
        {
            get { return initialDirectory; }
            set { initialDirectory = value; }
        }

        public double LineWidth
        {
            get { return lineWidth; }
            set { lineWidth = value; }
        }

        public Color ObjectColor
        {
            get { return objectColor; }
            set { objectColor = value; }
        }

        public string TextFontFamilyName
        {
            get { return textFontFamilyName; }
            set { textFontFamilyName = value; }
        }

        public string TextFontStyle
        {
            get { return textFontStyle; }
            set { textFontStyle = value; }
        }

        public string TextFontWeight
        {
            get { return textFontWeight; }
            set { textFontWeight = value; }
        }

        public string TextFontStretch
        {
            get { return textFontStretch; }
            set { textFontStretch = value; }
        }

        public double TextFontSize
        {
            get { return textFontSize; }
            set { textFontSize = value; }
        }

        public string mSlideTalkAppPath
        {
            get { return _mSlideTalkAppPath; }
            set { _mSlideTalkAppPath = value; }
        }

        public string mSlideTalkServerIP
        {
            get { return _mSlideTalkServerIP; }
            set { _mSlideTalkServerIP = value; }
        }

        public bool FavoriteItemsLoadOnStartup
        {
            get { return _isFavoriteItemsLoadOnStartup; }
            set { _isFavoriteItemsLoadOnStartup = value; }
        }

        public bool FavoriteItemsSaveOnExit
        {
            get { return _isFavoriteItemsSaveOnExit; }
            set { _isFavoriteItemsSaveOnExit = value; }
        }

        public bool SaveSnapShotOnExit
        {
            get { return _isSaveSnapShotOnExit; }
            set { _isSaveSnapShotOnExit = value; }
        }        

        public bool CTRLforScale
        {
            get { return _isCTRLforScale; }
            set { _isCTRLforScale = value; }
        }
        #endregion Properties
    }
}
