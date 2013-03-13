﻿using System;
using nivax.Data;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Search Contract item template is documented at http://go.microsoft.com/fwlink/?LinkId=234240

namespace nivax
{
    // TODO: Edit the manifest to enable searching
    //
    // The package manifest could not be automatically updated.  Open the package manifest
    // file and ensure that support for activation for searching is enabled.

    // TODO: Respond to activation for search results
    //
    // The following code could not be automatically added to your application subclass,
    // either because the appropriate class could not be located or because a method with
    // the same name already exists.  Ensure that appropriate code deals with activation
    // by displaying search results for the specified search term.
    //
    //         /// <summary>
    //         /// Invoked when the application is activated to display search results.
    //         /// </summary>
    //         /// <param name="args">Details about the activation request.</param>
    //         protected async override void OnSearchActivated(Windows.ApplicationModel.Activation.SearchActivatedEventArgs args)
    //         {
    //             // TODO: Register the Windows.ApplicationModel.Search.SearchPane.GetForCurrentView().QuerySubmitted
    //             // event in OnWindowCreated to speed up searches once the application is already running
    // 
    //             // If the Window isn't already using Frame navigation, insert our own Frame
    //             var previousContent = Window.Current.Content;
    //             var frame = previousContent as Frame;
    // 
    //             // If the app does not contain a top-level frame, it is possible that this 
    //             // is the initial launch of the app. Typically this method and OnLaunched 
    //             // in App.xaml.cs can call a common method.
    //             if (frame == null)
    //             {
    //                 // Create a Frame to act as the navigation context and associate it with
    //                 // a SuspensionManager key
    //                 frame = new Frame();
    //                 nivax.Common.SuspensionManager.RegisterFrame(frame, "AppFrame");
    // 
    //                 if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
    //                 {
    //                     // Restore the saved session state only when appropriate
    //                      try
    //                     {
    //                         await nivax.Common.SuspensionManager.RestoreAsync();
    //                     }
    //                     catch (nivax.Common.SuspensionManagerException)
    //                     {
    //                         //Something went wrong restoring state.
    //                         //Assume there is no state and continue
    //                     }
    //                 }
    //             }
    // 
    //             frame.Navigate(typeof(SearchResultsPage), args.QueryText);
    //             Window.Current.Content = frame;
    // 
    //             // Ensure the current window is active
    //             Window.Current.Activate();
    //         }
    /// <summary>
    /// This page displays search results when a global search is directed to this application.
    /// </summary>
    public sealed partial class SearchResultsPage : nivax.Common.LayoutAwarePage
    {

        public SearchResultsPage()
        {
            this.InitializeComponent();
        }
        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the page showing the recipe that was clicked
            this.Frame.Navigate(typeof(ItemDetailPage), ((RecipeDataItem)e.ClickedItem).UniqueId);
        }
        // Collection of RecipeDataItem collections representing search results
        private Dictionary<string, List<RecipeDataItem>> _results = new Dictionary<string, List<RecipeDataItem>>();


        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            var queryText = navigationParameter as String;

            // TODO: Application-specific searching logic.  The search process is responsible for
            //       creating a list of user-selectable result categories:
            //
            //       filterList.Add(new Filter("<filter name>", <result count>));
            //
            //       Only the first filter, typically "All", should pass true as a third argument in
            //       order to start in an active state.  Results for the active filter are provided
            //       in Filter_SelectionChanged below.

            var filterList = new List<Filter>();
            filterList.Add(new Filter("All", 0, true));
            // Search recipes and tabulate results
            var groups = RecipeDataSource.GetGroups("AllGroups");
            string query = queryText.ToLower();
            var all = new List<RecipeDataItem>();
            _results.Add("All", all);

            foreach (var group in groups)
            {
                var items = new List<RecipeDataItem>();
                _results.Add(group.Title, items);

                foreach (var item in group.Items)
                {
                    if (item.Title.ToLower().Contains(query) || item.Directions.ToLower().Contains(query))
                    {
                        all.Add(item);
                        items.Add(item);
                    }
                }

                filterList.Add(new Filter(group.Title, items.Count, false));
            }

            filterList[0].Count = all.Count;

            // Communicate results through the view model
            this.DefaultViewModel["QueryText"] = '\u201c' + queryText + '\u201d';
            this.DefaultViewModel["Filters"] = filterList;
            this.DefaultViewModel["ShowFilters"] = filterList.Count > 1;
        }

        /// <summary>
        /// Invoked when a filter is selected using the ComboBox in snapped view state.
        /// </summary>
        /// <param name="sender">The ComboBox instance.</param>
        /// <param name="e">Event data describing how the selected filter was changed.</param>
        void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Determine what filter was selected
            var selectedFilter = e.AddedItems.FirstOrDefault() as Filter;
            if (selectedFilter != null)
            {
                // Mirror the results into the corresponding Filter object to allow the
                // RadioButton representation used when not snapped to reflect the change
                selectedFilter.Active = true;

                // TODO: Respond to the change in active filter by setting this.DefaultViewModel["Results"]
                //       to a collection of items with bindable Image, Title, Subtitle, and Description properties
                this.DefaultViewModel["Results"] = _results[selectedFilter.Name];         
                // Ensure results are found
                object results;
                ICollection resultsCollection;
                if (this.DefaultViewModel.TryGetValue("Results", out results) &&
                    (resultsCollection = results as ICollection) != null &&
                    resultsCollection.Count != 0)
                {
                    VisualStateManager.GoToState(this, "ResultsFound", true);
                    return;
                }
            }

            // Display informational text when there are no search results.
            VisualStateManager.GoToState(this, "NoResultsFound", true);
        }

        /// <summary>
        /// Invoked when a filter is selected using a RadioButton when not snapped.
        /// </summary>
        /// <param name="sender">The selected RadioButton instance.</param>
        /// <param name="e">Event data describing how the RadioButton was selected.</param>
        void Filter_Checked(object sender, RoutedEventArgs e)
        {
            // Mirror the change into the CollectionViewSource used by the corresponding ComboBox
            // to ensure that the change is reflected when snapped
            if (filtersViewSource.View != null)
            {
                var filter = (sender as FrameworkElement).DataContext;
                filtersViewSource.View.MoveCurrentTo(filter);
            }
        }

        /// <summary>
        /// View model describing one of the filters available for viewing search results.
        /// </summary>
        private sealed class Filter : nivax.Common.BindableBase
        {
            private String _name;
            private int _count;
            private bool _active;

            public Filter(String name, int count, bool active = false)
            {
                this.Name = name;
                this.Count = count;
                this.Active = active;
            }

            public override String ToString()
            {
                return Description;
            }

            public String Name
            {
                get { return _name; }
                set { if (this.SetProperty(ref _name, value)) this.OnPropertyChanged("Description"); }
            }

            public int Count
            {
                get { return _count; }
                set { if (this.SetProperty(ref _count, value)) this.OnPropertyChanged("Description"); }
            }

            public bool Active
            {
                get { return _active; }
                set { this.SetProperty(ref _active, value); }
            }

            public String Description
            {
                get { return String.Format("{0} ({1})", _name, _count); }
            }
        }
    }
}
