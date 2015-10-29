using System.Collections.Generic;

namespace MyPresenter.pcoPlans
{
    public class PlansLinks
    {
        public string self { get; set; }
    }

    public class PlanAttributes
    {
        public List<object> attachment_type_ids { get; set; }
        public string created_at { get; set; }
        public string files_expire_at { get; set; }
        public int items_count { get; set; }
        public string title { get; set; }
        public string next_plan_id { get; set; }
        public int other_time_count { get; set; }
        public string permissions { get; set; }
        public int plan_notes_count { get; set; }
        public string previous_plan_id { get; set; }
        public int rehearsal_time_count { get; set; }
        public int service_time_count { get; set; }
        public int plan_people_count { get; set; }
        public int total_length { get; set; }
        public string short_dates { get; set; }
        public string sort_date { get; set; }
        public string updated_at { get; set; }
        public string last_time_at { get; set; }
        public string dates { get; set; }
        public bool @public { get; set; }
        public int needed_positions_count { get; set; }
        public int attachments_count { get; set; }
    }

    public class PlanLinks
    {
        public string self { get; set; }
    }

    public class PlansDatum
    {
        public string type { get; set; }
        public string id { get; set; }
        public PlanAttributes attributes { get; set; }
        public PlanLinks links { get; set; }
    }

    public class PlansParent
    {
        public string id { get; set; }
        public string type { get; set; }
    }

    public class PlansMeta
    {
        public int total_count { get; set; }
        public int count { get; set; }
        public List<string> can_order_by { get; set; }
        public List<string> can_query_by { get; set; }
        public List<string> can_include { get; set; }
        public List<string> can_filter { get; set; }
        public PlansParent parent { get; set; }
    }

    public class Plans
    {
        public PlansLinks links { get; set; }
        public List<PlansDatum> data { get; set; }
        public List<object> included { get; set; }
        public PlansMeta meta { get; set; }
    }
}
