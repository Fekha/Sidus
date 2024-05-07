using System;
using System.Collections.Generic;
using System.Linq;

namespace StartaneousAPI.Models
{
    [Serializable]
    public class ActionIds
    {
        public int? actionTypeId {get;set;}
        public Guid? selectedUnitId { get; set; }
        public List<Guid>? selectedModulesIds { get; set; }
        public List<Coords> selectedCoords { get; set; }
        public int generatedModuleId { get; set; }
        public Guid generatedGuid { get; set; }

        public ActionIds(Action action)
        {
            if (action is object) {
                actionTypeId = (int)action.actionType;
                selectedUnitId = action.selectedUnit?.unitGuid;
                selectedModulesIds = action.selectedModulesIds;
                selectedCoords = action.selectedPath.Select(x=>new Coords(x.x, x.y)).ToList();
                generatedGuid = action.generatedGuid;
                generatedModuleId = action.generatedModuleId;
            }
        }
    }
}
