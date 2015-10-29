using System.Collections.Generic;

namespace MyPresenter.pcoItems
{
    public class ItemsLinks
    {
        public string self { get; set; }
    }

    public class ItemAttributes
    {
        public string title { get; set; }
        public int sequence { get; set; }
        public string service_position { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string item_type { get; set; }
        public int length { get; set; }
        public string song_id { get; set; }
        public string description { get; set; }
    }

    public class ItemLinks
    {
        public string self { get; set; }
    }

    public class ItemsDatum
    {
        public string type { get; set; }
        public string id { get; set; }
        public ItemAttributes attributes { get; set; }
        public ItemLinks links { get; set; }
    }

    public class ItemsParent
    {
        public string id { get; set; }
        public string type { get; set; }
    }

    public class ItemsMeta
    {
        public int total_count { get; set; }
        public int count { get; set; }
        public List<string> can_include { get; set; }
        public ItemsParent parent { get; set; }
    }

    public class Items
    {
        public ItemsLinks links { get; set; }
        public List<ItemsDatum> data { get; set; }
        public List<object> included { get; set; }
        public ItemsMeta meta { get; set; }
    }
}
