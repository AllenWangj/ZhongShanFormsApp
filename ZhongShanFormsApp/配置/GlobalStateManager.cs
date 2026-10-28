using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZhongShanFormsApp.配置
{
    public static class GlobalStateManager
    {
        public static string ElevationTaskId { get; set; } = "";
        public static string PlanTaskId { get; set; } = "";
        public static string DetailTaskId { get; set; } = "";
        public static string NodeTaskId { get; set; } = "";
        public static string AssociationTaskId { get; set; } = "";
        public static string LocatorTaskId { get; set; } = "";
        public static string NodeLibraryTaskId { get; set; } = "";
    }
}
