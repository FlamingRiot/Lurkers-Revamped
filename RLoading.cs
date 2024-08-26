using Raylib_cs;
using static Raylib_cs.Raylib;
namespace uniray_Project
{
    public class RLoading
    {
        public RLoading() 
        {
            TraceLog(TraceLogLevel.Info, "New RLoading instance launched");
        }

        public Dictionary<string, Model> LoadRigged()
        {
            Dictionary<string, Model> rigged = new Dictionary<string, Model>()
            {
                {"cop", LoadModel("../../models/cop.m3d") },
                {"officer", LoadModel("../../models/officer.m3d") },
                {"blonde", LoadModel("../../models/blonde.m3d") },
                {"yaku", LoadModel("../../models/Yaku.m3d") },
                {"torso", LoadModel("../../models/torso.m3d") }
            };

            return rigged;
        }
    }
}