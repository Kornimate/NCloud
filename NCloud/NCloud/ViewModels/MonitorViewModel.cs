using NCloud.Models;

namespace NCloud.ViewModels
{
    public class MonitorViewModel
    {
        public List<CloudLineGraphPointModel> LineGraphPoints = new();
        public List<CloudHeatMapPointModel> HeatMapPoints = new();
    }
}
