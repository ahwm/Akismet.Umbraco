using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http.ModelBinding;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi.Filters;

namespace Akismet.Umbraco
{
    [Tree("akismet", "akismetTree", TreeTitle = "Akismet", SortOrder = 1)]
    [PluginController("akismet")]
    public class AkismetTreeController : TreeController
    {
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            root.RoutePath = string.Format("{0}/{1}/{2}", Constants.Applications.Settings, "akismetTree", "overview");

            // set the icon
            //root.Icon = "icon-wrench";
            // could be set to false for a custom tree with a single node.
            root.HasChildren = true;
            //url for menu
            root.MenuUrl = null;

            return root;
        }

        protected override MenuItemCollection GetMenuForNode(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                // root actions, perhaps users can create new items in this tree, or perhaps it's not a content tree, it might be a read only tree, or each node item might represent something entirely different...
                // add your menu item actions or custom ActionMenuItems
                menu.Items.Add(new CreateChildEntity(Services.TextService));
                // add refresh menu item (note no dialog)
                menu.Items.Add(new RefreshNode(Services.TextService, true));
                return menu;
            }

            return menu;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormDataCollection queryStrings)
        {
            // check if we're rendering the root node's children
            if (id == Constants.System.Root.ToInvariantString())
            {
                // create our node collection
                var nodes = new TreeNodeCollection
                {
                    CreateTreeNode("1", "-1", queryStrings, "Configuration", "icon-wrench", false, "akismet/configuration"),
                    CreateTreeNode("2", "-1", queryStrings, "Spam Queue", "icon-conversation-alt", false, "akismet/spam-queue"),
                    CreateTreeNode("3", "-1", queryStrings, "Comments", "icon-chat-active", false, "akismet/comments"),
                };

                return nodes;
            }

            // this tree doesn't support rendering more than 1 level
            throw new NotSupportedException();
        }
    }
}