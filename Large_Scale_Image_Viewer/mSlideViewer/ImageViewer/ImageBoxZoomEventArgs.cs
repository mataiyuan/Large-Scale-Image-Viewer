using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mSlideViewer
{
    /// <summary>
    /// Contains event data for the <see cref="ImageBox.ZoomChanged"/> event.
    /// </summary>
    public class ImageBoxZoomEventArgs : EventArgs
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageBoxZoomEventArgs"/> class.
        /// </summary>
        /// <param name="actions">The zoom operation being performed.</param>
        /// <param name="source">The source of the operation.</param>
        /// <param name="oldZoom">The old zoom level.</param>
        /// <param name="newZoom">The new zoom level.</param>
        public ImageBoxZoomEventArgs(ImageBoxZoomActions actions,  int oldZoom, int newZoom)
            : this()
        {
            this.Actions = actions;            
            this.OldZoom = oldZoom;
            this.NewZoom = newZoom;
        }

        #endregion

        #region Protected Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageBoxZoomEventArgs"/> class.
        /// </summary>
        protected ImageBoxZoomEventArgs()
        { 
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the actions that occured.
        /// </summary>
        /// <value>The zoom operation.</value>
        public ImageBoxZoomActions Actions { get; protected set; }

        /// <summary>
        /// Gets or sets the new zoom level.
        /// </summary>
        /// <value>The new zoom level.</value>
        public int NewZoom { get; protected set; }

        /// <summary>
        /// Gets or sets the old zoom level.
        /// </summary>
        /// <value>The old zoom level.</value>
        public int OldZoom { get; protected set; }
     
        #endregion
    }
}
