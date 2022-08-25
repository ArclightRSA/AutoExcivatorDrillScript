using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        //******************************************************************
        // -- Main
        string mainTag = "Ex/";
        // Piston Pistions
        string XTag = "X/";
        string ZTag = "Z/";
        string YTag = "Y/";

        string DrillTag = "/Dill";

        float xBlocks;
        float zBlocks;
        float yBlocks;
        float totalBlocks;

        // Custo Data Values
        float customXAxisSpeed;
        float customZAxisSpeed;
        float customYAxisSpeed;

        bool programSet;
        bool programState;
        bool LayerDone;

        bool XAxisJobActive;
        bool ZAxisJobActive;
        bool YAxisJobActive;



        List<IMyTerminalBlock> blocks;

        List<IMyPistonBase> XPistons;
        List<IMyPistonBase> ZPistons;
        List<IMyPistonBase> YPistons;

        List<IMyShipDrill> Drills;

        IMyPistonBase piston;
        IMyShipDrill drill;



        //Piston Variables
        float PMinLimit;
        float PMaxLimit;

        public Program()
        {
            Echo("Program");

            programSet = false;
            programState = false;

            // Set Values
            customXAxisSpeed = customYAxisSpeed = customZAxisSpeed = -0.4F;


            if (!getConfiguration()) setConfiguration();

            // Enable Runtime
            //Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateSource)
        {

            if (argument == "Set")
            {
                SetSystem();
            }
            else if (argument == "Reset" && programState)
            {
                Echo("Reset");
            }
            else if (argument == "Stop" && programState)
            {
                programSet = false;
                programState = false;
                Runtime.UpdateFrequency = UpdateFrequency.None;
            }
            else if (argument == "Start" && programSet)
            {
                Echo("----------");
                Echo("Starting");
                Echo("--------------");
                programState = true;
            }
            else if (argument == "X" && programState)
            {
                XAxisJobActive = false;
            }
            else if (argument == "Z" && programState)
            {
                ZAxisJobActive = false;
            }
            else if (programSet && programState)
            {
                Echo("----------");
                Echo("Running");
                Echo("--------------");
                AxisJobs();
            }
            else if (programSet)
            {
                Echo("----------");
                Echo("Setup Done - Ready to Start!");
                Echo("--------------");
                Echo("---Total Blocks per Axis ---");
                Echo("X - Axis = " + xBlocks);
                Echo("Z - Axis = " + zBlocks);
                Echo("Y - Axis = " + yBlocks);
                Echo("----------------------------");
                Echo("Total Blocks to be mined = " + totalBlocks);
            }
            else
                Echo("Setup Program First");
        }

        public void SetSystem()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            programState = false;

            Echo("----------");
            Echo("SetSystem");
            Echo("----------");

            //Piston Setup
            //PVelocity = -0.1F;
            PMinLimit = 0;
            PMaxLimit = 10;
            if (customXAxisSpeed == 0)
            {
                customXAxisSpeed = -0.4f;
            }

            XAxisJobActive = false;
            ZAxisJobActive = false;
            YAxisJobActive = false;

            LayerDone = false;

            XPistons = new List<IMyPistonBase>();
            ZPistons = new List<IMyPistonBase>();
            YPistons = new List<IMyPistonBase>();

            Drills = new List<IMyShipDrill>();

            blocks = new List<IMyTerminalBlock>();

            GridTerminalSystem.SearchBlocksOfName(mainTag, blocks);

            // Setup Lists
            try
            {
                // Set blocks in Lists
                foreach (IMyTerminalBlock b in blocks)
                {
                    if (b is IMyPistonBase)
                    {
                        piston = b as IMyPistonBase;

                        if (piston.CustomName.Contains(XTag))
                        {
                            if (!piston.GetValueBool("ShareInertiaTensor"))
                            {
                                piston.GetActionWithName("ShareInertiaTensor").Apply(piston);
                            }

                            XPistons.Add(piston);
                        }
                        else if (piston.CustomName.Contains(ZTag))
                        {
                            if (!piston.GetValueBool("ShareInertiaTensor"))
                            {
                                piston.GetActionWithName("ShareInertiaTensor").Apply(piston);
                            }

                            ZPistons.Add(piston);
                        }
                        else if (piston.CustomName.Contains(YTag))
                        {
                            if (!piston.GetValueBool("ShareInertiaTensor"))
                            {
                                piston.GetActionWithName("ShareInertiaTensor").Apply(piston);
                            }

                            YPistons.Add(piston);
                        }
                    }
                    else if (b is IMyShipDrill)
                    {
                        drill = b as IMyShipDrill;
                        drill.Enabled = true;
                        Drills.Add(drill);
                    }
                }

                // Set XPiston Varibles
                foreach (IMyPistonBase xP in XPistons)
                {
                    xP.Velocity = customXAxisSpeed / XPistons.Count;
                    xP.MinLimit = PMinLimit;
                    xP.MaxLimit = PMaxLimit;
                    xP.Retract();
                    xP.Enabled = true;
                }
                // Set ZPiston Varibles
                foreach (IMyPistonBase zP in ZPistons)
                {
                    zP.Velocity = customZAxisSpeed / ZPistons.Count;
                    zP.MinLimit = PMinLimit;
                    zP.MaxLimit = PMinLimit;
                    zP.Retract();
                    zP.Enabled = true;
                }
                // Set YPiston Varibles
                foreach (IMyPistonBase yP in YPistons)
                {
                    yP.Velocity = - customYAxisSpeed / YPistons.Count;
                    yP.MinLimit = PMinLimit;
                    yP.MaxLimit = PMinLimit;
                    yP.Retract();
                    yP.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                Echo(ex.Message);
            }
            JobCount();
            programSet = true;
        }

        public void setConfiguration()
        {
            Me.CustomData = "@Configuration\n" +
                            "You can configure the script right below here,\n" +
                            "by changing the values between then | characters.\n\n" +

                            "The configuration will be loaded if you click Check Code\n" +
                            "in the Code Editor inside the Programmable Block,\n" +

                            "Main Tag: |" + mainTag + "|\n\n" +   //2 string

                            "///////////////////////////////////////////\n" +
                            "Advance Configuration\n\n" +
                            "///////////////////////////////////////////\n" +
                            "Custom X-Axis Speed 0.0 - 5.0: |" + customXAxisSpeed + "|\n" +    //4 Float
                            "Custom Z-Axis Speed 0.0 - 5.0: |" + customZAxisSpeed + "|\n" +    //6 Float
                            "Custom Y-Axis Speed 0.0 - 5.0: |" + customYAxisSpeed + "|\n";    //8 Float

            Echo("Configuration Set to Custom Data!");
        }

        public bool getConfiguration()
        {
            if (Me.CustomData.StartsWith("@Configuration"))
            {
                string[] config = Me.CustomData.Split('|');
                if (config.Length == 10)
                {
                    bool result = true;
                    mainTag = config[2];

                    if (!float.TryParse(config[4], out customXAxisSpeed)) { Echo("Getting use_auto_pause failed!"); result = false; }
                    if (!float.TryParse(config[6], out customZAxisSpeed)) { Echo("Getting cargo_high_limit failed!"); result = false; }
                    if (!float.TryParse(config[8], out customYAxisSpeed)) { Echo("Getting cargo_high_limit failed!"); result = false; }
                    if (result)
                    {
                        Echo("Configuration Done!");
                        return true;
                    }
                    else
                    {
                        Echo("Configuration Error!");
                        return false;
                    }
                }
                else
                {
                    Echo("Getting Configuration failed!");
                    return false;
                }
            }
            else
            {
                Echo("Getting Configuration failed!");
                return false;
            }
        }

        public void JobCount()
        {
            xBlocks = (float)((10 / 2.5) * XPistons.Count);
            zBlocks = (float)((10 / 2.5) * ZPistons.Count);
            yBlocks = (float)((10 / 2.5) * YPistons.Count);
            totalBlocks = xBlocks * zBlocks * yBlocks;
        }

        public void AxisJobs()
        {
            Echo("---X---");
            Echo("X - Status: " + XPistons[0].Status);
            Echo("X - Current Pos : " + XPistons[0].CurrentPosition);
            Echo("Z - Status: " + XPistons[0].Status);
            Echo("Z - Current Pos : " + ZPistons[0].CurrentPosition);
            Echo("Y - Status: " + XPistons[0].Status);
            Echo("Y - Current Pos : " + YPistons[0].CurrentPosition);
            if (LayerDone)
            {
                YAxisJob();
            }
            else if (!LayerDone)
            {
                XAxisJob();
            }
        }

        public void XAxisJob()
        {
            if (XAxisJobActive && (XPistons[0].Status == PistonStatus.Retracted || XPistons[0].Status == PistonStatus.Extended))
            {
                ZAxisJob();
            }
            else if (!XAxisJobActive)
            {
                XAxisJobActive = true;
                if (XPistons[0].Status == PistonStatus.Retracted)
                {
                    //XAxisJobActive = true;
                    //Extend
                    foreach (IMyPistonBase xP in XPistons)
                    {
                        xP.Extend();
                    }
                }
                else if (XPistons[0].Status == PistonStatus.Extended)
                {
                    //Retract
                    foreach (IMyPistonBase xP in XPistons)
                    {
                        xP.Retract();
                    }
                }
            }
        }

        public void ZAxisJob()
        {

            if (ZPistons[0].MaxLimit == 10 && (ZPistons[0].Status == PistonStatus.Retracted || ZPistons[0].Status == PistonStatus.Extended) && XPistons[0].Status == PistonStatus.Retracted)
            {
                LayerDone = true;
                ZAxisJobActive = false;

                ResetAxisZ();
            }
            else if (ZAxisJobActive && (ZPistons[0].Status == PistonStatus.Retracted || ZPistons[0].Status == PistonStatus.Extended))
            {
                ZAxisJobActive = false;
                XAxisJobActive = false;
            }
            else if (!ZAxisJobActive)
            {
                foreach (IMyPistonBase p in ZPistons)
                {
                    ZAxisJobActive = true;
                    p.MaxLimit += 2;
                    p.Extend();
                }
            }
        }

        public void YAxisJob()
        {
            if (YAxisJobActive && YPistons[0].MaxLimit == YPistons[0].CurrentPosition && YPistons[0].Status == PistonStatus.Extended)
            {
                LayerDone = false;
                XAxisJobActive = false;
                ZAxisJobActive = false;
                YAxisJobActive = false;
            }
            else if (!YAxisJobActive && ZPistons[0].MaxLimit == ZPistons[0].CurrentPosition)
            {
                foreach (IMyPistonBase yP in YPistons)
                {
                    YAxisJobActive = true;
                    yP.MaxLimit += 2;
                    yP.Extend();
                }
            }
        }

        public void ResetAxisZ()
        {
            ZAxisJobActive = true;
            XAxisJobActive = true;

            foreach (IMyPistonBase zP in ZPistons)
            {
                zP.MaxLimit = 0;
                zP.Retract();
            }
            /*foreach (IMyShipDrill d in Drills)
            {
                d.Enabled = false;
            }*/
        }
        //******************************************************************
    }
}
