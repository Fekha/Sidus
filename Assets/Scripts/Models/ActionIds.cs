using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StartaneousAPI.Models
{
    [Serializable]
    public class ActionIds
    {
        public int? actionTypeId {get;set;}
        public Guid? selectedStructureId { get; set; }
        public List<Guid>? selectedModulesIds { get; set; }
        public ActionIds(Action action)
        {
            if (action is object) {
                actionTypeId = (int)action.actionType;
                selectedStructureId = action.selectedStructure?.structureId;
                selectedModulesIds = action.selectedModules?.Select(x => x.id).ToList();
            }
        }
    }
}
