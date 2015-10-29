using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPresenter.pcoPlan
{
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
        public string attachments { get; set; }
        public string items { get; set; }
        public string needed_positions { get; set; }
        public string next_plan { get; set; }
        public string notes { get; set; }
        public string plan_times { get; set; }
        public string previous_plan { get; set; }
        public object series { get; set; }
        public string team_member { get; set; }
        public string self { get; set; }
    }

    public class PlanData
    {
        public string type { get; set; }
        public string id { get; set; }
        public PlanAttributes attributes { get; set; }
        public PlanLinks links { get; set; }
    }

    public class PlanParent
    {
        public string id { get; set; }
        public string type { get; set; }
    }

    public class PlanMeta
    {
        public List<string> can_include { get; set; }
        public PlanParent parent { get; set; }
    }

    public class Plan
    {
        public PlanData data { get; set; }
        public List<object> included { get; set; }
        public PlanMeta meta { get; set; }
    }
}
