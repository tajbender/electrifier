//	<file>
//		<copyright see="www.electrifier.org"/>
//		<license   see="www.electrifier.org"/>
//		<owner     name="Thorsten Jung" email="taj@electrifier.org"/>
//		<version   value="$Id: BasicShellObject.cs,v 1.13 2004/09/10 20:30:33 taj bender Exp $"/>
//	</file>

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

using WeifenLuo.WinFormsUI.Docking;

using Electrifier.Core.Shell32.Services;
using Electrifier.Core.Shell32.Controls;
using Electrifier.Win32API;

namespace Electrifier.Core.Forms.DockControls {
	public delegate void BrowsingAddressChangedEventHandler(object source, EventArgs e);

	/// <summary>
	/// Zusammenfassung f�r ShellBrowserDockControl.
	/// </summary>
	public class ShellBrowserDockControl : DockContent, IDockControl, IPersistent {
        protected Guid Guid;

        protected ShellTreeView         shellTreeView        = null;
		protected ShellBrowser          shellBrowser         = null;
		protected Splitter              splitter             = null;
		protected IDockControlContainer dockControlContainer = null;
		protected string                browsingAddress      = null;
		public    string                BrowsingAddress {
			get { return this.browsingAddress; }
		}

		public event BrowsingAddressChangedEventHandler BrowsingAddressChanged = null;
		protected void OnBrowsingAddressChanged() {
			if(this.BrowsingAddressChanged != null)
				this.BrowsingAddressChanged(this, EventArgs.Empty);
		}


		public ShellBrowserDockControl() : this(Guid.NewGuid()) { }

		public ShellBrowserDockControl(Guid guid) : base() {
			// Initialize the underlying DockControl
			Guid = guid;
			Name = "ShellBrowserDockControl." + Guid.ToString();
			Text = Name;

			// Initialize ShellTreeView
			shellTreeView              = new ShellTreeView(ShellAPI.CSIDL.DESKTOP);
			shellTreeView.Dock         = DockStyle.Left;
			shellTreeView.Size         = new Size(256, this.Size.Height);
			shellTreeView.AfterSelect +=new TreeViewEventHandler(shellTreeView_AfterSelect);

			// Initialize ShellBrowser
			shellBrowser = new ShellBrowser(shellTreeView.SelectedNode.AbsolutePIDL);
			shellBrowser.Dock = DockStyle.Fill;
			shellBrowser.BrowseShellObject += new BrowseShellObjectEventHandler(shellBrowser_BrowseShellObject);

			this.UpdateDockCaption();

			// Initialize Splitter
			splitter      = new Splitter();
			splitter.Dock = DockStyle.Left;
			splitter.Size = new Size(6, this.Height);

			// Add the controls from right to left
			Controls.AddRange(new Control[] { shellBrowser, splitter, shellTreeView });
		}

		// TODO: Dispose when closed!!!

		private void shellTreeView_AfterSelect(object sender, TreeViewEventArgs e) {
			// TODO: TreeViewEventArgs.Node => shellTreeViewNode
			// TODO: ShellTreeView.SelectedNode => shellTreeViewNode
			//this.Cursor = Cursors.WaitCursor;
			shellBrowser.SetBrowsingFolder(shellTreeView.SelectedNode.AbsolutePIDL);
//			shellListView.SetBrowsingFolder(sender, (shellTreeView.SelectedNode as ShellTreeViewNode).AbsolutePIDL);
			this.UpdateDockCaption();
			this.browsingAddress = this.Text;
			this.OnBrowsingAddressChanged();
			//this.Cursor = Cursors.Default;
		}

		protected void UpdateDockCaption() {
			this.Text = shellTreeView.SelectedNode.Text;
			this.Icon = IconManager.GetIconFromPIDL(shellTreeView.SelectedNode.AbsolutePIDL, false);
		}

		#region IDockControl Member
		public void AttachToDockControlContainer(IDockControlContainer dockControlContainer) {
			if(this.dockControlContainer == null) {
				this.dockControlContainer = dockControlContainer;
				dockControlContainer.AttachDockControl(this);
			} else {
				throw new InvalidOperationException("IDockControlContainer already set!");
			}
		}

		#endregion

		#region IPersistent Member

		public System.Xml.XmlNode CreatePersistenceInfo(System.Xml.XmlDocument targetXmlDocument) {
			XmlNode dockControlNode = targetXmlDocument.CreateElement(this.GetType().FullName);
			XmlAttribute guidAttr = targetXmlDocument.CreateAttribute("Guid");
			XmlNode browsingAddressNode = targetXmlDocument.CreateElement("BrowsingAddress");


			guidAttr.Value = Guid.ToString();
			// TODO: RELAUNCH: Set browsingadress in browsingAddressNode

			dockControlNode.Attributes.Append(guidAttr);
			dockControlNode.AppendChild(browsingAddressNode);

			return dockControlNode;
		}

		public void ApplyPersistenceInfo(System.Xml.XmlNode persistenceInfo) {
			Guid = new Guid(persistenceInfo.Attributes.GetNamedItem("Guid").Value);

			Name = "ShellBrowserDockControl." + Guid.ToString();		// TODO: TestCode
		}

		#endregion

		private void shellBrowser_BrowseShellObject(object source, BrowseShellObjectEventArgs e) {
			// Currently, the IShellView seems to know that a new folder was browsed to,
			// and updates itself accordingly
//			this.shellBrowser.SetBrowsingFolder(e.ShellObjectPIDL);
			// TODO: The following statements should be ran in another thread
			this.shellTreeView.SelectedNode = this.shellTreeView.FindNodeByPIDL(e.ShellObjectPIDL);


			// TODO: Hehe, i like this feature; however, put something into our configuration dialog
			// to turn this off; also, do this when navigating with the help of the TreeView itself
			this.shellTreeView.SelectedNode.Expand();

		}
	}
}
