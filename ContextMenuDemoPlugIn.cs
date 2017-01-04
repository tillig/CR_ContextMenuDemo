/**********************************************************
 *                                                        *
 *  CR_ContextMenuDemo                                    *
 *  Feb. 2, 2005                                          *
 *  Written by Travis Illig                               *
 *  http://www.paraesthesia.com                           *
 *                                                        *
 *  This plugin demonstrates how to manipulate context    *
 *  menus from within the DXCore framework.               *
 *                                                        *
 *  This code is free, no warranties expressed or         *
 *  implied.  Use at your own risk, etc.                  *
 *                                                        *
 **********************************************************/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;
using DevExpress.CodeRush.Core;
using DevExpress.CodeRush.Menus;
using DevExpress.CodeRush.PlugInCore;
using DevExpress.CodeRush.StructuralParser;

namespace CR_ContextMenuDemo {
	/// <summary>
	/// DXCore plugin demonstrating use of context menus.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This plugin demonstrates how to manipulate context menus from within
	/// the DXCore.  Concepts illustrated include:
	/// </para>
	/// <list type="bullet">
	/// <item><term>Creating a context menu.</term></item>
	/// <item><term>Adding an arbitrary number of items to the context menu.</term></item>
	/// <item><term>Conditionally showing the context menu based on a specific DXCore context.</term></item>
	/// <item><term>Showing feedback based on a click of a context menu item.</term></item>
	/// <item><term>Internationalizing strings displayed in the context menu.</term></item>
	/// </list>
	/// <note>
	/// This plugin and source code are free, no warranties expressed or implied.  Use at
	/// your own risk, etc.
	/// </note>
	/// </remarks>
	/// <seealso href="http://www.paraesthesia.com">Travis Illig [Plugin Author]</seealso>
	/// <seealso href="http://www.devexpress.com/dxcore">DXCore Product Page/Download</seealso>
	public class ContextMenuDemoPlugIn: StandardPlugIn {
	  
		#region ContextMenuDemoPlugIn Variables
   
		/// <summary>
		/// Events object providing events in the environment.
		/// </summary>
		private DevExpress.CodeRush.PlugInCore.CodeRushEvents codeRushEvents1;

		/// <summary>
		/// Components contained in this plugin.
		/// </summary>
		private System.ComponentModel.IContainer components;
		
		/// <summary>
		/// A flag indicating if the right mouse button is the currently active button.
		/// </summary>
		private bool rightMouseButtonActivated = false;

		/// <summary>
		/// The demo context menu this plugin will be working with.
		/// </summary>
		private IMenuPopup demoContextMenu = null;

		/// <summary>
		/// A resource manager allowing us to internationalize strings.
		/// </summary>
		private ResourceManager resourceManager = null;
		
		/// <summary>
		/// The big feedback that will show when a demo button is clicked.
		/// </summary>
		private DevExpress.CodeRush.Core.BigFeedback bigFeedback1;
		
		/// <summary>
		/// A random number generator that will help us add a random number of items
		/// to the demo context menu.
		/// </summary>
		private Random random = new Random();
		
		#endregion
   
   
   
		#region ContextMenuDemoPlugIn Implementation
   
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ContextMenuDemoPlugIn"/> class.
		/// </summary>
		public ContextMenuDemoPlugIn() {
			
			// Required for Windows.Forms Class Composition Designer support.
			InitializeComponent();
		}
   
		#endregion
   
		#region Overrides

		/// <summary>
		/// Executes initialization code.
		/// </summary>
		public override void InitializePlugIn() {
			base.InitializePlugIn();

			// Create resource manager for string localization.
			resourceManager = new ResourceManager("CR_ContextMenuDemo.resources.DemoResources", typeof(ContextMenuDemoPlugIn).Assembly);

			// Handles a mouse-down event in the editor window.
			codeRushEvents1.EditorMouseDown += new EditorMouseEventHandler(codeRushEvents1_EditorMouseDown);

			// Handles a mouse-up event in the editor window.
			codeRushEvents1.EditorMouseUp += new EditorMouseEventHandler(codeRushEvents1_EditorMouseUp);
		}

		/// <summary>
		/// Executes finalization/cleanup code.
		/// </summary>
		public override void FinalizePlugIn() {
			base.FinalizePlugIn();
		}
   
		#endregion
   
		#region Event Handlers

		/// <summary>
		/// Handles a mouse-button-down event in the editor window.
		/// </summary>
		/// <param name="ea">Event arguments.</param>
		private void codeRushEvents1_EditorMouseDown(EditorMouseEventArgs ea) {
			if(ea.Button == MouseButtons.Right){
				// If the right mouse button was depressed, flag that it is active.
				// The context menu will be drawn if the right mouse button is
				// active and released.
				this.rightMouseButtonActivated = true;
			}

		}

		/// <summary>
		/// Handles a mouse-button-up event in the editor window.
		/// </summary>
		/// <param name="ea">Event arguments.</param>
		private void codeRushEvents1_EditorMouseUp(EditorMouseEventArgs ea) {
			if(!this.rightMouseButtonActivated){
				// The right mouse button wasn't activated, so we don't need
				// to draw the context menu.
				return;
			}

			try{
				// Retrieve the code editor context menu from the VSCore manager.
				MenuBar editorContextMenu = DevExpress.CodeRush.VSCore.Manager.Menus.Bars[VsCommonBar.EditorContext];
				
				// If we already have a demo context menu, clear the contents so
				// we can regenerate them with the newly available items.
				if(this.demoContextMenu != null){
					// Remove the items in descending order so the item
					// collection doesn't reorder on you mid-removal.
					for(int i = this.demoContextMenu.Count - 1; i >= 0; i--){
						this.demoContextMenu[i].Delete();
					}

					// Delete the menu itself
					this.demoContextMenu.Delete();
					this.demoContextMenu = null;
				}

				// Check to see if we've satisfied our context before redrawing the
				// menu.  In this case, we'll ensure we're in an XML doc comment
				// before we show our test menu.  Contexts are written in "path"
				// form and can be seen in the template editing option page.
				if(!CodeRush.Context.Satisfied(@"Editor\Code\InXmlDocComment", true)){
					return;
				}

				// Add the demo context menu to the editor context menu.
				this.demoContextMenu = editorContextMenu.AddPopup();

				// Set the name of the context menu.  Localize the string via resources.
				this.demoContextMenu.Caption = resourceManager.GetString("MenuCaption");

				// Randomly select a number of test items to add to the new context menu.
				// By adding a random number of items, the context menu should change
				// when we select it, showing that different options can be available at
				// different times.
				int numItemsToAdd = random.Next(4, 9);

				// Add the menu items.
				for(int i = 0; i < numItemsToAdd; i++){
					// Get a "key" for the button.  This will be used for
					// the resource string key and for the button "tag."
					string buttonKey = String.Format("TestItem{0}", i);

					// Create a button object and add it to the context menu.
					IMenuButton newButton = this.demoContextMenu.AddButton();

					// Set the caption on the button.  Localize the string via resources.
					newButton.Caption = resourceManager.GetString(buttonKey);

					// Set the tag on the button.  This will be used to uniquely
					// identify the button when it is clicked and act on it.
					newButton.Tag = buttonKey;
					
					// Place a separator at the fifth button to illustrate
					// grouping of items.
					if(i == 5){
						newButton.BeginGroup = true;
					}

					// Add a button click event handler.
					newButton.Click += new MenuButtonClickEventHandler(contextMenuButton_Click);
				}
			}
			finally{
				// The right mouse button has been released, so remove the
				// active flag.
				this.rightMouseButtonActivated = false;
			}
		}

		/// <summary>
		/// Handles the click event for demo context menu buttons.
		/// </summary>
		/// <param name="sender">The button being clicked.</param>
		/// <param name="e">Event arguments.</param>
		private void contextMenuButton_Click(object sender, MenuButtonClickEventArgs e) {
			// Do something with the button click.  In this case, we'll show the
			// big feedback with the name of the item clicked.
			this.bigFeedback1.Text = resourceManager.GetString(e.Button.Tag);
			this.bigFeedback1.Show();
		}

		#endregion
   
		#region Methods
   
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.codeRushEvents1 = new DevExpress.CodeRush.PlugInCore.CodeRushEvents(this.components);
			this.bigFeedback1 = new DevExpress.CodeRush.Core.BigFeedback(this.components);
			((System.ComponentModel.ISupportInitialize)(this.codeRushEvents1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
			// 
			// bigFeedback1
			// 
			this.bigFeedback1.Text = "DemoMenu";
			((System.ComponentModel.ISupportInitialize)(this.codeRushEvents1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this)).EndInit();

		}
   
		#endregion
   
		#endregion

	}
}