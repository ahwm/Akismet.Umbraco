using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;

namespace Akismet.Umbraco.Controllers
{
    [Tree("akismet", "akismet", TreeTitle = "Akismet", SortOrder = 1)]
    [PluginController("akismet")]
    public class AkismetTreeController : TreeController
    {
        private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;

        public AkismetTreeController(ILocalizedTextService localizedTextService,  UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, IMenuItemCollectionFactory menuItemCollectionFactory, IEventAggregator eventAggregator) : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
            _menuItemCollectionFactory = menuItemCollectionFactory ?? throw new ArgumentNullException(nameof(menuItemCollectionFactory));
        }

        protected override ActionResult<TreeNode> CreateRootNode(FormCollection queryStrings)
        {
            var rootResult = base.CreateRootNode(queryStrings);
            if (!(rootResult.Result is null))
            {
                return rootResult;
            }
            var root = rootResult.Value;

            root.RoutePath = string.Format("{0}/{1}", "akismet", "overview");

            // set the icon
            //root.Icon = "icon-wrench";
            // could be set to false for a custom tree with a single node.
            root.HasChildren = true;
            //url for menu
            root.MenuUrl = null;

            return root;
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        {
            var menu = _menuItemCollectionFactory.Create();

            if (id == Constants.System.Root.ToInvariantString())
            {
                // root actions, perhaps users can create new items in this tree, or perhaps it's not a content tree, it might be a read only tree, or each node item might represent something entirely different...
                // add your menu item actions or custom ActionMenuItems
                menu.Items.Add(new CreateChildEntity(LocalizedTextService));
                // add refresh menu item (note no dialog)
                menu.Items.Add(new RefreshNode(LocalizedTextService, true));
                return menu;
            }

            return menu;
        }

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
        {
            // check if we're rendering the root node's children
            if (id == Constants.System.Root.ToInvariantString())
            {
                // create our node collection
                var nodes = new TreeNodeCollection
                {
                    CreateTreeNode("1", "-1", queryStrings, "Configuration", "icon-wrench", false, "akismet/akismet/configuration"),
                    CreateTreeNode("2", "-1", queryStrings, "Spam Queue", "icon-conversation-alt", false, "akismet/akismet/spam-queue"),
                    CreateTreeNode("3", "-1", queryStrings, "Comments", "icon-chat-active", false, "akismet/akismet/comments"),
                };

                return nodes;
            }

            // this tree doesn't support rendering more than 1 level
            throw new NotSupportedException();
        }
    }
}