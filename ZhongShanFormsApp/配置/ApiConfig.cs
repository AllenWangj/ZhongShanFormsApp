using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZhongShanFormsApp.配置
{
    public static class ApiConfig
    {
        // 替换为实际服务器 IP
        public static string BaseIp = "127.0.0.1";

        // 端口映射 (8000->8090, 以此类推)
        public static string Port_Elevation = "8090";   // 立面图读取
        public static string Port_Plan = "8091";        // 平面图读取
        public static string Port_Detail = "8092";      // 大样图读取
        public static string Port_Node = "8093";        // 节点图读取
        public static string Port_Association = "8094"; // 大样关联立面
        public static string Port_Lightweight = "8095"; // 节点轻量化
        public static string Port_Locator = "8096";     // 生成定位线
        public static string Port_Skin = "8097";        // 表皮模型生成
        public static string Port_NodeLibrary = "8098"; // 节点种子库
        public static string Port_MemberNode = "8099";  // 构件节点匹配

        // 接口路径 (严格参考 README)
        public static string Path_Elevation = "/api/v1/elevation/tasks";
        public static string Path_Plan = "/api/v1/plan/tasks";
        public static string Path_Detail = "/api/v1/detail/tasks";
        public static string Path_Node = "/api/v1/node/tasks";
        public static string Path_Association = "/api/v1/detail-facade/tasks";
        public static string Path_Lightweight = "/api/v1/node-lightweight/tasks";
        public static string Path_Locator = "/api/v1/locator/tasks";
        public static string Path_Skin = "/api/v1/skin/tasks";
        public static string Path_NodeLibrary = "/api/v1/node-library/tasks";
        public static string Path_MemberNode = "/api/v1/member-node/tasks";
    }
}
