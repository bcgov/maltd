using System;
using System.Collections;
using System.Collections.Generic;

namespace BcGov.Malt.Web.Models.Configuration
{
    public class ProjectConfigurationCollection : IEnumerable<ProjectConfiguration>
    {
        private Dictionary<string, ProjectConfiguration> _items =
            new Dictionary<string, ProjectConfiguration>(StringComparer.OrdinalIgnoreCase);

        public int Count => _items.Count;

        public ProjectConfigurationCollection()
        {
        }

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

        public void Add(ProjectConfiguration item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            _items.Add(item.Name, item);
        }

        public bool Contains(ProjectConfiguration item)
        {
            return _items.ContainsKey(item.Name);
        }


        public IEnumerator<ProjectConfiguration> GetEnumerator()
        {
            return _items.Values.GetEnumerator();
        }

        public bool Remove(ProjectConfiguration item)
        {
            return _items.Remove(item.Name);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.Values.GetEnumerator();
        }
    }
}
