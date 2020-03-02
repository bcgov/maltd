using System;
using System.Collections;
using System.Collections.Generic;

namespace BcGov.Malt.Web.Models.Configuration
{
    public class ProjectConfigurationCollection : IEnumerable<ProjectConfiguration>
    {
        private Dictionary<string, ProjectConfiguration> _items =
            new Dictionary<string, ProjectConfiguration>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectConfigurationCollection"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <exception cref="ArgumentNullException"><paramref name="items"/> is null.</exception>
        /// <exception cref="ArgumentException">An more than one item has the same name.</exception>
        public ProjectConfigurationCollection(IEnumerable<ProjectConfiguration> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            foreach (var item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is null.</exception>
        /// <exception cref="ArgumentException">An item with the same name has already been added.</exception>
        private void Add(ProjectConfiguration item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            _items.Add(item.Name, item);
        }

        private bool Contains(ProjectConfiguration item)
        {
            return _items.ContainsKey(item.Name);
        }


        public IEnumerator<ProjectConfiguration> GetEnumerator()
        {
            return _items.Values.GetEnumerator();
        }

        private bool Remove(ProjectConfiguration item)
        {
            return _items.Remove(item.Name);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.Values.GetEnumerator();
        }
    }
}
