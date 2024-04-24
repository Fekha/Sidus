using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StartaneousAPI.Models
{
    public class ActionIds
    {
        public int actionTypeId;
        public Guid? selectedStructureId;
        public List<int>? selectedModulesIds;
        public ActionIds(Action action)
        {
            actionTypeId = (int)action.actionType;
            selectedStructureId = action.selectedStructure?.structureId;
            selectedModulesIds = action.selectedModules?.Select(x=>x.id).ToList();
        }
    }
}
